using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crawler.Core;
using Crawler.Engines;
using Crawler.Entities;
using Crawler.Services.DbService;
using PuppeteerSharp; // 引入 PuppeteerSharp

namespace Crawler.Views
{
    public class Crawl : UserControl
    {
        private TextBox _txtUrl;
        private DataGridView _grid;
        private ProgressBar _progress;
        private Label _lblStatus;
        private Button _btnCancel;
        private List<ScanResult> _data = new();
        private CancellationTokenSource _cts;

        public void InitUi()
        {
            this.Controls.Clear();
            this.Padding = new Padding(20);

            // --- 顶部搜索区 ---
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
            _txtUrl = new TextBox { Location = new System.Drawing.Point(0, 15), Width = 500, Font = new Font("Consolas", 12) };
            var btnScan = new Button { Text = "解析页面资源", Location = new System.Drawing.Point(510, 12), Width = 120, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnScan.Click += async (s, e) => await HandleScan();

            searchPanel.Controls.Add(_txtUrl);
            searchPanel.Controls.Add(btnScan);

            // --- 中间表格区 ---
            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                GridColor = Color.FromArgb(236, 240, 241)
            };
            _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(236, 240, 241), ForeColor = Color.Black, Font = new Font("微软雅黑", 10, FontStyle.Bold), Padding = new Padding(5) };

            // --- 底部操作区 ---
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 100 };
            _progress = new ProgressBar { Dock = DockStyle.Top, Height = 15 };
            _lblStatus = new Label { Text = "准备就绪", Top = 30, Left = 0, AutoSize = true };

            var btnDown = new Button { Text = "下载选中项", Top = 25, Left = 500, Width = 120, Height = 35, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White };
            btnDown.Click += async (s, e) => await HandleDownload();

            _btnCancel = new Button { Text = "取消任务", Top = 25, Left = 630, Width = 100, Height = 35, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, Enabled = false };
            _btnCancel.Click += (s, e) => _cts?.Cancel();

            bottomPanel.Controls.Add(_progress);
            bottomPanel.Controls.Add(_lblStatus);
            bottomPanel.Controls.Add(btnDown);
            bottomPanel.Controls.Add(_btnCancel);

            this.Controls.Add(_grid);
            this.Controls.Add(searchPanel);
            this.Controls.Add(bottomPanel);
        }

        // --- 核心：真正的解析逻辑（已升级为无头浏览器动态渲染） ---
        private async Task HandleScan()
        {
            string url = _txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            _data.Clear();
            _grid.DataSource = null;

            try
            {
                _lblStatus.Text = "正在检查浏览器环境 (首次运行可能需下载内核)...";
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                _lblStatus.Text = "正在启动浏览器引擎，加载动态页面...";

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
                using var page = await browser.NewPageAsync();

                await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                // 等待网络空闲（Networkidle2），确保 JS 渲染完毕，图片懒加载完成
                await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });

                // 获取执行 JS 后的真实 HTML
                string html = await page.GetContentAsync();

                // 1. 提取网页标题
                string title = Regex.Match(html, @"<title>(.*?)</title>", RegexOptions.IgnoreCase).Groups[1].Value.Trim();
                if (string.IsNullOrEmpty(title)) title = "未知网页";
                title = string.Join("_", title.Split(Path.GetInvalidFileNameChars())); // 清理非法字符

                // 2. 添加 视频 和 文本 (整页处理)
                _data.Add(new ScanResult { Title = $"[视频/音频] {title}", Type = "Video", Url = url, Extension = ".mp4" });
                _data.Add(new ScanResult { Title = $"[文本] {title}_文章", Type = "Text", Url = url, Extension = ".txt" });

                // 3. 提取所有图片并单独列出 (兼容 src 和 data-src懒加载)
                int imgCount = 1;
                var pattern = @"<(?:img|source)[^>]+(?:src|data-src|data-original|srcset)\s*=\s*[""']([^""' >]+)[""']";
                var imgMatches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase);

                foreach (Match m in imgMatches)
                {
                    string imgUrl = m.Groups[1].Value.Split(' ')[0]; // 处理 srcset 中的多余参数

                    if (!imgUrl.StartsWith("http"))
                    {
                        imgUrl = new Uri(new Uri(url), imgUrl).ToString();
                    }

                    if (imgUrl.StartsWith("data:image")) continue;

                    _data.Add(new ScanResult
                    {
                        Title = $"[图片] {title}_图{imgCount++}",
                        Type = "Image",
                        Url = imgUrl,
                        Extension = ".jpg"
                    });
                }

                _grid.DataSource = _data;
                _lblStatus.Text = $"解析完成。发现 {imgCount - 1} 张图片，以及媒体/文本资源。";

            }
            catch (Exception ex)
            {
                _lblStatus.Text = "解析失败: " + ex.Message;
            }
        }

        // --- 核心：完整的下载流程 ---
        private async Task HandleDownload()
        {
            if (_grid.SelectedRows.Count == 0) return;

            var item = _data[_grid.SelectedRows[0].Index];
            bool isAudioOnly = false;
            string finalExt = item.Extension;

            if (item.Type == "Video")
            {
                var formatChoice = MessageBox.Show("是否仅提取音频 (MP3)？\n\n[是] 下载音频\n[否] 下载视频", "yt-dlp参数设置", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (formatChoice == DialogResult.Cancel) return;
                isAudioOnly = formatChoice == DialogResult.Yes;
                finalExt = isAudioOnly ? ".mp3" : ".mp4";
            }

            using var sfd = new SaveFileDialog
            {
                Title = "选择保存路径并修改文件名",
                FileName = item.Title.Replace("[视频/音频] ", "").Replace("[图片] ", "").Replace("[文本] ", "") + finalExt,
                Filter = $"资源文件|*{finalExt}|所有文件|*.*"
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            string savePath = sfd.FileName;
            string finalTitle = Path.GetFileNameWithoutExtension(savePath);

            ICrawlEngine engine = item.Type switch
            {
                "Video" => new MediaEngine(isAudioOnly),
                "Image" => new ImageEngine(),
                _ => new TextEngine()
            };

            _cts = new CancellationTokenSource();
            _btnCancel.Enabled = true;

            var prog = new Progress<ProgressInfo>(i =>
            {
                // 使用 Invoke 确保在 UI 线程更新
                this.Invoke((MethodInvoker)delegate
                {
                    _progress.Value = Math.Max(0, Math.Min(100, i.Percent));
                    _lblStatus.Text = i.Message;
                });
            });

            try
            {
                await engine.StartAsync(item.Url, savePath, prog, _cts.Token);

                if (!_cts.IsCancellationRequested)
                {
                    var fileInfo = new FileInfo(savePath);
                    string sizeStr = fileInfo.Exists ? (fileInfo.Length / 1024.0 / 1024.0).ToString("0.00") + " MB" : "未知";

                    new FileDbService().SaveFile(new FileEntity
                    {
                        Title = finalTitle,
                        Url = item.Url,
                        LocalPath = savePath,
                        Type = isAudioOnly ? "Audio" : item.Type,
                        OriginalName = item.Title + finalExt,
                        FileSize = sizeStr,
                        DownloadTime = DateTime.Now
                    });

                    _lblStatus.Text = "下载并归档成功！";
                    MessageBox.Show("下载成功并已存入数据库！");
                }
            }
            catch (OperationCanceledException) { _lblStatus.Text = "已手动取消任务。"; }
            catch (Exception ex) { _lblStatus.Text = "出错: " + ex.Message; }
            finally
            {
                _btnCancel.Enabled = false;
                _cts?.Dispose();
                _progress.Value = 0;
            }
        }
    }
}