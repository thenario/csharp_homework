using System;

namespace Crawler.Entities;//实体类的命名空间

public class FileEntity//文件实体类
{
    public int Id { get; set; }//和c++不同，get获取，set赋值，同时可以在里面加上额外的处理逻辑set{......}
    public string? Title { get; set; }//标题，不带后缀
    public string? Type { get; set; }//文件类型，video，txt，image
    public DateTime DownloadTime { get; set; }//下载时间
    public string? FileSize { get; set; }//文件大小
    public string? Url { get; set; }//文件的url
    public string? OriginalName { get; set; }//文件原始名，带后缀
    public string? LocalPath {get;set;}//本地位置
    public FileEntity()//构造函数，下载时间默认为构造对象时的时间
    {
        DownloadTime = DateTime.Now;
    }
}
