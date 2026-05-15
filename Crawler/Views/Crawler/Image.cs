using System;
using System.Collections.Generic;//泛型容器
using System.Drawing;
using System.IO;
using System.Linq;//语言集成查询
using System.Threading.Tasks;
using System.Windows.Forms;
using Crawler.Entities;
using Crawler.Services.DbService;
using PuppeteerSharp;//浏览器内核，不带ui界面
using Microsoft.Web.WebView2.WinForms;//嵌入式浏览器控件，利用了windows电脑自带的edge内核，带ui界面，用于预览网页的图片内容

namespace Crawler.Views
{
    public class ImageCrawl : UserControl
    {
        private TextBox _txtUrl;//显示url
        private Label _lblStatus;//显示状态
        private Button _btnDownCurrent;//下载按钮
        private WebView2 _webViewPreview;//预览用的浏览器实例
        private Label _lblPage;//当前页
        private Button _btnPrev;//上一页
        private Button _btnNext;//下一页

        private List<string> _imgUrls = new List<string>();//存储页面中图片的url
        private int _currentIndex = 0;//初始化当前页为零
        private string _pageTitle = "图片资源";//页面标题
        private IBrowser _browser;//浏览器实例
        private IPage _mainPage;//页面实例

        private byte[] _currentImageBytes = null;//存储当前图片的二进制原始信息
        private string _currentImageExt = ".jpg";//初始化后缀为jpg

        public void InitUi()
        {
            this.Controls.Clear();//清空控件
            this.Padding = new Padding(20);//内边距

            var searchPanel = new Panel { Dock = DockStyle.Top, Height = 60 };//上方搜索栏
            _txtUrl = new TextBox { Location = new System.Drawing.Point(0, 15), Width = 500, Font = new Font("Consolas", 12) };
            var btnScan = new Button { Text = "嗅探图片", Location = new System.Drawing.Point(510, 12), Width = 130, Height = 32, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White };
            btnScan.Click += async (s, e) => await HandleScan();
            searchPanel.Controls.Add(_txtUrl);
            searchPanel.Controls.Add(btnScan);

            //中间预览区
            var galleryPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(236, 240, 241) };
            _webViewPreview = new WebView2
            {
                Dock = DockStyle.Fill,//填充剩余空间
                DefaultBackgroundColor = Color.Black//设置背景颜色
            };
            galleryPanel.Controls.Add(_webViewPreview);
            //翻页控制条
            var controlsPanel = new Panel { Dock = DockStyle.Bottom, Height = 60, BackColor = Color.White };
            _btnPrev = new Button { Text = "◀ 上一张", Location = new System.Drawing.Point(20, 10), Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            _btnNext = new Button { Text = "下一张 ▶", Location = new System.Drawing.Point(260, 10), Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            _lblPage = new Label { Text = "0 / 0", Location = new System.Drawing.Point(130, 18), Width = 120, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("微软雅黑", 12, FontStyle.Bold) };
            //判断逻辑
            _btnPrev.Click += async (s, e) => { if (_currentIndex > 0) { _currentIndex--; await ShowCurrentPreviewAsync(); } };
            _btnNext.Click += async (s, e) => { if (_currentIndex < _imgUrls.Count - 1) { _currentIndex++; await ShowCurrentPreviewAsync(); } };
            //添加控件
            controlsPanel.Controls.Add(_btnPrev);
            controlsPanel.Controls.Add(_lblPage);
            controlsPanel.Controls.Add(_btnNext);
            galleryPanel.Controls.Add(controlsPanel);
            // 底部状态栏
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };
            _lblStatus = new Label { Text = "准备就绪", Top = 25, Left = 0, AutoSize = true, MaximumSize = new Size(200, 0) };
            _btnDownCurrent = new Button { Text = "下载当前预览图", Top = 20, Left = 650, Width = 150, Height = 40, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, Enabled = false };
            _btnDownCurrent.Click += HandleDownload;
            bottomPanel.Controls.Add(_lblStatus);
            bottomPanel.Controls.Add(_btnDownCurrent);
            //添加
            this.Controls.Add(galleryPanel);
            this.Controls.Add(searchPanel);
            this.Controls.Add(bottomPanel);
        }

