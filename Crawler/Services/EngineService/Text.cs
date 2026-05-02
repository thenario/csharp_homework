using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmartReader;
using PuppeteerSharp;
using Crawler.Core;

namespace Crawler.Engines
{
    public class TextEngine : ICrawlEngine
    {
        public async Task<string> StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token)
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

            progress.Report(new ProgressInfo(40, "-", "正在暴力破解隐藏正文及折叠按钮..."));
            await page.EvaluateFunctionAsync(@"async () => {
                // 1. 自动滚屏
                await new Promise((resolve) => {
                    let totalHeight = 0;
                    let distance = 300;
                    let timer = setInterval(() => {
                        window.scrollBy(0, distance);
                        totalHeight += distance;
                        if(totalHeight >= document.body.scrollHeight || totalHeight > 10000) { clearInterval(timer); resolve(); }
                    }, 100);
                });

                // 2. 狂暴点击所有可能的“展开全文”按钮
                const expandSelectors =['.read-more', '.expand', '.btn-readmore', '.js-unfold', '.show-more', '.btn-bg', '.fold-btn', '.text-expand', '.article-unfold', '.collapse-btn'];
                expandSelectors.forEach(sel => document.querySelectorAll(sel).forEach(b => { try { b.click(); } catch(e){} }));

                // 3. 终极破解：强制取消所有被网站隐藏的样式 (把 display:none 强行拔掉)
                document.querySelectorAll('*').forEach(el => {
                    const style = window.getComputedStyle(el);
                    if (style.display === 'none' && !el.tagName.toLowerCase().includes('script') && !el.tagName.toLowerCase().includes('style')) {
                        el.style.display = 'block';
                        el.style.height = 'auto';
                        el.style.maxHeight = 'none';
                        el.style.overflow = 'visible';
                    }
                });

                // 4. 清除垃圾节点
                const trashSelectors =['#onetrust-consent-sdk', '.cookie-banner', '.recommend', '.related', '.advertisement', '.ad-box', 'nav', 'footer', '.header', '.comment-list'];
                trashSelectors.forEach(sel => document.querySelectorAll(sel).forEach(el => el.remove()));
            }");

            await Task.Delay(2000, token); // 给 DOM 渲染留一点时间

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
                    if (ps.length > 0) {
                        return ps.map(p => p.innerText.trim()).filter(t => t.length > 10).join('\n\n');
                    }
                    return document.body.innerText;
                }");
            }

            // 清除混进去的杂质词
            finalContent = finalContent?.Replace("为你精选更多内容", "")?.Replace("继续阅读", "")?.Trim();

            // 如果折腾了这么多还是提取不到，直接抛出异常拦截，而不是生成无用的 txt 文件
            if (string.IsNullOrWhiteSpace(finalContent) || finalContent.Length < 20)
            {
                throw new Exception("无法提取到有效的正文，该网站的正文可能被严重加密，或者需要强制登录/App打开。");
            }

            progress.Report(new ProgressInfo(90, "-", "正在写入文件..."));
            using var writer = new StreamWriter(savePath);
            await writer.WriteLineAsync($"标题: {(string.IsNullOrEmpty(article.Title) ? "未命名标题" : article.Title)}");
            await writer.WriteLineAsync($"抓取时间: {DateTime.Now}");
            await writer.WriteLineAsync(new string('-', 50) + "\n");
            await writer.WriteLineAsync(finalContent);

            progress.Report(new ProgressInfo(100, "-", "文本提取完成。"));
            return savePath;
        }
    }
}