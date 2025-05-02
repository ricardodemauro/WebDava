using System.Net;
using WebDava.Repositories;

namespace WebDava.ApiHandlers;

public static class DeleteHandler
{
    public static async Task HandleAsync(HttpContext context)
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
            var storage = context.RequestServices.GetRequiredService<IStorageRepository>();

            var resource = await storage.GetResource(path, context.RequestAborted);
            if (resource == null || !resource.IsValid)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsync("Resource not found.");
                return;
            }

            await storage.DeleteResource(path, context.RequestAborted);

            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            await context.Response.WriteAsync("Access denied.");
        }
        catch (DirectoryNotFoundException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsync("Resource not found.");
        }
        catch (IOException ex) when (ex.Message.Contains("not empty"))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            await context.Response.WriteAsync("Directory is not empty.");
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