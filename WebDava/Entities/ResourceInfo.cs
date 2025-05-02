using System;
using System.Security.Cryptography.X509Certificates;

namespace WebDava.Entities;

public class ResourceInfo
{
    public string Name { get; set; } = string.Empty;

    public string Extension { get; set; } = string.Empty;

    public DateTime LastWriteTimeUtc { get; set; }

    public string ETag { get; set; } = string.Empty;

    public string? ContentType { get; set; } = string.Empty;

    public long Length { get; set; }

    public bool IsDirectory { get; set; } = false;

    public string Path { get; set; } = string.Empty;

    public bool IsValid { get; set; } = true;

    public static ResourceInfo EmptyResourceInfo(string path)
    {
        return new ResourceInfo
        {
            Name = string.Empty,
            Extension = string.Empty,
            LastWriteTimeUtc = DateTime.UtcNow,
            ETag = string.Empty,
            ContentType = null,
            Length = 0,
            IsDirectory = false,
            Path = path,
            IsValid = false
        };
    }
}
