namespace WebDava.ApiHandlers;

using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using WebDava.Repositories;

public static class HeadHandler
{
    public static async Task HandleAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var fileName = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;

        var storage = context.RequestServices.GetRequiredService<IStorageRepository>();
        var resource = await storage.GetResource(fileName, cancellationToken);
        if (resource == null || !resource.IsValid)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if(cancellationToken.IsCancellationRequested)
        {
            context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers.Append("Content-Type", resource.ContentType); // Adjust MIME type as needed
        context.Response.Headers.Append("Content-Length", resource.Length.ToString());
        context.Response.Headers.Append("Last-Modified", resource.LastWriteTimeUtc.ToString("R"));
        context.Response.Headers.Append("ETag", resource.ETag);
        context.Response.Headers.Append("Accept-Ranges", "bytes");

        await context.Response.CompleteAsync();
    }
}