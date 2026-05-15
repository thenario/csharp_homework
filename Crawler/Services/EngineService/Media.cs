using System;//iprogress在这个包里
using System.Diagnostics;//用户打印调试信息的包，process，processstartinfo在这个包里
using System.IO;//负责输入输出的包
using System.Text.RegularExpressions;//正则表达式
using System.Threading;//多线程操作，cancellation在这个包里
using System.Threading.Tasks;//异步操作，async，await
using Crawler.Core;//存有progressinfo的那个命名空间

namespace Crawler.Engines
{
    public class MediaEngine
    {
        private readonly bool _isAudioOnly;//只读
        public MediaEngine(bool isAudioOnly = false) { _isAudioOnly = isAudioOnly; }//构造函数，默认video
        //task表示一个正在进行的人物
        //string标识任务完成之后返回字符串
        //iprogress可以根据progress的report内容吧数据发回到主程序
        //cancellationtoken用于控制这个任务，比如中断任务
        public async Task<string> StartAsync(string url, string savePath, IProgress<ProgressInfo> progress, CancellationToken token)
        {
            //yt-dlp的启动参数，正常的视频都是分段发送，拼接的操作比较困难，所以就是用ytdlp进行底层的操作，因此该爬虫程序里有ytdlp，ffmpeg，ffprobe，体积较大
            //-x --audio-format mp3，-x表示提取音频，下载完之后只保留音频，--audio-format mp3标识把下载下来的原始音频重新编码为MP3
            //-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"
            //-f表示指定下载的资源的格式
            // /代表或者，从最开始的选项一直执行直到成功
            //bestvideo[ext=mp4]+bestaudio[ext=m4a]下载最高画质的视频流即最高画质的音频流并且合并
            //best[ext=mp4]下载已有的最好的单文件MP4
            //或者不管什么东西，最好就行
            string formatArgs = _isAudioOnly ? "-x --audio-format mp3" : "-f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/best[ext=mp4]/best\"";

            var startInfo = new ProcessStartInfo {
                FileName = "Tools/yt-dlp.exe",//程序路径
                //--newline表示输出进度时换行
                //-o表示输出的文件路径
                Arguments = $"{formatArgs} \"{url}\" -o \"{savePath}\" --newline",
                RedirectStandardOutput = true,//重定向标准输出，把输出定向到程序这里
                UseShellExecute = false,//让进程由程序控制
                CreateNoWindow = true,//不创建窗口，后台静默运行
                StandardOutputEncoding = System.Text.Encoding.UTF8//编码，防止乱码
            };

            using var process = new Process { StartInfo = startInfo };//创建执行体
            process.Start();//执行程序
            //订阅cancellationtokensource的cancel信号，一旦信号触发，执行函数体，停止程序
            using var registration = token.Register(() => { if (!process.HasExited) process.Kill(); });

            while (!process.StandardOutput.EndOfStream) {//检查是否还有输出
                if (token.IsCancellationRequested) break;//cancel（）执行会变为false
                string line = await process.StandardOutput.ReadLineAsync();//等待读取一行standoutput的一行输出
                if (line != null && line.Contains("[download]")) {//满足条件的话提取需要的信息
                    //正则，@表示无转义，\d+匹配一个或多个数字，\.?一个或零个小数点，\d*零个或多个数字，(?=%)表示后面有一个%但是不要%
                    var pctMatch = Regex.Match(line, @"(\d+\.?\d*)(?=%)");
                    if (pctMatch.Success) {
                        float.TryParse(pctMatch.Value, out float percent);//try防止程序因为某些情况崩溃，比如多了一个空格，指定输出percent
                        progress.Report(new ProgressInfo((int)percent, "-", $"下载中: {percent}%"));//将信息report给主进程
                    }
                }
            }
            await process.WaitForExitAsync();//等待程序下载完成

            if (process.ExitCode != 0 || !File.Exists(savePath))//检查yt-dlp是否报错退出，且本地是否真的生成了文件
            {
                throw new Exception("下载失败：该网页并没有包含 yt-dlp 支持的真实视频源（可能只是广告插件）。");
            }

            return savePath;//返回保存地址，后续数据库操作使用
        }
    }
}