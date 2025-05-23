using System.Net;
using WebDava.Repositories;

namespace WebDava.ApiHandlers;

public static class PutHandler
{
    public static async Task HandleAsync(HttpContext context, CancellationToken cancellationToken)
    {
        var path = context.Request.Path.NormalizedString();

        if (string.IsNullOrWhiteSpace(path))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync("Path cannot be null or empty.");
            return;
        }

        try
        {
            var bodyReader = context.Request.BodyReader; // 1 MB limit

            var storageRepository = context.RequestServices.GetRequiredService<IStorageRepository>();
            var result = await storageRepository.SaveResource(path, bodyReader, cancellationToken);

            if (result.IsFailure)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(result.Error ?? "Failed to save resource.");
                return;
            }

            if(cancellationToken.IsCancellationRequested)
            {
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                await context.Response.WriteAsync("Request was canceled.");
                return;
            }

            var resourceInfo = await storageRepository.GetResource(path, context.RequestAborted);
            context.Response.StatusCode = (int)HttpStatusCode.Created;
            context.Response.Headers["ETag"] = resourceInfo.ETag;
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Access denied.");
        }
        catch (DirectoryNotFoundException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsync("Parent directory does not exist.");
        }
        catch (IOException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InsufficientStorage;
            await context.Response.WriteAsync("Insufficient storage.");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync($"An unexpected error occurred: {ex.Message}");
        }
    }
}