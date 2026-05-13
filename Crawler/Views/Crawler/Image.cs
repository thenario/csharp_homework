using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Crawler.Entities;
using Crawler.Services.DbService;
using PuppeteerSharp;
using Microsoft.Web.WebView2.WinForms; // 必须引用

namespace Crawler.Views
{
    public class ImageCrawl : UserControl
    {
        private TextBox _txtUrl;
        private Label _lblStatus;
        private Button _btnDownCurrent;
        private WebView2 _webViewPreview; // 核心：替换 PictureBox
        private Label _lblPage;
        private Button _btnPrev;
        private Button _btnNext;
        
        private List<string> _imgUrls = new List<string>();
        private int _currentIndex = 0;
        private string _pageTitle = "图片资源";

        private IBrowser _browser; 
        private IPage _mainPage; 

        private byte[] _currentImageBytes = null;
        private string _currentImageExt = ".jpg";

        public void InitUi()
        {
            this.Controls.Clear();
            this.Padding = new Padding(20);

            // 上方搜索栏
            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60 };
            _txtUrl = new TextBox { Location = new System.Drawing.Point(0, 15), Width = 500, Font = new Font("Consolas", 12) };
            var btnScan = new Button { Text = "嗅探高清图库", Location = new System.Drawing.Point(510, 12), Width = 130, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnScan.Click += async (s, e) => await HandleScan();
            searchPanel.Controls.Add(_txtUrl); searchPanel.Controls.Add(btnScan);

            // 中间预览区 (WebView2)
            var galleryPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(236, 240, 241) };
            _webViewPreview = new WebView2 { 
                Dock = DockStyle.Fill, 
                DefaultBackgroundColor = Color.Black 
            };
            galleryPanel.Controls.Add(_webViewPreview);

            // 翻页控制条
            var controlsPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            _btnPrev = new Button { Text = "◀ 上一张", Location = new System.Drawing.Point(20, 10), Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            _btnNext = new Button { Text = "下一张 ▶", Location = new System.Drawing.Point(220, 10), Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            _lblPage = new Label { Text = "0 / 0", Location = new System.Drawing.Point(130, 18), Width = 80, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("微软雅黑", 12, FontStyle.Bold) };
            
            _btnPrev.Click += async (s, e) => { if (_currentIndex > 0) { _currentIndex--; await ShowCurrentPreviewAsync(); } };
            _btnNext.Click += async (s, e) => { if (_currentIndex < _imgUrls.Count - 1) { _currentIndex++; await ShowCurrentPreviewAsync(); } };
            
            controlsPanel.Controls.Add(_btnPrev); controlsPanel.Controls.Add(_lblPage); controlsPanel.Controls.Add(_btnNext);
            galleryPanel.Controls.Add(controlsPanel);

            // 底部状态栏
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            _lblStatus = new Label { Text = "准备就绪", Top = 25, Left = 0, AutoSize = true };
            _btnDownCurrent = new Button { Text = "下载当前预览图", Top = 20, Left = 650, Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Enabled = false };
            _btnDownCurrent.Click += HandleDownload; 
            bottomPanel.Controls.Add(_lblStatus); bottomPanel.Controls.Add(_btnDownCurrent);

            this.Controls.Add(galleryPanel);
            this.Controls.Add(searchPanel);
            this.Controls.Add(bottomPanel);
        }

        private async Task HandleScan()
        {
            string url = _txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url)) return;

