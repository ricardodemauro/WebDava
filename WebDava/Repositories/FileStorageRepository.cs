using WebDava.Configurations;
using WebDava.Entities;

namespace WebDava.Repositories;

public static class FilePathExtensions
{
    public static string AsFullPath(this string path, StorageOptions storageOptions)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        return Path.Combine(storageOptions.StoragePath, path.TrimStart('/'));
    }
}

public class FileStorageRepository(StorageOptions options) : IStorageRepository
{
    public Task CopyResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("Source path cannot be null or empty", nameof(sourcePath));
        }

        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            throw new ArgumentException("Destination path cannot be null or empty", nameof(destinationPath));
        }

        var sPath = sourcePath.AsFullPath(options);
        var dPath = destinationPath.AsFullPath(options);

        if (!File.Exists(sPath) && !Directory.Exists(sPath))
        {
            throw new FileNotFoundException("The source resource does not exist.", sourcePath);
        }

        if (File.Exists(sPath))
        {
            File.Copy(sPath, dPath, overwrite: true);
        }
        else if (Directory.Exists(sPath))
        {
            CopyDirectory(sPath, dPath);
        }

        return Task.CompletedTask;
    }

    static void CopyDirectory(string sourceDir, string destinationDir)
    {
        var dir = new DirectoryInfo(sourceDir);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        }

        Directory.CreateDirectory(destinationDir);

        foreach (var file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, overwrite: true);
        }

        foreach (var subDir in dir.GetDirectories())
        {
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir);
        }
    }

    public Task CreateCollection(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        var sPath = path.AsFullPath(options);

        if (Directory.Exists(sPath))
        {
            throw new IOException("The specified directory already exists.");
        }

        Directory.CreateDirectory(sPath);
        return Task.CompletedTask;
    }

    public Task DeleteResource(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        var sPath = path.AsFullPath(options);

        if (File.Exists(sPath))
        {
            File.Delete(sPath);
        }
        else if (Directory.Exists(sPath))
        {
            Directory.Delete(sPath, recursive: true);
        }
        else
        {
            throw new FileNotFoundException("The specified resource does not exist.", sPath);
        }

        return Task.CompletedTask;
    }

    public Task<ResourceInfo> GetResource(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        var sPath = path.AsFullPath(options);

        if (!File.Exists(sPath) && !Directory.Exists(sPath))
        {
            throw new FileNotFoundException("The specified resource does not exist.", sPath);
        }

        var fileInfo = new FileInfo(sPath);
        var isDirectory = (File.GetAttributes(sPath) & FileAttributes.Directory) == FileAttributes.Directory;

        return Task.FromResult(new ResourceInfo
        {
            Path = path,
            Name = fileInfo.Name,
            IsDirectory = isDirectory,
            Length = isDirectory ? 0 : fileInfo.Length,
            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
            ContentType = isDirectory ? null : MimeTypeHelper.GetMimeType(fileInfo.Name)
        });
    }

    public Task<Stream> GetResourceStream(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        var sPath = path.AsFullPath(options);

        if (!File.Exists(sPath))
        {
            throw new FileNotFoundException("The specified file does not exist.", sPath);
        }

        return Task.FromResult((Stream)File.OpenRead(sPath));
    }

    public async Task<IEnumerable<ResourceInfo>> ListResources(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        var sPath = path.AsFullPath(options);

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException("The specified directory does not exist.");
        }

        var directoryInfo = new DirectoryInfo(path);
        var resources = directoryInfo.EnumerateFileSystemInfos().Select(info => new ResourceInfo
        {
            Path = info.FullName,
            Name = info.Name,
            IsDirectory = (info.Attributes & FileAttributes.Directory) == FileAttributes.Directory,
            Length = (info.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? 0 : ((FileInfo)info).Length,
            LastWriteTimeUtc = info.LastWriteTimeUtc
        });

        return await Task.FromResult(resources);
    }

    public async Task MoveResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
        {
            throw new ArgumentException("Source path cannot be null or empty", nameof(sourcePath));
        }

        if (string.IsNullOrWhiteSpace(destinationPath))
        {
            throw new ArgumentException("Destination path cannot be null or empty", nameof(destinationPath));
        }

        var sPath = sourcePath.AsFullPath(options);
        var dPath = destinationPath.AsFullPath(options);

        if (!File.Exists(sPath) && !Directory.Exists(sPath))
        {
            throw new FileNotFoundException("The source resource does not exist.", sPath);
        }

        if (File.Exists(sPath))
        {
            File.Move(sPath, dPath);
        }
        else if (Directory.Exists(sPath))
        {
            Directory.Move(sPath, dPath);
        }

        await Task.CompletedTask;
    }

    public Task<bool> ResourceExists(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        var sPath = path.AsFullPath(options);
        return Task.FromResult(File.Exists(sPath) || Directory.Exists(sPath));
    }

    public async Task SaveResource(string path, Stream content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        if (content == null)
        {
            throw new ArgumentNullException(nameof(content), "Content stream cannot be null");
        }

        var sPath = path.AsFullPath(options);

        var directory = Path.GetDirectoryName(sPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var fileStream = new FileStream(sPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, cancellationToken);
    }
}
