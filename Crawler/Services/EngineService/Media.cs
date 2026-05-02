using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Crawler.Core;

namespace Crawler.Engines
{
    public class MediaEngine : ICrawlEngine
    {
        private readonly bool _isAudioOnly;
        public MediaEngine(bool isAudioOnly = false) { _isAudioOnly = isAudioOnly; }

        public async Task<string> StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            string formatArgs = _isAudioOnly ? "-x --audio-format mp3" : "-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"";

            var startInfo = new ProcessStartInfo {
                FileName = "Tools/yt-dlp.exe",
                Arguments = $"{formatArgs} \"{url}\" -o \"{savePath}\" --newline",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            using var registration = token.Register(() => { if (!process.HasExited) process.Kill(); });

            while (!process.StandardOutput.EndOfStream) {
                if (token.IsCancellationRequested) break;
                string line = await process.StandardOutput.ReadLineAsync();
                if (line != null && line.Contains("[download]")) {
                    var pctMatch = Regex.Match(line, @"(\d+\.?\d*)(?=%)");
                    if (pctMatch.Success) {
                        float.TryParse(pctMatch.Value, out float percent);
                        progress.Report(new ProgressInfo((int)percent, "-", $"下载中: {percent}%"));
                    }
                }
            }
            await process.WaitForExitAsync();

            // 核心修复：检查 yt-dlp 是否报错退出，且本地是否真的生成了文件
            if (process.ExitCode != 0 || !File.Exists(savePath))
            {
                throw new Exception("下载失败：该网页并没有包含 yt-dlp 支持的真实视频源（可能只是广告插件）。");
            }

            return savePath;
        }
    }
}