using Microsoft.AspNetCore.Http;
using WebDava.Repositories;

namespace WebDava.ApiHandlers;

public class MkcolHandler
{
    public async Task HandleAsync(HttpContext context)
    {
        var path = context.Request.Path.ToString();

        if (string.IsNullOrWhiteSpace(path))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync("Path cannot be null or empty.");
            return;
        }

        var storage = context.RequestServices.GetRequiredService<IStorageRepository>();
        var resource = await storage.CreateCollection(path);
        
        if (resource == null || !resource.IsValid)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsync("Resource already exists or is invalid.");
            return;
        }
        context.Response.StatusCode = StatusCodes.Status201Created;
    }
}