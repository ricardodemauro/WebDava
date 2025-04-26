namespace WebDava.ApiHandlers;

using Microsoft.AspNetCore.Http;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using WebDava.Repositories;
using WebDava.Entities;

public static class PropfindHandler
{
    public static async Task HandleAsync(HttpContext context)
    {
        var storage = context.RequestServices.GetRequiredService<IStorageRepository>();
        var depth = context.Request.Headers["Depth"].ToString();
        if (string.IsNullOrEmpty(depth)) depth = "1"; // Default to Depth: 1

        var targetPath = context.Request.Path.Value?.TrimStart('/') ?? string.Empty;
        var resource = await storage.GetResource(targetPath);

        if (resource == null || !resource.IsValid)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        var davNamespace = XNamespace.Get("DAV:");

        var responseXml = new XDocument(
            new XElement(davNamespace + "multistatus",
                new XAttribute(XNamespace.Xmlns + "D", davNamespace.NamespaceName),
                await GenerateResponse(storage, targetPath, depth)
            )
        );

        context.Response.StatusCode = StatusCodes.Status207MultiStatus;
        context.Response.ContentType = "application/xml; charset=utf-8";
        await context.Response.WriteAsync(responseXml.ToString(), Encoding.UTF8);
    }

    static async Task<XElement> GenerateResponse(IStorageRepository storage, string path, string depth)
    {
        var responses = new List<XElement>();

        ResourceInfo resource = await storage.GetResource(path);
        if (resource != null && resource.IsValid)
        {
            responses.Add(CreateResponseElement(resource));

            if (resource.IsDirectory && (depth == "1" || depth == "infinity"))
            {
                var children = await storage.GetChildren(path);
                foreach (var child in children)
                {
                    responses.Add(CreateResponseElement(child));
                }
            }
        }

        XNamespace ns = "DAV:";
        return new XElement(ns + "multistatus", responses);
    }

    private static XElement CreateResponseElement(ResourceInfo resource)
    {
        XNamespace ns = "DAV:";

        return new XElement(ns + "response",
            new XElement(ns + "href", $"/{resource.Path.Replace("\\", "/")}"),
            new XElement(ns + "propstat",
                new XElement(ns + "prop",
                    new XElement(ns + "displayname", resource.Name),
                    resource.IsDirectory
                        ? new XElement(ns + "resourcetype", new XElement(ns + "collection"))
                        : new XElement(ns + "resourcetype"),
                    new XElement(ns + "getlastmodified", resource.LastWriteTimeUtc.ToString("R")),
                    !resource.IsDirectory ? new XElement(ns + "getcontentlength", resource.Length) : null
                ),
                new XElement(ns + "status", "HTTP/1.1 200 OK")
            )
        );
    }
}