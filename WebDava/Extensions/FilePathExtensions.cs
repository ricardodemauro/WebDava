using System;
using WebDava.Configurations;
using WebDava.Entities;

namespace WebDava;

public static class FilePathExtensions
{
    public static bool IsFullPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        return Path.IsPathFullyQualified(path);
    }

    public static string AsFullPath(this string path, StorageOptions storageOptions)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        if (IsFullPath(path)) return path;

        return Path.Combine(storageOptions.StoragePath, path.TrimStart('/'));
    }

    public static bool IsDirectory(this FileInfo fileInfo)
    {
        if (fileInfo == null)
            throw new ArgumentNullException(nameof(fileInfo), "FileInfo cannot be null");

        return (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
    }

    public static bool IsDirectory(this FileSystemInfo fileInfo)
    {
        if (fileInfo == null)
            throw new ArgumentNullException(nameof(fileInfo), "FileInfo cannot be null");

        return (fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
    }

    public static ResourceInfo AsResourceInfo(this FileInfo info)
    {
        return new ResourceInfo
        {
            Path = info.FullName,
            Name = info.Name,
            IsDirectory = info.IsDirectory(),
            Length = info.IsDirectory() ? 0 : info.Length,
            LastWriteTimeUtc = info.LastWriteTimeUtc,
            IsValid = true,
            ContentType = MimeTypeHelper.GetMimeType(info.Name),
            ETag = info.LastWriteTimeUtc.Ticks.ToString(),
            Extension = info.IsDirectory() ? string.Empty : Path.GetExtension(info.Name)
        };
    }
}