        private async Task HandleScan()
        {
            string url = _txtUrl.Text.Trim();//去除开头末尾的空格
            if (string.IsNullOrEmpty(url)) return;//空则返回
            _lblStatus.Text = "正在启动独立内核浏览器...";//显示状态
            try
            {
                if (_browser == null || _browser.IsClosed)
                {
                    var browserFetcher = new BrowserFetcher();
                    await browserFetcher.DownloadAsync();
                    _browser = await Puppeteer.LaunchAsync(new LaunchOptions
                    {
                        Headless = true,
                        UserDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BrowserData_Image"),
                        Args = new[] {
                            "--disable-web-security",//禁用网络安全性，可跨域
                            "--disable-features=IsolateOrigins,site-per-process",//禁用站点隔离，所有网页在一个进程运行，节省内存
                            "--disable-notifications"//减少弹窗干扰
                        }
                    });
                }
                //程序退出时触发，清理浏览器进程
                var chromeProcess = _browser.Process;
                AppDomain.CurrentDomain.ProcessExit += (sender, args) =>
                {
                    if (chromeProcess != null && !chromeProcess.HasExited)
                    {
                        chromeProcess.Kill();
                    }
                };

                if (_mainPage != null && !_mainPage.IsClosed) await _mainPage.CloseAsync();

                _mainPage = await _browser.NewPageAsync();
                //防止被认出是自动程序
                await _mainPage.EvaluateFunctionOnNewDocumentAsync(@"() => { Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); }");
                //依旧是当连接稳定时确认加载完毕
                await _mainPage.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });
                //获取网页标题作为页面标题
                _pageTitle = await _mainPage.EvaluateExpressionAsync<string>("document.title") ?? "图片资源";
                //把非法的字符斜杠冒号问号之类的变成_
                _pageTitle = string.Join("_", _pageTitle.Split(Path.GetInvalidFileNameChars()));
                //模拟滚动触发懒加载
                await _mainPage.EvaluateFunctionAsync(@"async () => {
                    await new Promise((resolve) => {
                        let totalHeight = 0, distance = 400;
                        let timer = setInterval(() => { window.scrollBy(0, distance); totalHeight += distance; if(totalHeight > 8000){ clearInterval(timer); resolve(); } }, 100);
                    });
                }");
                //将返回的nodelist转换成数组，获取src并把空的和base64这种非url的src过滤掉
                string jsCode = @"Array.from(document.querySelectorAll('img')).map(img => img.src || img.getAttribute('data-src')).filter(src => src && !src.startsWith('data:image'))";
                //evaluateexpressionasync直接传js代码字符串
                //evaluatefunctionasync传js格式的函数字符串
                var urls = await _mainPage.EvaluateExpressionAsync<string[]>(jsCode);
                //转换成set能直接去重

                _imgUrls = new HashSet<string>(urls).Select(u =>
                {
                    //try防止程序崩溃
                    //传入url充当基址，如果失败了就返回空字符串
                    if (!u.StartsWith("http")) return Uri.TryCreate(new Uri(url), u, out Uri abs) ? abs.ToString() : "";
                    return u;
                }).Where(u => !string.IsNullOrEmpty(u)).ToList();//过滤掉空并转化为list

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
                //确保预览组件初始化
                await _webViewPreview.EnsureCoreWebView2Async();
                //加载图片
                string currentUrl = _imgUrls[_currentIndex];
                //$表示插值就是格式字符串，py的f，node里的`${}`
                string htmlContent = $@"
                    <div style='background-color:black; height:100vh; display:flex; justify-content:center; align-items:center; margin:0; padding:0; overflow:hidden;'>
                        <img src='{currentUrl}' style='max-width:100%; max-height:100%; object-fit:contain;' />
                    </div>";
                _webViewPreview.NavigateToString(htmlContent);//渲染出图片
                // 3. 同时在后台静默提取字节流，为“零延迟下载”做准备
                if (_mainPage != null && !_mainPage.IsClosed)
                {
                    string jsFetch = @"async (url) => {
                        try {
                            const resp = await fetch(url);//抓取响应
                            const blob = await resp.blob();//转换成二进制
                            return await new Promise((res, rej) => {
                                const r = new FileReader();//读取器
                                r.onloadend = () => res(r.result);//读取结束返回结果
                                r.readAsDataURL(blob);//读取数据
                            });
                        } catch(e) { return 'ERR:' + e.message; }
                    }";

                    string base64Data = await _mainPage.EvaluateFunctionAsync<string>(jsFetch, currentUrl);
                    if (!base64Data.StartsWith("ERR:"))
                    {
                        var parts = base64Data.Split(',');//分割，数据格式belike"数据头，数据"
                        _currentImageBytes = Convert.FromBase64String(parts[1]);//获取真正的数据并转换成原始的文件二进制流
                        _currentImageExt = base64Data.Contains("webp") ? ".webp" : //确定后缀
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

            using var sfd = new SaveFileDialog
            {
                Title = "保存图片",
                FileName = $"{_pageTitle}_{_currentIndex + 1}{_currentImageExt}",
                Filter = $"{_currentImageExt.ToUpper().Trim('.')}文件|*{_currentImageExt}|所有文件|*.*"//描述以及过滤条件
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                File.WriteAllBytes(sfd.FileName, _currentImageBytes);//写进磁盘

                var fileInfo = new FileInfo(sfd.FileName);
                new FileDbService().SaveFile(new FileEntity
                {
                    Title = Path.GetFileNameWithoutExtension(sfd.FileName),
                    Url = _imgUrls[_currentIndex],
                    LocalPath = sfd.FileName,
                    Type = "Image",
                    OriginalName = Path.GetFileName(sfd.FileName),
                    FileSize = (fileInfo.Length / 1024.0 / 1024.0).ToString("0.00") + " MB",
                    DownloadTime = DateTime.Now
                });
                MessageBox.Show("图片已成功归档！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex) { MessageBox.Show("保存失败: " + ex.Message); }
        }
        //清理浏览器进程
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        if (_mainPage != null && !_mainPage.IsClosed) await _mainPage.CloseAsync();
                        if (_browser != null && !_browser.IsClosed) await _browser.CloseAsync();
                        if (_webViewPreview != null) _webViewPreview.Dispose();
                    }
                    catch { /* 忽略销毁时的异常 */ }
                });
            }
            base.Dispose(disposing);
        }
    }

}