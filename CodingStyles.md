# C# Coding Style Guide (Modern Approach)

This guide defines modern C# coding conventions for clean, consistent, and maintainable codebases.

## Naming Conventions

### Classes, Structs, Enums, Interfaces
- **PascalCase**
```csharp
public class OrderService { }
public struct OrderInfo { }
public enum OrderStatus { Pending, Confirmed, Shipped }
public interface IOrderProcessor { }
```

### Methods, Properties, Events
- **PascalCase**
```csharp
public string GetOrderId() => Guid.NewGuid().ToString();
public int Quantity { get; set; }
public event EventHandler OrderPlaced;
```

### Local Variables, Parameters, Fields
- **camelCase**
- Use `_camelCase` for private fields
```csharp
private readonly ILogger _logger;
public void ProcessOrder(Order order) { var orderId = order.Id; }
```

### Constants and Static Readonly Fields
- **PascalCase**
```csharp
public const int MaxRetries = 3;
public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);
```

---

## Code Structure & Layout

### File Structure
- One class per file
- Match file name with class name

### Usings
- Use top-level `using` statements
- Use global usings for shared namespaces (`GlobalUsings.cs`)
```csharp
// GlobalUsings.cs
global using System;
global using System.Collections.Generic;
```

### Namespace Declaration (C# 10+)
- Use file-scoped namespaces
```csharp
namespace MyApp.Services;

public class OrderService { }
```

---

## ✍️ Syntax Preferences

### Expression-bodied Members
```csharp
public string Name => _name;
public override string ToString() => $"{FirstName} {LastName}";
```

### Pattern Matching
```csharp
if (obj is Order order && order.Quantity > 0) { /* ... */ }
```

### Null Coalescing & Null-Conditional Operators
```csharp
var name = customer?.Name ?? "Unknown";
```

### Target-Typed `new` Expressions
```csharp
List<Order> orders = new();
```

### Collection Expressions
- Use collection expressions for concise and readable initialization of collections (C# 12+).
```csharp
var numbers = [1, 2, 3, 4, 5];
var orders = [
    new Order { Id = 1, Name = "Order1" },
    new Order { Id = 2, Name = "Order2" }
];
```

---

## Access Modifiers

- Use the most restrictive access possible
- Avoid `public` unless necessary
- Order: `public`, `internal`, `protected`, `private`

---

## Object Initialization

Use collection and object initializers for readability:
```csharp
var order = new Order
{
    Id = Guid.NewGuid(),
    Items = new List<Item>
    {
        new() { Name = "Apple", Price = 1.2m },
        new() { Name = "Banana", Price = 0.8m }
    }
};
```

---

## Clean Code Practices

- Prefer immutable types (use `record` for models)
- Minimize use of `var` (except when type is obvious)
- Avoid magic numbers (use constants)
- Use meaningful names

---

## Modern Features Checklist

- [x] `record` types for DTOs
- [x] `readonly struct` for value types
- [x] `init` setters for immutability
- [x] `async/await` for asynchronous code
- [x] Nullable reference types (`#nullable enable`)
- [x] Top-level statements (for minimal APIs/CLI)
- [x] File-scoped namespaces
- [x] Target-typed `new`

---

## Example

```csharp
namespace MyApp.Models;

public record Order(Guid Id, string CustomerName, IReadOnlyList<Item> Items);

public record Item(string Name, decimal Price);
```

```csharp
namespace MyApp.Services;

public interface IOrderProcessor
{
    Task ProcessAsync(Order order, CancellationToken cancellationToken = default);
}

public class OrderProcessor(ILogger<OrderProcessor> logger) : IOrderProcessor
{
    public async Task ProcessAsync(Order order, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing order {Id}", order.Id);
        await Task.Delay(500, cancellationToken); // Simulate async work
    }
}
```