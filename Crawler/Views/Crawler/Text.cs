using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crawler.Engines;
using Crawler.Entities;
using Crawler.Services.DbService;

namespace Crawler.Views
{
    public class TextCrawl : UserControl
    {
        private TextBox _txtUrl;//url
        private TextBox _txtPreview;//预览框
        private Label _lblStatus;//状态
        private Button _btnDown;//下载
        private string _currentTitle = "未命名文本";//初始化标题

        public void InitUi()
        {
            this.Controls.Clear();//清空控件
            this.Padding = new Padding(20);//内边距
            //顶部搜索栏
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
            _txtUrl = new TextBox { Location = new Point(0, 15), Width = 500, Font = new Font("Consolas", 12) };
            var btnScan = new Button { Text = "嗅探文章正文", Location = new Point(510, 12), Width = 130, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnScan.Click += async (s, e) => await HandleScan();
            searchPanel.Controls.Add(_txtUrl);
            searchPanel.Controls.Add(btnScan);
            //中间的预览栏
            _txtPreview = new TextBox {
                Dock = DockStyle.Fill,
                Multiline = true,//多行
                ScrollBars = ScrollBars.Vertical,//垂直滚动条
                Font = new Font("微软雅黑", 12),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,//无边框
                Margin = new Padding(0, 20, 0, 20),
                Text = "\n\n\n\n\n请在上方输入网址进行嗅探，结果将在此处预览...\n您可以直接在此处修改文字后再保存。",
                ForeColor = Color.Gray
            };
            //底部状态栏
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            _lblStatus = new Label { Text = "准备就绪", Top = 25, Left = 0, AutoSize = true };
            _btnDown = new Button { Text = "保存至本地", Top = 20, Left = 650, Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Enabled = false };
            _btnDown.Click += HandleDownload;
            bottomPanel.Controls.Add(_lblStatus);
            bottomPanel.Controls.Add(_btnDown);

            this.Controls.Add(_txtPreview);
            this.Controls.Add(searchPanel);
            this.Controls.Add(bottomPanel);
        }

        private async Task HandleScan()
        {
            string url = _txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            _lblStatus.Text = "正在启动无头浏览器提取动态正文...";
            _txtPreview.Text = "正在解析中，请稍候...";
            _txtPreview.ForeColor = Color.Black;

            try
            {
                var engine = new TextEngine();
                var prog = new Progress<Crawler.Core.ProgressInfo>(i => _lblStatus.Text = i.Message);//回传信息
                //使用定义好的方法获取文本信息
                string extractedText = await engine.ExtractTextAsync(url, prog, new System.Threading.CancellationToken());
                //直接显示
                _txtPreview.Text = extractedText;
                // 从第一行提取标题作为默认保存的文件名
                var lines = _txtPreview.Text.Split('\n');
                if(lines.Length > 0) _currentTitle = lines[0].Replace("标题: ", "").Trim();
                _btnDown.Enabled = true;
                _lblStatus.Text = "操作成功，您可以预览、编辑并保存。";
            }
            catch (Exception ex) { _lblStatus.Text = "出错: " + ex.Message; }
        }

        private void HandleDownload(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Title = "保存文本", FileName = _currentTitle + ".txt", Filter = "文本文件|*.txt" };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            //写入硬盘
            File.WriteAllText(sfd.FileName, _txtPreview.Text);

            var fileInfo = new FileInfo(sfd.FileName);
            new FileDbService().SaveFile(new FileEntity {
                Title = Path.GetFileNameWithoutExtension(sfd.FileName),
                Url = _txtUrl.Text,
                LocalPath = sfd.FileName,
                Type = "Text",
                OriginalName = Path.GetFileName(sfd.FileName),
                FileSize = (fileInfo.Length / 1024.0).ToString("0.00") + " KB",
                DownloadTime = DateTime.Now
            });

            MessageBox.Show("保存并归档成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}