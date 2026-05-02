using System;
using System.Collections.Generic;
using System.ComponentModel; // 引入这个命名空间
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crawler.Core;
using Crawler.Engines;
using Crawler.Entities;
using Crawler.Services.DbService;
using PuppeteerSharp; 

namespace Crawler.Views
{
    public class Crawl : UserControl
    {
        private TextBox _txtUrl;
        private DataGridView _grid;
        private ProgressBar _progress;
        private Label _lblStatus;
        private Button _btnCancel;
        
        // 核心修复1：使用 BindingList，这是 WinForms 动态刷新表格的标准数据结构
        private BindingList<ScanResult> _data = new BindingList<ScanResult>();
        private CancellationTokenSource _cts;

        public void InitUi()
        {
            this.Controls.Clear();
            this.Padding = new Padding(20);

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
            _txtUrl = new TextBox { Location = new System.Drawing.Point(0, 15), Width = 500, Font = new Font("Consolas", 12) };
            var btnScan = new Button { Text = "解析页面资源", Location = new System.Drawing.Point(510, 12), Width = 120, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnScan.Click += async (s, e) => await HandleScan();

            searchPanel.Controls.Add(_txtUrl);
            searchPanel.Controls.Add(btnScan);

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

            // 核心修复2：只需绑定一次，后续调用 _data.Add 界面会自动刷新
            _grid.DataSource = _data; 

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

        private async Task HandleScan()
        {
            string url = _txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            // 清空列表即可，界面会自动刷新
            _data.Clear(); 

            try
            {
                _lblStatus.Text = "正在检查浏览器环境...";
                var browserFetcher = new BrowserFetcher();
                await browserFetcher.DownloadAsync();

                _lblStatus.Text = "正在启动浏览器引擎...";

                using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { 
                    Headless = true,
                    UserDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BrowserData")
                });
                
                using var page = await browser.NewPageAsync();
                await page.EvaluateFunctionOnNewDocumentAsync(@"() => { Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); }");
                await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

                await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });

                // 核心修复3：注入 JS 模拟真人向下滚动网页，把懒加载的图片和文本逼出来！
                _lblStatus.Text = "正在模拟鼠标滚动，加载隐藏资源...";
                await page.EvaluateFunctionAsync(@"async () => {
                    await new Promise((resolve) => {
                        let totalHeight = 0;
                        let distance = 300;
                        let timer = setInterval(() => {
                            window.scrollBy(0, distance);
                            totalHeight += distance;
                            // 滚到底部或者最多滚 15000 像素后停止
                            if(totalHeight >= document.body.scrollHeight || totalHeight > 15000){ 
                                clearInterval(timer);
                                resolve();
                            }
                        }, 100);
                    });
                }");

                await Task.Delay(1000); // 滚完后等 1 秒让图片加载完

                string title = await page.EvaluateExpressionAsync<string>("document.title");
                if (string.IsNullOrEmpty(title)) title = "未知网页";
                title = string.Join("_", title.Split(Path.GetInvalidFileNameChars()));

                _data.Add(new ScanResult { Title = $"[文本] {title}_正文", Type = "Text", Url = url, Extension = ".txt" });

                bool hasVideo = await page.EvaluateExpressionAsync<bool>("document.querySelectorAll('video, iframe').length > 0");
                if (hasVideo || url.Contains("bilibili") || url.Contains("youtube") || url.Contains("v.qq"))
                {
                    _data.Add(new ScanResult { Title = $"[视频/音频] {title}", Type = "Video", Url = url, Extension = ".mp4" });
                }

                // 核心修复4：兼容更多前端框架的图片懒加载属性 (v-lazy, lazy-src 等)
                string jsCode = @"
                    Array.from(document.querySelectorAll('img'))
                         .map(img => img.src || img.getAttribute('data-src') || img.getAttribute('data-original') || img.getAttribute('v-lazy') || img.getAttribute('lazy-src'))
                         .filter(src => src && !src.startsWith('data:image'))
                ";
                var imgUrls = await page.EvaluateExpressionAsync<string[]>(jsCode);

                var uniqueUrls = new HashSet<string>(imgUrls);
                int imgCount = 1;
                int realAddCount = 0; // 记录真正添加到表格里的数量

                foreach (string imgUrl in uniqueUrls)
                {
                    if (imgUrl.ToLower().Contains("onetrust")) continue;

                    string finalImgUrl = imgUrl;
                    if (!finalImgUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        if (Uri.TryCreate(new Uri(url), finalImgUrl, out Uri absUri))
                        {
                            finalImgUrl = absUri.ToString();
                        }
                        else continue;
                    }

                    string ext = ".jpg";
                    string lowerUrl = finalImgUrl.ToLower();
                    if (lowerUrl.Contains(".webp") || lowerUrl.Contains("format=webp") || lowerUrl.Contains("format/webp")) ext = ".webp";
                    else if (lowerUrl.Contains(".png") || lowerUrl.Contains("format=png")) ext = ".png";
                    else if (lowerUrl.Contains(".gif") || lowerUrl.Contains("format=gif")) ext = ".gif";

                    _data.Add(new ScanResult
                    {
                        Title = $"[图片] {title}_图{imgCount++}",
                        Type = "Image",
                        Url = finalImgUrl,
                        Extension = ext 
                    });
                    realAddCount++;
                }

                _lblStatus.Text = $"解析完成。成功抓取到 {realAddCount} 张图片以及媒体/文本。";
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "解析失败: " + ex.Message;
            }
        }

        // --- HandleDownload 方法保持上一版的代码不变即可 ---
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
                this.Invoke((MethodInvoker)delegate
                {
                    _progress.Value = Math.Max(0, Math.Min(100, i.Percent));
                    _lblStatus.Text = i.Message;
                });
            });

            try
            {
                string actualSavedPath = await engine.StartAsync(item.Url, savePath, prog, _cts.Token);

                if (!_cts.IsCancellationRequested)
                {
                    var fileInfo = new FileInfo(actualSavedPath);
                    string sizeStr = fileInfo.Exists ? (fileInfo.Length / 1024.0 / 1024.0).ToString("0.00") + " MB" : "未知";
                    string finalTitle = Path.GetFileNameWithoutExtension(actualSavedPath); 

                    new FileDbService().SaveFile(new FileEntity
                    {
                        Title = finalTitle,
                        Url = item.Url,
                        LocalPath = actualSavedPath, 
                        Type = isAudioOnly ? "Audio" : item.Type,
                        OriginalName = Path.GetFileName(actualSavedPath),
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