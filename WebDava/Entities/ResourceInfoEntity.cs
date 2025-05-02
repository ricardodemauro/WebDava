using System;

namespace WebDava.Entities;

public class ResourceInfoEntity
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public bool IsDirectory { get; set; }
    public long Length { get; set; }
    public DateTime LastWriteTimeUtc { get; set; }
    public string ETag { get; set; } = string.Empty;
    public string? ContentType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
}