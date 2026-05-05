using System;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler.Core
{
    public record ProgressInfo(int Percent, string Speed, string Message);

    public interface ICrawlEngine
    {
        Task<string> StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token);
    }

    public class ScanResult
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
    }
}