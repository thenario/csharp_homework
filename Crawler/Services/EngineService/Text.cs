using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartReader;//用于提取html的标题正文等信息
using PuppeteerSharp;//用于启动无头浏览器
using Crawler.Core;

namespace Crawler.Engines
{
    public class TextEngine
    {
        public async Task<string> ExtractTextAsync(string url, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            progress.Report(new ProgressInfo(10, "-", "正在启动无头浏览器..."));//把信息report给主线程
            var browserFetcher = new BrowserFetcher();//创建一个浏览器获取器
            await browserFetcher.DownloadAsync();//如果没有浏览器内核则异步下载浏览器内核

            //按照参数内容启动浏览器
            //headless无头模式，不会有ui界面
            //userdata，在程序运行目录下创建一个文件夹用于记录浏览数据，cookie，缓存等
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, UserDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BrowserData_Txt") });
            //新建一个网页
            using var page = await browser.NewPageAsync();
            //EvaluateFunctionOnNewDocumentAsync钩子，在page里的内容变化之前执行里面的函数体
            //@无转义及多行字符串
            //后面的函数体将webdriver改为undefined避免被浏览器识别为自动程序
            await page.EvaluateFunctionOnNewDocumentAsync(@"() => { Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); }");
            //设置useragent，用户代理，覆盖原有的信息，Mozilla/5.0通用前缀，Win64; x64操作系统，AppleWebKit/537.36渲染引擎
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            progress.Report(new ProgressInfo(30, "-", "等待页面加载..."));//report信息
            //访问这个url，WaitUntilNavigation.Networkidle2表示至少500毫秒内，网络请求不超过2个。
            await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });

            progress.Report(new ProgressInfo(40, "-", "正在破解隐藏正文及折叠按钮..."));//
            await page.EvaluateFunctionAsync(@"async () => {
            /*滚动直到距离超过10000像素*/
                await new Promise((resolve) => {
                    let totalHeight = 0, distance = 300;
                    let timer = setInterval(() => { window.scrollBy(0, distance); totalHeight += distance; if(totalHeight > 10000) { clearInterval(timer); resolve(); } }, 100);
                });
                /*寻找可能的展开button并且点击*/
                const expandSelectors =['.read-more', '.expand', '.btn-readmore', '.js-unfold', '.show-more', '.btn-bg', '.fold-btn', '.text-expand', '.article-unfold', '.collapse-btn'];
                expandSelectors.forEach(sel => document.querySelectorAll(sel).forEach(b => { try { b.click(); } catch(e){} }));
                /*把可能被隐藏的元素都展示出来*/
                document.querySelectorAll('*').forEach(el => {
                    const style = window.getComputedStyle(el);
                    if (style.display === 'none' && !el.tagName.toLowerCase().includes('script') && !el.tagName.toLowerCase().includes('style')) {
                        el.style.display = 'block'; el.style.height = 'auto'; el.style.maxHeight = 'none'; el.style.overflow = 'visible';
                    }
                });
                /*删除掉广告弹窗之类的东西*/
                const trashSelectors =['#onetrust-consent-sdk', '.cookie-banner', '.recommend', '.related', '.advertisement', '.ad-box', 'nav', 'footer', '.header', '.comment-list'];
                trashSelectors.forEach(sel => document.querySelectorAll(sel).forEach(el => el.remove()));
            }");
            //等待上述操作完成，传入的token可以通过cancel取消这个delay
            await Task.Delay(2000, token);
            string html = await page.GetContentAsync();//获取处理之后的整个页面的html源码
            progress.Report(new ProgressInfo(60, "-", "正在智能识别正文..."));

            Reader reader = new Reader(url, html);//初始化一个smarterreader阅读器，传url的话他会自动处理相对路径的问题
            Article article = await reader.GetArticleAsync();//通过内部的算法自动判断出正文内容并返回一个article对象
            string finalContent = article.TextContent;//获取正文文本

            if (string.IsNullOrWhiteSpace(finalContent) || finalContent.Length < 100)//提取不到河里的正文内容
            {
                progress.Report(new ProgressInfo(75, "-", "启动 DOM 强制提取..."));
                finalContent = await page.EvaluateFunctionAsync<string>(@"() => {
                    let selectors =['article', '.article', '#article', '.post_body', '.content-article', '.article-content', '#artibody', '.rich_media_content', '.post-content', '.article__content', '#content', 'main'];
                    let mainNodes = document.querySelectorAll(selectors.join(', '));
                    if (mainNodes.length > 0) {
                        let best = Array.from(mainNodes).sort((a, b) => b.innerText.length - a.innerText.length)[0];
                        if (best && best.innerText.length > 50) return best.innerText;
                    }
                    /*连上面都没有内容*/
                    let ps = Array.from(document.querySelectorAll('p'));
                    if (ps.length > 0) return ps.map(p => p.innerText.trim()).filter(t => t.length > 10).join('\n\n');
                    /*最后直接把所有内容都抓取出来返回*/
                    return document.body.innerText;
                }");
            }

            finalContent = finalContent?.Replace("为你精选更多内容", "")?.Replace("继续阅读", "")?.Trim();//替换无意义的内容，并删除开头和末尾的空格

            if (string.IsNullOrWhiteSpace(finalContent) || finalContent.Length < 20)
                throw new Exception("无法提取到有效的正文。");

            progress.Report(new ProgressInfo(100, "-", "文本提取完成。"));

            //直接拼装字符串再返回
            string resultText = $"标题: {(string.IsNullOrEmpty(article.Title) ? "未命名标题" : article.Title)}\r\n";
            resultText += $"抓取时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";
            resultText += new string('-', 50) + "\r\n\r\n";
            resultText += finalContent;

            return resultText;
        }
    }
}