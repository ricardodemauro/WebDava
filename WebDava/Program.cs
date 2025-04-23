using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WebDava.ApiHandlers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Register the OPTIONS handler for WebDAV
app.MapMethods("/webdav", ["OPTIONS"], OptionsHandler.HandleAsync);

// Register the HEAD handler for WebDAV
app.MapMethods("/webdav/{*path}", ["HEAD"], HeadHandler.HandleAsync);

// Register the PROPFIND handler for WebDAV
app.MapMethods("/webdav/{*path}", ["PROPFIND"], PropfindHandler.HandleAsync);

// Register the GET handler for WebDAV
app.MapMethods("/webdav/{*path}", ["GET"], GetHandler.HandleAsync);

app.MapGet("/", () => "Hello World!");

app.Run();
