using Microsoft.AspNetCore.Http;
using System.Net;
using System.Xml;

namespace WebDava.ApiHandlers;

public static class PropPatchHandler
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
            var bodyReader = context.Request.BodyReader;
            using var memoryStream = new MemoryStream();
            await bodyReader.CopyToAsync(memoryStream, context.RequestAborted);
            memoryStream.Seek(0, SeekOrigin.Begin);

            using var reader = XmlReader.Create(memoryStream, new XmlReaderSettings { IgnoreWhitespace = true });
            var setProperties = new Dictionary<string, string>();
            var removeProperties = new List<string>();

            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "D:set":
                            reader.ReadToDescendant("D:prop");
                            while (reader.ReadToFollowing("D:*"))
                            {
                                var propName = reader.Name;
                                var propValue = reader.ReadElementContentAsString();
                                setProperties[propName] = propValue;
                            }
                            break;
                        case "D:remove":
                            reader.ReadToDescendant("D:prop");
                            while (reader.ReadToFollowing("D:*"))
                            {
                                removeProperties.Add(reader.Name);
                            }
                            break;
                    }
                }
            }

            // Simulate property updates and removals (replace with actual logic)
            var responseXml = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" +
                              "<D:multistatus xmlns:D=\"DAV:\">\n";

            foreach (var prop in setProperties)
            {
                responseXml += $"  <D:response>\n" +
                               $"    <D:href>/{path}</D:href>\n" +
                               $"    <D:propstat>\n" +
                               $"      <D:prop><D:{prop.Key}/></D:prop>\n" +
                               $"      <D:status>HTTP/1.1 200 OK</D:status>\n" +
                               $"    </D:propstat>\n" +
                               $"  </D:response>\n";
            }

            foreach (var prop in removeProperties)
            {
                responseXml += $"  <D:response>\n" +
                               $"    <D:href>/{path}</D:href>\n" +
                               $"    <D:propstat>\n" +
                               $"      <D:prop><D:{prop}/></D:prop>\n" +
                               $"      <D:status>HTTP/1.1 200 OK</D:status>\n" +
                               $"    </D:propstat>\n" +
                               $"  </D:response>\n";
            }

            responseXml += "</D:multistatus>";

            context.Response.StatusCode = (int)HttpStatusCode.MultiStatus;
            context.Response.ContentType = "application/xml; charset=utf-8";
            await context.Response.WriteAsync(responseXml);
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