            _lblStatus.Text = "正在启动独立内核浏览器...";
            try
            {
                if (_browser == null || _browser.IsClosed)
                {
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();
                    _browser = await Puppeteer.LaunchAsync(new LaunchOptions { 
                        Headless = true, 
                        // 关键：增加子目录 Image，避免多进程占用 BrowserData 冲突
                        UserDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BrowserData", "Image"),
                        Args = new[] { 
                            "--disable-web-security", 
                            "--disable-features=IsolateOrigins,site-per-process",
                            "--disable-notifications" // 减少弹窗干扰
                        }
                    });
                }

                if (_mainPage != null && !_mainPage.IsClosed) await _mainPage.CloseAsync();

                _mainPage = await _browser.NewPageAsync();
                await _mainPage.EvaluateFunctionOnNewDocumentAsync(@"() => { Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); }");
                
                await _mainPage.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                _pageTitle = await _mainPage.EvaluateExpressionAsync<string>("document.title") ?? "图片资源";
                _pageTitle = string.Join("_", _pageTitle.Split(Path.GetInvalidFileNameChars()));

                // 模拟滚动触发懒加载
                await _mainPage.EvaluateFunctionAsync(@"async () => {
                    await new Promise((resolve) => {
                        let totalHeight = 0, distance = 400;
                        let timer = setInterval(() => { window.scrollBy(0, distance); totalHeight += distance; if(totalHeight > 8000){ clearInterval(timer); resolve(); } }, 100);
                    });
                }");

                string jsCode = @"Array.from(document.querySelectorAll('img')).map(img => img.src || img.getAttribute('data-src')).filter(src => src && !src.startsWith('data:image'))";
                var urls = await _mainPage.EvaluateExpressionAsync<string[]>(jsCode);
                
                _imgUrls = new HashSet<string>(urls).Select(u => {
                    if (!u.StartsWith("http")) return Uri.TryCreate(new Uri(url), u, out Uri abs) ? abs.ToString() : "";
                    return u;
                }).Where(u => !string.IsNullOrEmpty(u)).ToList();

                if (_imgUrls.Count > 0)
                {
                    _currentIndex = 0;
                    await ShowCurrentPreviewAsync(); 
                    _lblStatus.Text = $"嗅探完成！共发现 {_imgUrls.Count} 张图。";
                }
                else _lblStatus.Text = "未发现任何有效图片。";
            }
            catch (Exception ex) { _lblStatus.Text = "嗅探出错: " + ex.Message; }
        }

        private async Task ShowCurrentPreviewAsync()
        {
            if (_imgUrls.Count == 0) return;

            _lblPage.Text = $"{_currentIndex + 1} / {_imgUrls.Count}";
            _lblStatus.Text = "正在渲染高清预览...";
            _btnDownCurrent.Enabled = false; 

            try
            {
                // 1. 确保预览组件初始化并获取 Handle
                await _webViewPreview.EnsureCoreWebView2Async();

                // 2. 使用 HTML 容器加载图片，确保完美居中且支持所有现代格式 (AVIF/WebP)
                string currentUrl = _imgUrls[_currentIndex];
                string htmlContent = $@"
                    <div style='background-color:black; height:100vh; display:flex; justify-content:center; align-items:center; margin:0; padding:0; overflow:hidden;'>
                        <img src='{currentUrl}' style='max-width:100%; max-height:100%; object-fit:contain;' />
                    </div>";
                
                _webViewPreview.NavigateToString(htmlContent);

                // 3. 同时在后台静默提取字节流，为“零延迟下载”做准备
                if (_mainPage != null && !_mainPage.IsClosed)
                {
                    string jsFetch = @"async (url) => {
                        try {
                            const resp = await fetch(url);
                            const blob = await resp.blob();
                            return await new Promise((res, rej) => {
                                const r = new FileReader();
                                r.onloadend = () => res(r.result);
                                r.readAsDataURL(blob);
                            });
                        } catch(e) { return 'ERR:' + e.message; }
                    }";

                    string base64Data = await _mainPage.EvaluateFunctionAsync<string>(jsFetch, currentUrl);
                    if (!base64Data.StartsWith("ERR:"))
                    {
                        var parts = base64Data.Split(',');
                        _currentImageBytes = Convert.FromBase64String(parts[1]);
                        _currentImageExt = base64Data.Contains("webp") ? ".webp" : 
                                         base64Data.Contains("png") ? ".png" : 
                                         base64Data.Contains("gif") ? ".gif" : ".jpg";
                        _btnDownCurrent.Enabled = true;
                        _lblStatus.Text = "预览与数据准备就绪。";
                    }
                    else { _lblStatus.Text = "预览成功，但数据抓取受限。"; }
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = "预览加载异常: " + ex.Message;
            }
        }

        private void HandleDownload(object sender, EventArgs e)
        {
            if (_currentImageBytes == null) return;

            using var sfd = new SaveFileDialog { 
                Title = "保存高清图片", 
                FileName = $"{_pageTitle}_{_currentIndex + 1}{_currentImageExt}", 
                Filter = "图片文件|*.*" 
            };
            
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try {
                File.WriteAllBytes(sfd.FileName, _currentImageBytes);
                
                var fileInfo = new FileInfo(sfd.FileName);
                new FileDbService().SaveFile(new FileEntity {
                    Title = Path.GetFileNameWithoutExtension(sfd.FileName), 
                    Url = _imgUrls[_currentIndex],
                    LocalPath = sfd.FileName, 
                    Type = "Image", 
                    OriginalName = Path.GetFileName(sfd.FileName),
                    FileSize = (fileInfo.Length / 1024.0 / 1024.0).ToString("0.00") + " MB", 
                    DownloadTime = DateTime.Now
                });
                MessageBox.Show("图片已成功归档！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) { MessageBox.Show("保存失败: " + ex.Message); }
        }
    }
}