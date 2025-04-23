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

app.MapGet("/", () => "Hello World!");

app.Run();
