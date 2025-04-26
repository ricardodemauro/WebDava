using System;
using WebDava.Entities;

namespace WebDava.Repositories;

public interface IStorageRepository
{
    Task<bool> ResourceExists(string path, CancellationToken cancellationToken = default);

    Task<ResourceInfo> GetResource(string path, CancellationToken cancellationToken = default);

    Task<Stream> GetResourceStream(string path, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceInfo>> ListResources(string path, CancellationToken cancellationToken = default);

    Task SaveResource(string path, Stream content, CancellationToken cancellationToken = default);

    Task DeleteResource(string path, CancellationToken cancellationToken = default);

    Task MoveResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);

    Task CopyResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    
    Task CreateCollection(string path, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ResourceInfo>> GetChildren(string path);
}
