using Microsoft.EntityFrameworkCore;
using WebDava.Entities;
using WebDava.Configurations;
using System.IO.Pipelines;
using WebDava.Helpers;

namespace WebDava.Repositories;

public class SqliteStorageRepository(
    WebDavDbContext db, 
    StorageOptions options, 
    ILogger<SqliteStorageRepository> logger) : IStorageRepository
{
    readonly WebDavDbContext _db = db;

    readonly StorageOptions _options = options;

    readonly ILogger<SqliteStorageRepository> _logger = logger;

    private string GetPhysicalPath(string path) =>
        Path.Combine(_options.StoragePath, path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

    public async Task<bool> ResourceExists(string path, CancellationToken cancellationToken = default)
    {
        return await _db.Resources.AsNoTracking().AnyAsync(r => r.Path == path, cancellationToken);
    }

    public async Task<ResourceInfo> GetResource(string path, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Path == path, cancellationToken);
        if (entity == null) return ResourceInfo.EmptyResourceInfo(path);
        return new ResourceInfo
        {
            Name = entity.Name,
            Extension = entity.Extension,
            LastWriteTimeUtc = entity.LastWriteTimeUtc,
            ETag = entity.ETag,
            ContentType = entity.ContentType,
            Length = entity.Length,
            IsDirectory = entity.IsDirectory,
            Path = entity.Path,
            IsValid = true
        };
    }

    public Task<Stream> GetResourceStream(string path, CancellationToken cancellationToken = default)
    {
        var physicalPath = GetPhysicalPath(path);
        if (!File.Exists(physicalPath)) throw new FileNotFoundException();
        return Task.FromResult((new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read)) as Stream);
    }

    public async Task<IEnumerable<ResourceInfo>> ListResources(string path, CancellationToken cancellationToken = default)
    {
        var prefix = path.TrimEnd('/') + "/";
        var entities = await _db.Resources.AsNoTracking()
            .Where(r => r.Path.StartsWith(prefix) && r.Path != path)
            .ToListAsync(cancellationToken);
        return entities.Select(entity => new ResourceInfo
        {
            Name = entity.Name,
            Extension = entity.Extension,
            LastWriteTimeUtc = entity.LastWriteTimeUtc,
            ETag = entity.ETag,
            ContentType = entity.ContentType,
            Length = entity.Length,
            IsDirectory = entity.IsDirectory,
            Path = entity.Path,
            IsValid = true
        });
    }

    public async Task<Result> SaveResource(string path, Stream content, CancellationToken cancellationToken = default)
    {
        try
        {
            var physicalPath = GetPhysicalPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            using (var fs = new FileStream(physicalPath, FileMode.Create, FileAccess.Write))
            {
                await content.CopyToAsync(fs, cancellationToken);
            }
            var now = DateTime.UtcNow;
            var fileInfo = new FileInfo(physicalPath);
            var entity = await _db.Resources.FirstOrDefaultAsync(r => r.Path == path, cancellationToken);
            if (entity == null)
            {
                entity = new ResourceInfoEntity
                {
                    Path = path,
                    Name = Path.GetFileName(path),
                    Extension = Path.GetExtension(path),
                    LastWriteTimeUtc = now,
                    ETag = Guid.NewGuid().ToString(),
                    ContentType = MimeTypeHelper.GetMimeType(Path.GetFileName(path)),
                    Length = fileInfo.Length,
                    IsDirectory = false
                };
                _db.Resources.Add(entity);
            }
            else
            {
                entity.LastWriteTimeUtc = now;
                entity.ETag = Guid.NewGuid().ToString();
                entity.Length = fileInfo.Length;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SaveResource for path {Path}", path);
            return Result.Failure($"Error saving resource: {ex.Message}");
        }
    }

    public async Task<Result> DeleteResource(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _db.Resources.FirstOrDefaultAsync(r => r.Path == path, cancellationToken);
            if (entity != null)
            {
                _db.Resources.Remove(entity);
                await _db.SaveChangesAsync(cancellationToken);
            }
            var physicalPath = GetPhysicalPath(path);
            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
            else if (Directory.Exists(physicalPath))
            {
                Directory.Delete(physicalPath, true);
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteResource for path {Path}", path);
            return Result.Failure($"Error deleting resource: {ex.Message}");
        }
    }

    public async Task<Result> MoveResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var srcPhysical = GetPhysicalPath(sourcePath);
            var dstPhysical = GetPhysicalPath(destinationPath);
            if (File.Exists(srcPhysical))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dstPhysical)!);
                File.Move(srcPhysical, dstPhysical, true);
            }
            else if (Directory.Exists(srcPhysical))
            {
                Directory.Move(srcPhysical, dstPhysical);
                // Update all child resources in the database
                var prefix = sourcePath.TrimEnd('/') + "/";
                var newPrefix = destinationPath.TrimEnd('/') + "/";
                var children = await _db.Resources.Where(r => r.Path.StartsWith(prefix)).ToListAsync(cancellationToken);
                foreach (var child in children)
                {
                    child.Path = newPrefix + child.Path.Substring(prefix.Length);
                    child.Name = Path.GetFileName(child.Path);
                    child.Extension = Path.GetExtension(child.Path);
                    child.LastWriteTimeUtc = DateTime.UtcNow;
                }
            }
            var entity = await _db.Resources.FirstOrDefaultAsync(r => r.Path == sourcePath, cancellationToken);
            if (entity != null)
            {
                entity.Path = destinationPath;
                entity.Name = Path.GetFileName(destinationPath);
                entity.Extension = Path.GetExtension(destinationPath);
                entity.LastWriteTimeUtc = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in MoveResource from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
            return Result.Failure($"Error moving resource: {ex.Message}");
        }
    }

    public async Task<Result> CopyResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var srcPhysical = GetPhysicalPath(sourcePath);
            var dstPhysical = GetPhysicalPath(destinationPath);
            if (File.Exists(srcPhysical))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dstPhysical)!);
                File.Copy(srcPhysical, dstPhysical, true);
            }
            else if (Directory.Exists(srcPhysical))
            {
                // Recursively copy directory contents
                await CopyDirectoryRecursive(sourcePath, destinationPath, cancellationToken);
            }
            var srcEntity = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Path == sourcePath, cancellationToken);
            if (srcEntity != null)
            {
                var newEntity = new ResourceInfoEntity
                {
                    Path = destinationPath,
                    Name = Path.GetFileName(destinationPath),
                    Extension = Path.GetExtension(destinationPath),
                    LastWriteTimeUtc = DateTime.UtcNow,
                    ETag = Guid.NewGuid().ToString(),
                    ContentType = srcEntity.ContentType,
                    Length = srcEntity.Length,
                    IsDirectory = srcEntity.IsDirectory
                };
                _db.Resources.Add(newEntity);
                await _db.SaveChangesAsync(cancellationToken);
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CopyResource from {SourcePath} to {DestinationPath}", sourcePath, destinationPath);
            return Result.Failure($"Error copying resource: {ex.Message}");
        }
    }

    // Helper for recursive directory copy
    async Task CopyDirectoryRecursive(string sourcePath, string destinationPath, CancellationToken cancellationToken)
    {
        var srcPhysical = GetPhysicalPath(sourcePath);
        var dstPhysical = GetPhysicalPath(destinationPath);
        Directory.CreateDirectory(dstPhysical);

        // Copy directory resource entry
        var srcEntity = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Path == sourcePath, cancellationToken);
        if (srcEntity != null)
        {
            var newEntity = new ResourceInfoEntity
            {
                Path = destinationPath,
                Name = Path.GetFileName(destinationPath),
                Extension = srcEntity.Extension,
                LastWriteTimeUtc = DateTime.UtcNow,
                ETag = Guid.NewGuid().ToString(),
                ContentType = srcEntity.ContentType,
                Length = srcEntity.Length,
                IsDirectory = srcEntity.IsDirectory
            };
            _db.Resources.Add(newEntity);
            await _db.SaveChangesAsync(cancellationToken);
        }

        // Copy files
        foreach (var file in Directory.GetFiles(srcPhysical))
        {
            var fileName = Path.GetFileName(file);
            var destFilePath = Path.Combine(dstPhysical, fileName);
            File.Copy(file, destFilePath, true);
            var childSourcePath = sourcePath.TrimEnd('/') + "/" + fileName;
            var childDestPath = destinationPath.TrimEnd('/') + "/" + fileName;
            var childEntity = await _db.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Path == childSourcePath, cancellationToken);
            if (childEntity != null)
            {
                var newChildEntity = new ResourceInfoEntity
                {
                    Path = childDestPath,
                    Name = fileName,
                    Extension = childEntity.Extension,
                    LastWriteTimeUtc = DateTime.UtcNow,
                    ETag = Guid.NewGuid().ToString(),
                    ContentType = childEntity.ContentType,
                    Length = childEntity.Length,
                    IsDirectory = false
                };
                _db.Resources.Add(newChildEntity);
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        // Recursively copy subdirectories
        foreach (var dir in Directory.GetDirectories(srcPhysical))
        {
            var dirName = Path.GetFileName(dir);
            var childSourcePath = sourcePath.TrimEnd('/') + "/" + dirName;
            var childDestPath = destinationPath.TrimEnd('/') + "/" + dirName;
            await CopyDirectoryRecursive(childSourcePath, childDestPath, cancellationToken);
        }
    }

    public async Task<Result<ResourceInfo>> CreateCollection(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            var physicalPath = GetPhysicalPath(path);
            Directory.CreateDirectory(physicalPath);
            var now = DateTime.UtcNow;
            var entity = await _db.Resources.FirstOrDefaultAsync(r => r.Path == path, cancellationToken);
            if (entity == null)
            {
                entity = new ResourceInfoEntity
                {
                    Path = path,
                    Name = Path.GetFileName(path),
                    Extension = string.Empty,
                    LastWriteTimeUtc = now,
                    ETag = Guid.NewGuid().ToString(),
                    ContentType = null,
                    Length = 0,
                    IsDirectory = true
                };
                _db.Resources.Add(entity);
                await _db.SaveChangesAsync(cancellationToken);
            }
            var result = new ResourceInfo
            {
                Name = entity.Name,
                Extension = entity.Extension,
                LastWriteTimeUtc = entity.LastWriteTimeUtc,
                ETag = entity.ETag,
                ContentType = entity.ContentType,
                Length = entity.Length,
                IsDirectory = entity.IsDirectory,
                Path = entity.Path,
                IsValid = true
            };
            return Result<ResourceInfo>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateCollection for path {Path}", path);
            return Result<ResourceInfo>.Failure($"Error creating collection: {ex.Message}");
        }
    }

    public async Task<IEnumerable<ResourceInfo>> GetChildren(string path)
    {
        var prefix = path.TrimEnd('/') + "/";
        var entities = await _db.Resources.AsNoTracking()
            .Where(r => r.Path.StartsWith(prefix) && r.Path != path)
            .ToListAsync();
        var result = entities.Select(entity => new ResourceInfo
        {
            Name = entity.Name,
            Extension = entity.Extension,
            LastWriteTimeUtc = entity.LastWriteTimeUtc,
            ETag = entity.ETag,
            ContentType = entity.ContentType,
            Length = entity.Length,
            IsDirectory = entity.IsDirectory,
            Path = entity.Path,
            IsValid = true
        });
        return result;
    }

    public async Task<Result> SaveResource(string path, PipeReader bodyReader, CancellationToken cancellationToken)
    {
        try
        {
            var physicalPath = GetPhysicalPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            using var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None);
            while (true)
            {
                var result = await bodyReader.ReadAsync(cancellationToken);
                var buffer = result.Buffer;
                foreach (var segment in buffer)
                {
                    await fileStream.WriteAsync(segment, cancellationToken);
                }
                bodyReader.AdvanceTo(buffer.End);
                if (result.IsCompleted) break;
            }
            await fileStream.FlushAsync(cancellationToken);
            // Save or update resource entry in the database
            return await SaveResource(path, Stream.Null, cancellationToken); // Reuse logic for DB update
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SaveResource (PipeReader) for path {Path}", path);
            return Result.Failure($"Error saving resource: {ex.Message}");
        }
    }
}