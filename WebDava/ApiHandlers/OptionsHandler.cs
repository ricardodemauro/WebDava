namespace WebDava.ApiHandlers;

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public static class OptionsHandler
{
    public static async Task HandleAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.Headers.Append("Allow", "OPTIONS, GET, PUT, DELETE, MKCOL, COPY, MOVE, PROPFIND, PROPPATCH");
        context.Response.Headers.Append("DAV", "1,2");
        context.Response.Headers.Append("MS-Author-Via", "DAV");
        context.Response.ContentLength = 0;
        await context.Response.CompleteAsync();
    }
}