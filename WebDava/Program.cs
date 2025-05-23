using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using WebDava.ApiHandlers;
using WebDava.Configurations;
using WebDava.Repositories;
using WebDava.Models;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        LogEventLevel.Information,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] [{SourceContext}] {Message}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.Configure<StorageOptions>(c => c.StoragePath = builder.Configuration["StoragePath"] ?? string.Empty);
builder.Services.AddDbContext<WebDavDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("WebDavSqlite")));
builder.Services.AddTransient<IStorageRepository, SqliteStorageRepository>();
builder.Services.AddSingleton<StorageOptions>(sp => sp.GetRequiredService<IOptions<StorageOptions>>().Value);

builder.Services.AddTransient<PropfindHandler>();

var app = builder.Build();

app.UseSerilogRequestLogging();

// Middleware to log requests and responses with additional details
app.Use(async (context, next) =>
{
    var connectionId = context.Connection.Id;
    var method = context.Request.Method;
    var pathWithQuery = context.Request.Path + context.Request.QueryString;
    var headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
    var userAgent = headers.TryGetValue("User-Agent", out string? value) ? value : string.Empty;

    Log.Information("Incoming request: {ConnectionId} {Method} {PathWithQuery} {Headers}", connectionId, method, pathWithQuery, headers);
    await next.Invoke();
    Log.Information("Response: {StatusCode}", context.Response.StatusCode);
});

// Register the OPTIONS handler for WebDAV
app.MapMethods("/webdav", ["OPTIONS"], OptionsHandler.HandleAsync);

app.MapMethods("/", ["OPTIONS"], OptionsHandler.HandleAsync);

app.MapMethods("/webdav/{*path}", ["HEAD"], HeadHandler.HandleAsync);

app.MapMethods("/webdav/{*path}", ["PROPFIND"], PropfindHandler.HandleAsync);

app.MapMethods("/webdav/{*path}", ["GET"], GetHandler.HandleAsync);

app.MapMethods("/webdav/{*path}", ["MKCOL"], MkcolHandler.HandleAsync);

app.MapMethods("/webdav/{*path}", ["PUT"], PutHandler.HandleAsync);

app.MapGet("/", () => "Hello World!");

app.MapMethods("/webdav/{*path}", [ "LOCK" ], LockHandler.HandleAsync);

app.MapMethods("/webdav/{*path}", ["DELETE"], DeleteHandler.HandleAsync);

app.MapMethods("/webdav/{*path}", ["PROPPATCH"], PropPatchHandler.HandleAsync);

app.Run();
