using System;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Core;

namespace Crawler.Engines
{
    public class ImageEngine : ICrawlEngine
    {
        private readonly HttpClient _client = new();

        public async Task StartAsync(string url, string saveDir, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            progress.Report(new ProgressInfo(10, "-", "正在获取网页源码..."));
            string html = await _client.GetStringAsync(url, token);

            // 1. 使用正则提取所有 img 标签的 src
            var matches = Regex.Matches(html, @"<img[^>]+src=""([^"">]+)""");
            if (matches.Count == 0) return;

            Directory.CreateDirectory(saveDir);
            int completed = 0;

            // 2. 遍历下载图片
            foreach (Match match in matches)
            {
                token.ThrowIfCancellationRequested(); // 检查是否取消

                string imgUrl = match.Groups[1].Value;
                if (!imgUrl.StartsWith("http")) imgUrl = new Uri(new Uri(url), imgUrl).ToString(); // 处理相对路径

                try
                {
                    byte[] imageBytes = await _client.GetByteArrayAsync(imgUrl, token);
                    string fileName = Path.GetFileName(new Uri(imgUrl).AbsolutePath);
                    if (string.IsNullOrEmpty(fileName)) fileName = Guid.NewGuid() + ".jpg";

                    await File.WriteAllBytesAsync(Path.Combine(saveDir, fileName), imageBytes, token);
                }
                catch { /* 忽略单个图片下载失败 */ }

                completed++;
                int percent = (int)((completed / (double)matches.Count) * 100);
                progress.Report(new ProgressInfo(percent, "-", $"已下载 {completed}/{matches.Count} 张图片"));
            }
        }
    }
}