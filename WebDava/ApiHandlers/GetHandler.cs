namespace WebDava.ApiHandlers;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.IO;
using System.Threading.Tasks;
using WebDava.Repositories;

public static class GetHandler
{
    private const int StreamCopyBufferSize = 64 * 1024;

    public static async Task HandleAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var fileName = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;

        var storage = context.RequestServices.GetRequiredService<IStorageRepository>();
        var resource = await storage.GetResource(fileName);
        if (resource == null || !resource.IsValid)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            context.Response.StatusCode = StatusCodes.Status408RequestTimeout;
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers.Append("Content-Type", MimeTypeHelper.GetMimeType(resource.Extension));
        context.Response.Headers.Append("Content-Length", resource.Length.ToString());
        context.Response.Headers.Append("Last-Modified", resource.LastWriteTimeUtc.ToString("R"));
        context.Response.Headers.Append("ETag", $"\"{resource.LastWriteTimeUtc.Ticks}\"");
        context.Response.Headers.Append("Accept-Ranges", "bytes");

        var stream = await storage.GetResourceStream(fileName, cancellationToken).ConfigureAwait(false);
        //await context.Response.SendFileAsync(stream, cancellationToken);
        await StreamCopyOperation.CopyToAsync(stream, context.Response.Body, 0, StreamCopyBufferSize, cancellationToken);
    }
}