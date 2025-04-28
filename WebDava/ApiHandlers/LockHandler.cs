using Microsoft.AspNetCore.Http;
using System.Net;
using System.Xml;

namespace WebDava.ApiHandlers;

public static class LockHandler
{
    public static async Task HandleAsync(HttpContext context)
    {
        var path = context.Request.Path.ToString().TrimStart('/');

        if (string.IsNullOrWhiteSpace(path))
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync("Path cannot be null or empty.");
            return;
        }

        string[] contentTypes = ["text/xml", "application/xml"];
        if (!contentTypes.Any(ct => context.Request.ContentType?.Contains(ct) ?? false))
        {
            context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
            await context.Response.WriteAsync("Content-Type must be text/xml or application/xml.");
            return;
        }

        try
        {
            var bodyReader = context.Request.BodyReader; // 1 MB limit
            using var m = new MemoryStream();
            await bodyReader.CopyToAsync(m, context.RequestAborted);
            m.Seek(0, SeekOrigin.Begin);

            using var reader = XmlReader.Create(m, new XmlReaderSettings { IgnoreWhitespace = true });
            string lockScope = null;
            string lockType = null;
            string owner = null;

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "D:lockscope":
                            reader.Read();
                            lockScope = reader.Name;
                            break;
                        case "D:locktype":
                            reader.Read();
                            lockType = reader.Name;
                            break;
                        case "D:owner":
                            reader.Read();
                            owner = reader.ReadElementContentAsString();
                            break;
                    }
                }
            }

            if (string.IsNullOrEmpty(lockScope) || string.IsNullOrEmpty(lockType) || string.IsNullOrEmpty(owner))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync("Invalid lock request body.");
                return;
            }

            // Simulate lock creation (this should be replaced with actual lock storage logic)
            var lockToken = Guid.NewGuid().ToString();

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.Headers["Lock-Token"] = $"opaquelocktoken:{lockToken}";
            context.Response.ContentType = "application/xml; charset=utf-8";

            await context.Response.WriteAsync($"<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" +
                $"<D:prop xmlns:D=\"DAV:\">\n" +
                $"  <D:lockdiscovery>\n" +
                $"    <D:activelock>\n" +
                $"      <D:locktype><{lockType}/></D:locktype>\n" +
                $"      <D:lockscope><{lockScope}/></D:lockscope>\n" +
                $"      <D:owner>{owner}</D:owner>\n" +
                $"      <D:timeout>Second-3600</D:timeout>\n" +
                $"      <D:locktoken><D:href>opaquelocktoken:{lockToken}</D:href></D:locktoken>\n" +
                $"    </D:activelock>\n" +
                $"  </D:lockdiscovery>\n" +
                $"</D:prop>");
        }
        catch (XmlException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync("Malformed XML in request body.");
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await context.Response.WriteAsync($"An unexpected error occurred: {ex.Message}");
        }
    }
}