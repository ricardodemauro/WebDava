# WebDava Project Architecture and Folder Structure

## Overview
The WebDava project is a WebDAV server built using C# and .NET 8. The architecture follows modern software design principles, including the use of the Command Query Responsibility Segregation (CQRS) pattern, repository pattern, and Data Transfer Objects (DTOs). This ensures scalability, maintainability, and testability.

## Technology Stack

- .NET Version: .NET 8 or later
- Web Framework: ASP.NET Core Minimal API
- Dependency Injection: Microsoft.Extensions.DependencyInjection
- Logging: Microsoft.Extensions.Logging (Serilog optional)
- XML Processing: System.Xml
- Testing: xUnit & FluentAssertions

## Updated Architecture: API Handlers

### Overview
To simplify the structure and improve maintainability, each API endpoint will be implemented in a single file. This file will include:
1. The request model (command or query).
2. The response model (DTO).
3. The handler logic (command or query handler).

This approach ensures that all related logic for an endpoint is encapsulated in one place, making it easier to understand and modify.

### Minimal APIs
- The project will use **Minimal APIs** instead of traditional controllers to handle HTTP requests.
- Each API handler will be implemented as a standalone file, encapsulating the request, response, and handler logic.
- This approach simplifies the codebase and aligns with modern .NET practices for lightweight APIs.

### Folder Structure

```
/src
  /WebDavaServer
    /ApiHandlers
      /Options
        - OptionsRequest.cs
        - OptionsResponse.cs
        - OptionsHandler.cs
      /Propfind
        - PropfindRequest.cs
        - PropfindResponse.cs
        - PropfindHandler.cs
      … (Get, Put, Mkcol, Delete, Move, Copy)
    /Entities
      - ResourceInfo.cs
      - WebDavResource.cs   (if needed)
    /Repositories
      - IStorageRepository.cs
      - FileSystemStorageRepository.cs
    /Middlewares
      - AuthenticationMiddleware.cs
      - RequestLoggingMiddleware.cs
    /Configuration
      - WebDavOptions.cs
      - DependencyInjection.cs
    /Utilities
      - PathHelper.cs
      - HeaderHelper.cs
    Program.cs
    WebDavaServer.csproj

/tests
  /WebDavaServer.Tests
    (xUnit + FluentAssertions)
```

### Explanation of Changes

#### ApiHandlers
- Purpose: Encapsulate all logic for an API endpoint in a single folder.
- Structure:
  - `[EndpointName]Request.cs`: Defines the request model (command or query).
  - `[EndpointName]Response.cs`: Defines the response model (DTO).
  - `[EndpointName]Handler.cs`: Implements the handler logic for the request.

- Repository Pattern: IStorageRepository abstracts file system operations with specific methods for WebDAV resource management.

- Options Pattern: Use IOptions<WebDavOptions> to inject configuration settings (root path, feature toggles).

- Middleware Pattern: Global authentication and authorization enforcement before reaching endpoints.

- XML Processing per Handler: Each Handler manages its own XML request parsing and response generation using System.Xml.

#### Storage Repository Interface
```csharp
public interface IStorageRepository
{
    Task<ResourceInfo> GetResourceAsync(string path, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResourceInfo>> ListResourcesAsync(string path, CancellationToken cancellationToken = default);
    Task SaveResourceAsync(string path, Stream content, CancellationToken cancellationToken = default);
    Task DeleteResourceAsync(string path, CancellationToken cancellationToken = default);
    Task MoveResourceAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task CopyResourceAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    Task CreateCollectionAsync(string path, CancellationToken cancellationToken = default);
}
```