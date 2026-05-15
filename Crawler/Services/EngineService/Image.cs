//已修改，在v4不需要这个文件，但为了结构的完整还是保留了这个

// using System;
// using System.IO;
// using System.Net.Http;
// using System.Threading.Tasks;

// namespace Crawler.Engines
// {
//     public class ImageEngine
//     {
//         // 核心修改：直接返回内存字节流和真实后缀！不再写入硬盘！
//         public async Task<(byte[] Bytes, string Extension)> FetchImageAsync(string imageUrl, string refererUrl)
//         {
//             using var client = new HttpClient();
//             // 伪装浏览器请求头
//             client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
//             client.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");
            
//             // 【核心破盾】：带上原网页的网址作为 Referer，解决 99% 的图片防盗链！
//             if (!string.IsNullOrEmpty(refererUrl))
//                 client.DefaultRequestHeaders.Add("Referer", refererUrl);

//             using var response = await client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);
//             response.EnsureSuccessStatusCode();

//             var contentType = response.Content.Headers.ContentType?.MediaType?.ToLower() ?? "";
//             if (contentType.StartsWith("text/"))
//                 throw new Exception("服务器拒绝了图片请求，返回了验证码或报错网页。");

//             // 动态验证并修正后缀
//             string trueExt = ".jpg";
//             if (contentType.Contains("webp")) trueExt = ".webp";
//             else if (contentType.Contains("png")) trueExt = ".png";
//             else if (contentType.Contains("gif")) trueExt = ".gif";
//             else if (contentType.Contains("svg")) trueExt = ".svg";

//             // 直接将图片下载到内存中
//             byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
//             if (imageBytes == null || imageBytes.Length == 0)
//                 throw new Exception("获取到了0字节的空图片。");

//             return (imageBytes, trueExt);
//         }
//     }
// }