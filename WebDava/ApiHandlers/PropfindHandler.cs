namespace WebDava.ApiHandlers;

using Microsoft.AspNetCore.Http;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Threading.Tasks;

public static class PropfindHandler
{
    public static async Task HandleAsync(HttpContext context)
    {
        var depth = context.Request.Headers["Depth"].ToString();
        if (string.IsNullOrEmpty(depth)) depth = "1"; // Default to Depth: 1

        var targetPath = Path.Combine("wwwroot", context.Request.Path.Value?.TrimStart('/') ?? string.Empty);

        if (!Directory.Exists(targetPath) && !File.Exists(targetPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var responseXml = new XDocument(
            new XElement("D:multistatus",
                new XAttribute(XNamespace.Xmlns + "D", "DAV:"),
                GenerateResponse(targetPath, depth)
            )
        );

        context.Response.StatusCode = StatusCodes.Status207MultiStatus;
        context.Response.ContentType = "application/xml; charset=utf-8";
        await context.Response.WriteAsync(responseXml.ToString(), Encoding.UTF8);
    }

    private static XElement GenerateResponse(string path, string depth)
    {
        var responses = new List<XElement>();

        if (Directory.Exists(path))
        {
            responses.Add(CreateResponseElement(path, isDirectory: true));

            if (depth == "1" || depth == "infinity")
            {
                foreach (var entry in Directory.GetFileSystemEntries(path))
                {
                    responses.Add(CreateResponseElement(entry, isDirectory: Directory.Exists(entry)));
                }
            }
        }
        else if (File.Exists(path))
        {
            responses.Add(CreateResponseElement(path, isDirectory: false));
        }

        return new XElement("D:multistatus", responses);
    }

    private static XElement CreateResponseElement(string path, bool isDirectory)
    {
        var fileInfo = new FileInfo(path);
        return new XElement("D:response",
            new XElement("D:href", $"/{path.Replace("\\", "/")}"),
            new XElement("D:propstat",
                new XElement("D:prop",
                    new XElement("D:displayname", fileInfo.Name),
                    isDirectory
                        ? new XElement("D:resourcetype", new XElement("D:collection"))
                        : new XElement("D:resourcetype"),
                    new XElement("D:getlastmodified", fileInfo.LastWriteTimeUtc.ToString("R")),
                    !isDirectory ? new XElement("D:getcontentlength", fileInfo.Length) : null
                ),
                new XElement("D:status", "HTTP/1.1 200 OK")
            )
        );
    }
}