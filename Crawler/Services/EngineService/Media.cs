using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Core;

namespace Crawler.Engines
{
    public class MediaEngine : ICrawlEngine
    {
        public async Task StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "Tools/yt-dlp.exe",
                Arguments = $"\"{url}\" -o \"{savePath}\" --newline",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            using var registration = token.Register(() =>
            {
                if (!process.HasExited) process.Kill();
            });

            while (!process.StandardOutput.EndOfStream)
            {
                if (token.IsCancellationRequested) break;

                string? line = await process.StandardOutput.ReadLineAsync();
                if (line != null && line.Contains("[download]"))
                {
                    var pctMatch = Regex.Match(line, @"(\d+\.?\d*)(?=%)");

                    var speedMatch = Regex.Match(line, @"(?<=at\s+)(.*?/s)");

                    if (pctMatch.Success)
                    {
                        float.TryParse(pctMatch.Value, out float percent);
                        string speed = speedMatch.Success ? speedMatch.Value.Trim() : "未知";
                        progress.Report(new ProgressInfo((int)percent, speed, $"正在下载: {percent}%"));
                    }
                }
            }

            await process.WaitForExitAsync();
        }
    }
}
