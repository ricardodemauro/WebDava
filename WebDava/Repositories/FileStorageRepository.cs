using System.IO.Pipelines;
using WebDava.Configurations;
using WebDava.Entities;
using WebDava.Helpers;

namespace WebDava.Repositories;

public class FileStorageRepository(StorageOptions options) : IStorageRepository
{
    public Task<Result> CopyResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return Task.FromResult(Result.Failure("Source path cannot be null or empty"));
            if (string.IsNullOrWhiteSpace(destinationPath))
                return Task.FromResult(Result.Failure("Destination path cannot be null or empty"));
            var sPath = sourcePath.AsFullPath(options);
            var dPath = destinationPath.AsFullPath(options);
            if (!File.Exists(sPath) && !Directory.Exists(sPath))
                return Task.FromResult(Result.Failure("The source resource does not exist."));
            if (File.Exists(sPath))
            {
                File.Copy(sPath, dPath, overwrite: true);
            }
            else if (Directory.Exists(sPath))
            {
                CopyDirectory(sPath, dPath);
            }
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure($"Error copying resource: {ex.Message}"));
        }
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

    public Task<Result<ResourceInfo>> CreateCollection(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(Result<ResourceInfo>.Failure("Path cannot be null or empty"));
            var sPath = path.AsFullPath(options);
            var directoryInfo = Directory.CreateDirectory(sPath);
            var resource = new ResourceInfo
            {
                Path = directoryInfo.FullName,
                Name = directoryInfo.Name,
                IsDirectory = true,
                IsValid = true,
                LastWriteTimeUtc = directoryInfo.LastWriteTimeUtc
            };
            return Task.FromResult(Result<ResourceInfo>.Success(resource));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<ResourceInfo>.Failure($"Error creating collection: {ex.Message}"));
        }
    }

    public Task<Result> DeleteResource(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(Result.Failure("Path cannot be null or empty"));
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
                return Task.FromResult(Result.Failure("The specified resource does not exist."));
            }
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure($"Error deleting resource: {ex.Message}"));
        }
    }

    public Task<ResourceInfo> GetResource(string path, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        var sPath = path.AsFullPath(options);

        if (!File.Exists(sPath) && !Directory.Exists(sPath))
        {
            return Task.FromResult(ResourceInfo.EmptyResourceInfo(path));
        }

        var fileInfo = new FileInfo(sPath);

        return Task.FromResult(fileInfo.AsResourceInfo());
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
        var resources = directoryInfo.EnumerateFiles().Select(info => info.AsResourceInfo());

        return await Task.FromResult(resources);
    }

    public Task<Result> MoveResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return Task.FromResult(Result.Failure("Source path cannot be null or empty"));
            if (string.IsNullOrWhiteSpace(destinationPath))
                return Task.FromResult(Result.Failure("Destination path cannot be null or empty"));
            var sPath = sourcePath.AsFullPath(options);
            var dPath = destinationPath.AsFullPath(options);
            if (!File.Exists(sPath) && !Directory.Exists(sPath))
                return Task.FromResult(Result.Failure("The source resource does not exist."));
            if (File.Exists(sPath))
            {
                File.Move(sPath, dPath);
            }
            else if (Directory.Exists(sPath))
            {
                Directory.Move(sPath, dPath);
            }
            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure($"Error moving resource: {ex.Message}"));
        }
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

    public async Task<Result> SaveResource(string path, PipeReader bodyReader, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return Result.Failure("Path cannot be null or empty");
            if (bodyReader == null)
                return Result.Failure("Content stream cannot be null");
            var sPath = path.AsFullPath(options);
            var directory = Path.GetDirectoryName(sPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using var fileStream = new FileStream(sPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await bodyReader.CopyToAsync(fileStream, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error saving resource: {ex.Message}");
        }
    }

    public async Task<Result> SaveResource(string path, Stream content, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return Result.Failure("Path cannot be null or empty");
            if (content == null)
                return Result.Failure("Content stream cannot be null");
            var sPath = path.AsFullPath(options);
            var directory = Path.GetDirectoryName(sPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            using var fileStream = new FileStream(sPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await content.CopyToAsync(fileStream, cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error saving resource: {ex.Message}");
        }
    }

    public Task<IEnumerable<ResourceInfo>> GetChildren(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        var sPath = path.AsFullPath(options);

        if (!Directory.Exists(sPath))
        {
            throw new DirectoryNotFoundException("The specified directory does not exist.");
        }

        var directoryInfo = new DirectoryInfo(sPath);
        var children = directoryInfo.EnumerateFiles().Select(info => info.AsResourceInfo());

        return Task.FromResult(children);

    }
}
