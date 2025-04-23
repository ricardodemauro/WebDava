using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// OPTIONS handler for WebDAV
app.MapMethods("/webdav", new[] { "OPTIONS" }, async context =>
{
    context.Response.StatusCode = StatusCodes.Status200OK;
    context.Response.Headers.Append("Allow", "OPTIONS, GET, PUT, DELETE, MKCOL, COPY, MOVE, PROPFIND, PROPPATCH");
    context.Response.Headers.Append("DAV", "1,2");
    context.Response.Headers.Append("MS-Author-Via", "DAV");
    context.Response.ContentLength = 0;
    await context.Response.CompleteAsync();
});

app.MapGet("/", () => "Hello World!");

app.Run();
