using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PuppeteerSharp;
using Crawler.Core;

namespace Crawler.Engines
{
    public class ImageEngine : ICrawlEngine
    {
        public async Task<string> StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            progress.Report(new ProgressInfo(20, "-", "正在启动无头浏览器强穿防盗链..."));

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            // 使用真实的浏览器环境直接访问图片，网站绝不可能拦截
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, UserDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BrowserData") });
            using var page = await browser.NewPageAsync();

            await page.EvaluateFunctionOnNewDocumentAsync(@"() => { Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); }");
            await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            progress.Report(new ProgressInfo(50, "-", "正在获取真实图片数据..."));
            var response = await page.GoToAsync(url, new NavigationOptions { WaitUntil = new[] { WaitUntilNavigation.Networkidle2 } });

            if (!response.Ok)
                throw new Exception($"图片下载失败：服务器拒绝访问 (HTTP {response.Status})，可能是高级防盗链。");

            // 直接从浏览器底层抽离图片字节流！
            byte[] imageBytes = await response.BufferAsync();
            if (imageBytes == null || imageBytes.Length == 0)
                throw new Exception("下载失败：获取到了空文件，图片可能已失效。");

            // 动态验证并修正后缀
            var contentType = response.Headers.ContainsKey("content-type") ? response.Headers["content-type"].ToLower() : "";
            string trueExt = Path.GetExtension(savePath);
            if (contentType.Contains("webp")) trueExt = ".webp";
            else if (contentType.Contains("png")) trueExt = ".png";
            else if (contentType.Contains("gif")) trueExt = ".gif";
            else if (contentType.Contains("svg")) trueExt = ".svg";
            else if (contentType.Contains("jpeg") || contentType.Contains("jpg")) trueExt = ".jpg";

            string dir = Path.GetDirectoryName(savePath);
            string name = Path.GetFileNameWithoutExtension(savePath);
            string finalSavePath = Path.Combine(dir, name + trueExt);

            progress.Report(new ProgressInfo(80, "-", $"格式识别为 {trueExt}，正在写入硬盘..."));
            await File.WriteAllBytesAsync(finalSavePath, imageBytes, token);

            progress.Report(new ProgressInfo(100, "-", "图片下载完成"));
            return finalSavePath;
        }
    }
}