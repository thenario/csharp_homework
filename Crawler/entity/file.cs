using System;

namespace Crawler.Entities;

public class FileEntity
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
    public DateTime DownloadTime { get; set; }
    public string? FileSize { get; set; }
    public string? Url { get; set; }
    public string? OriginalName { get; set; }
    public string? LocalPath {get;set;}
    public FileEntity()
    {
        DownloadTime = DateTime.Now;
    }
}
