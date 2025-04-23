namespace WebDava.ApiHandlers;

using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

public static class GetHandler
{
    public static async Task HandleAsync(HttpContext context)
    {
        var filePath = Path.Combine("wwwroot", context.Request.Path.Value?.TrimStart('/') ?? string.Empty);

        if (!File.Exists(filePath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var fileInfo = new FileInfo(filePath);

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers.Append("Content-Type", MimeTypeHelper.GetMimeType(fileInfo.Extension));
        context.Response.Headers.Append("Content-Length", fileInfo.Length.ToString());
        context.Response.Headers.Append("Last-Modified", fileInfo.LastWriteTimeUtc.ToString("R"));
        context.Response.Headers.Append("ETag", $"\"{fileInfo.LastWriteTimeUtc.Ticks}\"");
        context.Response.Headers.Append("Accept-Ranges", "bytes");

        await context.Response.SendFileAsync(filePath);
    }
}