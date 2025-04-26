using Serilog;
using WebDava.ApiHandlers;
using WebDava.Configurations;
using WebDava.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<StorageOptions>(c => c.StoragePath = builder.Configuration["StoragePath"] ?? string.Empty);
builder.Services.AddTransient<IStorageRepository, FileStorageRepository>();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Middleware to log requests and responses with additional details
app.Use(async (context, next) =>
{
    var connectionId = context.Connection.Id;
    var method = context.Request.Method;
    var pathWithQuery = context.Request.Path + context.Request.QueryString;

    Log.Information("Incoming request: {ConnectionId} {Method} {PathWithQuery}", connectionId, method, pathWithQuery);
    await next.Invoke();
    Log.Information("Response: {StatusCode}", context.Response.StatusCode);
});

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
