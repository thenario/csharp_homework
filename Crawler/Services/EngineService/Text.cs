using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartReader;
using PuppeteerSharp;
using Crawler.Core;

namespace Crawler.Engines
{
    public class TextEngine
    {
        public async Task<string> ExtractTextAsync(string url, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            progress.Report(new ProgressInfo(10, "-", "正在启动无头浏览器..."));
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, UserDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BrowserData") });
            using var page = await browser.NewPageAsync();

            await page.EvaluateFunctionOnNewDocumentAsync(@"() => { Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); }");
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            progress.Report(new ProgressInfo(30, "-", "等待页面加载..."));
            await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });

            progress.Report(new ProgressInfo(40, "-", "正在破解隐藏正文及折叠按钮..."));
            await page.EvaluateFunctionAsync(@"async () => {
                await new Promise((resolve) => {
                    let totalHeight = 0, distance = 300;
                    let timer = setInterval(() => { window.scrollBy(0, distance); totalHeight += distance; if(totalHeight > 10000) { clearInterval(timer); resolve(); } }, 100);
                });

                const expandSelectors =['.read-more', '.expand', '.btn-readmore', '.js-unfold', '.show-more', '.btn-bg', '.fold-btn', '.text-expand', '.article-unfold', '.collapse-btn'];
                expandSelectors.forEach(sel => document.querySelectorAll(sel).forEach(b => { try { b.click(); } catch(e){} }));

                document.querySelectorAll('*').forEach(el => {
                    const style = window.getComputedStyle(el);
                    if (style.display === 'none' && !el.tagName.toLowerCase().includes('script') && !el.tagName.toLowerCase().includes('style')) {
                        el.style.display = 'block'; el.style.height = 'auto'; el.style.maxHeight = 'none'; el.style.overflow = 'visible';
                    }
                });

                const trashSelectors =['#onetrust-consent-sdk', '.cookie-banner', '.recommend', '.related', '.advertisement', '.ad-box', 'nav', 'footer', '.header', '.comment-list'];
                trashSelectors.forEach(sel => document.querySelectorAll(sel).forEach(el => el.remove()));
            }");

            await Task.Delay(2000, token);

            string html = await page.GetContentAsync();
            progress.Report(new ProgressInfo(60, "-", "正在智能识别正文..."));

            Reader reader = new Reader(url, html);
            Article article = await reader.GetArticleAsync();
            string finalContent = article.TextContent;

            if (string.IsNullOrWhiteSpace(finalContent) || finalContent.Length < 100)
            {
                progress.Report(new ProgressInfo(75, "-", "启动 DOM 强制提取..."));
                finalContent = await page.EvaluateFunctionAsync<string>(@"() => {
                    let selectors =['article', '.article', '#article', '.post_body', '.content-article', '.article-content', '#artibody', '.rich_media_content', '.post-content', '.article__content', '#content', 'main'];
                    let mainNodes = document.querySelectorAll(selectors.join(', '));
                    if (mainNodes.length > 0) {
                        let best = Array.from(mainNodes).sort((a, b) => b.innerText.length - a.innerText.length)[0];
                        if (best && best.innerText.length > 50) return best.innerText;
                    }
                    let ps = Array.from(document.querySelectorAll('p'));
                    if (ps.length > 0) return ps.map(p => p.innerText.trim()).filter(t => t.length > 10).join('\n\n');
                    return document.body.innerText;
                }");
            }

            finalContent = finalContent?.Replace("为你精选更多内容", "")?.Replace("继续阅读", "")?.Trim();

            if (string.IsNullOrWhiteSpace(finalContent) || finalContent.Length < 20)
                throw new Exception("无法提取到有效的正文。");

            progress.Report(new ProgressInfo(100, "-", "文本提取完成。"));

            // 核心修改：直接在内存中拼装好字符串，返回给 UI！
            string resultText = $"标题: {(string.IsNullOrEmpty(article.Title) ? "未命名标题" : article.Title)}\r\n";
            resultText += $"抓取时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";
            resultText += new string('-', 50) + "\r\n\r\n";
            resultText += finalContent;

            return resultText; 
        }
    }
}