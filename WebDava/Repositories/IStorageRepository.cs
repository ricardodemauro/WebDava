using System.IO.Pipelines;
using WebDava.Entities;
using WebDava.Helpers;

namespace WebDava.Repositories;

public interface IStorageRepository
{
    Task<bool> ResourceExists(string path, CancellationToken cancellationToken = default);

    Task<ResourceInfo> GetResource(string path, CancellationToken cancellationToken = default);

    Task<Stream> GetResourceStream(string path, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceInfo>> ListResources(string path, CancellationToken cancellationToken = default);

    Task<Result> SaveResource(string path, Stream content, CancellationToken cancellationToken = default);

    Task<Result> DeleteResource(string path, CancellationToken cancellationToken = default);

    Task<Result> MoveResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    Task<Result> CopyResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    
    Task<Result<ResourceInfo>> CreateCollection(string path, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ResourceInfo>> GetChildren(string path);
    
    Task<Result> SaveResource(string path, PipeReader bodyReader, CancellationToken cancellationToken);
}
