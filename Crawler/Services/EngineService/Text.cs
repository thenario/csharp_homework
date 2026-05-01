using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PuppeteerSharp;
using Crawler.Core;

namespace Crawler.Engines
{
    public class TextEngine : ICrawlEngine
    {
        public async Task StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            progress.Report(new ProgressInfo(10, "-", "正在启动无头浏览器提取动态文本..."));

            // 1. 启动浏览器加载动态页面
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            token.ThrowIfCancellationRequested();

            progress.Report(new ProgressInfo(40, "-", "正在等待页面元素渲染完成..."));
            await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });

            string html = await page.GetContentAsync();

            progress.Report(new ProgressInfo(70, "-", "正在解析DOM并提取正文..."));

            // 2. 将渲染后的 HTML 喂给 HtmlAgilityPack 进行精准解析
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes("//p | //article | //div[contains(@class, 'content')]");

            if (nodes == null || nodes.Count == 0)
            {
                throw new Exception("无法提取到文本正文，可能是该页面的正文标签不符合常见规则。");
            }

            // 3. 写入文件
            using var writer = new StreamWriter(savePath);
            foreach (var node in nodes)
            {
                token.ThrowIfCancellationRequested();
                string text = node.InnerText.Trim();

                // 过滤掉过短的无意义字符，比如纯粹的换行符或空格
                if (text.Length > 10)
                {
                    // 替换多余的空白符，保持文本干净
                    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
                    await writer.WriteLineAsync(text + "\n");
                }
            }

            progress.Report(new ProgressInfo(100, "-", "文本提取完成并已保存。"));
        }
    }
}