using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Crawler.Core;

namespace Crawler.Engines
{
    public class TextEngine : ICrawlEngine
    {
        public async Task StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            progress.Report(new ProgressInfo(20, "-", "正在解析 DOM 树..."));
            
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url, token);

            // 提取网页标题和所有段落文本 (示例)
            string title = doc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? "未命名文档";
            var nodes = doc.DocumentNode.SelectNodes("//p"); // 抓取所有 <p> 标签

            progress.Report(new ProgressInfo(60, "-", "正在提取文本..."));
            
            using var writer = new StreamWriter(savePath);
            await writer.WriteLineAsync($"# {title}\n");

            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    token.ThrowIfCancellationRequested();
                    await writer.WriteLineAsync(node.InnerText.Trim());
                    await writer.WriteLineAsync(); // 空行
                }
            }

            progress.Report(new ProgressInfo(100, "-", "文本保存完成！"));
        }
    }
}