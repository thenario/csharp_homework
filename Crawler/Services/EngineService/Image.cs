using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Core;

namespace Crawler.Engines
{
    public class ImageEngine : ICrawlEngine
    {
        public async Task StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            progress.Report(new ProgressInfo(10, "-", "正在连接图片服务器..."));
            
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.Add("Referer", url);

            byte[] imageBytes = await client.GetByteArrayAsync(url, token);
            
            progress.Report(new ProgressInfo(80, "-", "正在写入本地文件..."));
            await File.WriteAllBytesAsync(savePath, imageBytes, token);
            
            progress.Report(new ProgressInfo(100, "-", "图片下载完成"));
        }
    }
}