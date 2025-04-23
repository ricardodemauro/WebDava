# WebDava Project Architecture and Folder Structure

## Overview
The WebDava project is a WebDAV server built using C# and .NET 8. The architecture follows modern software design principles, including the use of the Command Query Responsibility Segregation (CQRS) pattern, repository pattern, and Data Transfer Objects (DTOs). This ensures scalability, maintainability, and testability.

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
WebDava/
├── ApiHandlers/
│   ├── [EndpointName]/
│   │   ├── [EndpointName]Request.cs
│   │   ├── [EndpointName]Response.cs
│   │   └── [EndpointName]Handler.cs
├── Application/
│   ├── Interfaces/
│       └── [InterfaceName].cs
├── Domain/
│   ├── Entities/
│   │   └── [EntityName].cs
│   ├── ValueObjects/
│   │   └── [ValueObjectName].cs
│   └── Services/
│       └── [ServiceName].cs
├── Infrastructure/
│   ├── Repositories/
│   │   └── [EntityName]Repository.cs
│   ├── Data/
│   │   └── [DbContextName].cs
│   └── Configurations/
│       └── [EntityName]Configuration.cs
├── Presentation/
│   ├── Middleware/
│       └── [MiddlewareName].cs
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
├── Program.cs
└── WebDava.csproj
```

### Explanation of Changes

#### ApiHandlers
- **Purpose**: Encapsulate all logic for an API endpoint in a single folder.
- **Structure**:
  - `[EndpointName]Request.cs`: Defines the request model (command or query).
  - `[EndpointName]Response.cs`: Defines the response model (DTO).
  - `[EndpointName]Handler.cs`: Implements the handler logic for the request.

#### Application
- Retains interfaces for shared application logic.

#### Domain
- Unchanged, continues to define core entities, value objects, and domain services.

#### Infrastructure
- Unchanged, continues to manage data access and external integrations.

#### Presentation
- Middleware remains here for cross-cutting concerns.

### Benefits
- **Encapsulation**: All logic for an endpoint is in one place.
- **Maintainability**: Easier to locate and modify endpoint-specific logic.
- **Scalability**: New endpoints can be added without affecting unrelated parts of the codebase.

---
This updated architecture ensures a clean and modular structure while simplifying the development process.