using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crawler.Entities;
using Crawler.Engines;
using Crawler.Services.DbService;
using PuppeteerSharp;
using Microsoft.Web.WebView2.WinForms;

namespace Crawler.Views
{
    public class MediaCrawl : UserControl//控件
    {
        private TextBox _txtUrl;//url
        private Label _lblStatus;//状态
        private Panel _previewCard;//预览panel
        private Button _btnDown;//下载
        private string _pageTitle;//页面标题
        private WebView2 _webViewPlayer;//预览用实例控件

        public void InitUi()
        {
            this.Controls.Clear();//清空控件
            this.Padding = new Padding(20);//内边距
            //顶部搜索栏
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
            _txtUrl = new TextBox { Location = new System.Drawing.Point(0, 15), Width = 500, Font = new Font("Consolas", 12) };
            var btnScan = new Button { Text = "嗅探媒体源", Location = new System.Drawing.Point(510, 12), Width = 130, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnScan.Click += async (s, e) => await HandleScan();
            searchPanel.Controls.Add(_txtUrl);
            searchPanel.Controls.Add(btnScan);
            //中间的预览
            _previewCard = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black, Visible = false };
            _webViewPlayer = new WebView2
            {
                Dock = DockStyle.Fill,
                DefaultBackgroundColor = Color.Black
            };
            _previewCard.Controls.Add(_webViewPlayer);
            //底部状态栏
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            _lblStatus = new Label { Text = "准备就绪", Top = 25, Left = 0, AutoSize = true };
            _btnDown = new Button { Text = "下载", Top = 20, Left = 650, Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Enabled = false };
            _btnDown.Click += async (s, e) => await HandleDownload();
            bottomPanel.Controls.Add(_lblStatus);
            bottomPanel.Controls.Add(_btnDown);

            this.Controls.Add(_previewCard);
            this.Controls.Add(searchPanel);
            this.Controls.Add(bottomPanel);
        }

        private async Task HandleScan()
        {
            string url = _txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            if (_webViewPlayer == null)
            {
                MessageBox.Show("错误：WebView2 对象未实例化，请检查 InitUi 是否运行。");
                return;
            }

            _lblStatus.Text = "正在准备预览播放器...";
            try
            {
                //确保WebView2已初始化
                await _webViewPlayer.EnsureCoreWebView2Async();
                //显示播放器卡片
                _previewCard.Visible = true;
                //临时给个名字
                _pageTitle = "视频资源_" + DateTime.Now.ToString("HHmmss");
                //处理 URI
                Uri targetUri = new Uri(url);
                _webViewPlayer.Source = targetUri;
                //等页面加载完，清理无关元素，并获取真实标题
                _webViewPlayer.NavigationCompleted += async (s, e) =>
                {
                    if (e.IsSuccess)
                    {
                        //利用WebView2提取网页真实标题，供下载重命名使用
                        _pageTitle = _webViewPlayer.CoreWebView2.DocumentTitle ?? "未知视频源";
                        _pageTitle = string.Join("_", _pageTitle.Split(Path.GetInvalidFileNameChars()));
                        //注入JS尽可能隐藏侧边栏，评论区
                        string cleanJs = @"
                    (function() {
                        const selectors = ['.international-header', '.right-container', '.comment-m', '.footer', '.bili-header'];
                        selectors.forEach(s => {
                            const el = document.querySelector(s);
                            if(el) el.style.display = 'none';
                        });
                        const leftContainer = document.querySelector('.left-container');
                        if(leftContainer) leftContainer.style.width = '100%';
                        const player = document.querySelector('#bilibili-player');
                        if(player) player.style.width = '100%';
                    })();//外层的()表示变量局部有效，最后的()表示立即执行
                ";
                        await _webViewPlayer.ExecuteScriptAsync(cleanJs);
                        _lblStatus.Text = "预览就绪";
                    }
                    else
                    {
                        _lblStatus.Text = "网页加载中断或失败。";
                    }
                };

                _btnDown.Enabled = true;
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "预览加载失败: " + ex.Message;
            }
        }

        private async Task HandleDownload()
        {
            var formatChoice = MessageBox.Show("是否仅提取音频 (MP3)？\n\n[是] 下载音频\n[否] 下载视频", "参数设置", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (formatChoice == DialogResult.Cancel) return;
            bool isAudioOnly = formatChoice == DialogResult.Yes;
            string finalExt = isAudioOnly ? ".mp3" : ".mp4";
            string safeTitle = string.Concat(_pageTitle.Split(Path.GetInvalidFileNameChars()));
            using var sfd = new SaveFileDialog
            {
                Title = "保存媒体",
                //使用清理后的标题
                FileName = safeTitle + finalExt,
                Filter = $"{finalExt.Trim('.').ToUpper()} 文件 (*{finalExt})|*{finalExt}|所有文件 (*.*)|*.*"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                var engine = new MediaEngine(isAudioOnly);
                var prog = new Progress<Crawler.Core.ProgressInfo>(i => _lblStatus.Text = i.Message);//子进程report之后执行这里的代码

                _btnDown.Enabled = false;
                string finalPath = await engine.StartAsync(_txtUrl.Text, sfd.FileName, prog, new System.Threading.CancellationToken());

                var fileInfo = new FileInfo(finalPath);
                new FileDbService().SaveFile(new FileEntity
                {
                    Title = Path.GetFileNameWithoutExtension(finalPath),
                    Url = _txtUrl.Text,
                    LocalPath = finalPath,
                    Type = "Video",
                    OriginalName = Path.GetFileName(finalPath),
                    FileSize = (fileInfo.Length / 1024.0 / 1024.0).ToString("0.00") + " MB",
                    DownloadTime = DateTime.Now
                });
                MessageBox.Show("媒体下载并归档成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("下载出错: " + ex.Message); }
            finally { _btnDown.Enabled = true; }
        }
    }
}