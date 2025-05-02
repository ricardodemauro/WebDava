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

**Async Suffix Policy:**
- Do **not** use the `Async` suffix for asynchronous methods. All asynchronous methods should be named without the `Async` suffix, for consistency and brevity.
- Example: Use `GetResource`, `ListResources`, `SaveResource`, etc., instead of `GetResourceAsync`.

```csharp
public interface IStorageRepository
{
    Task<ResourceInfo> GetResource(string path, CancellationToken cancellationToken = default);

    Task<IEnumerable<ResourceInfo>> ListResources(string path, CancellationToken cancellationToken = default);
    
    Task SaveResource(string path, Stream content, CancellationToken cancellationToken = default);
    
    Task DeleteResource(string path, CancellationToken cancellationToken = default);
    
    Task MoveResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    
    Task CopyResource(string sourcePath, string destinationPath, CancellationToken cancellationToken = default);
    
    Task CreateCollection(string path, CancellationToken cancellationToken = default);
}
```

---

## Access Modifiers: Avoid Redundant `private`

In C#, fields, methods, and members are private by default in classes and primary constructors. Do not use the `private` keyword where it is redundant, as this improves code clarity and reduces noise.

**Fields and Constructor Parameters:**
```csharp
public class MyClass(string name)
{
    readonly string _name = name;
    // ...
}
```

**Not this:**
```csharp
public class MyClass(private string name)
{
    private readonly string _name = name;
    // ...
}
```

**Methods:**
```csharp
public class MyClass
{
    void DoWork() { /* ... */ }
}
```

**Not this:**
```csharp
public class MyClass
{
    private void DoWork() { /* ... */ }
}
```

**Rationale:**
- The `private` modifier is implied for fields, methods, and constructor parameters unless another access modifier is specified.
- Redundant use of `private` adds unnecessary clutter.

## Formatting: Empty Lines Between Members

Always insert a single empty line between fields, properties, methods, and other members in a class or struct. This improves readability and helps visually separate logical sections of code.

**Example:**
```csharp
public class MyClass
{
    int _value;

    string _name;

    public MyClass(int value, string name)
    {
        _value = value;
        _name = name;
    }

    void DoWork()
    {
        // ...
    }

    int GetValue() => _value;
}
```