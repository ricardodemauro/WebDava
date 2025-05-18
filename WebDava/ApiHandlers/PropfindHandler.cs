namespace WebDava.ApiHandlers;

using Microsoft.AspNetCore.Http;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using WebDava.Repositories;
using WebDava.Entities;
using WebDava.Models.Requests;
using WebDava.Models.Responses;

public class PropFindRequest : WebDavRequestBase
{
    /// <summary>
    /// The Depth header value. Can be 0, 1, or "infinity"
    /// </summary>
    public string Depth { get; set; } = "1";
    
    /// <summary>
    /// The XML body of the PROPFIND request
    /// </summary>
    public XDocument? RequestBody { get; set; }
}

public class PropFindResponse : WebDavResponseBase
{
    /// <summary>
    /// The XML response body to return
    /// </summary>
    public XDocument ResponseBody { get; set; } = new XDocument();
}

public class PropfindHandler(IStorageRepository storage)
{
    readonly IStorageRepository _storage = storage;

    public static async Task HandleAsync(HttpContext context)
    {
        // Create the request from the HttpContext
        var request = await CreatePropFindRequestFromContext(context);
        
        // Create handler instance with injected dependencies
        var storage = context.RequestServices.GetRequiredService<IStorageRepository>();
        var handler = new PropfindHandler(storage);
        
        // Process the request
        var response = await handler.HandleAsync(request);
        
        // Apply the response back to the HttpContext
        await ApplyPropFindResponseToContext(response, context);
    }
    
    static async Task<PropFindRequest> CreatePropFindRequestFromContext(HttpContext context)
    {
        var request = new PropFindRequest
        {
            Path = context.Request.RouteValues["path"]?.ToString() ?? string.Empty,
            Depth = context.Request.Headers["Depth"].ToString(),
            Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        };
        
        // Set default depth if not provided
        if (string.IsNullOrEmpty(request.Depth)) request.Depth = "1";
        
        // Parse request body if present
        if (context.Request.ContentLength > 0)
        {
            try
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                var bodyContent = await reader.ReadToEndAsync();
                request.RequestBody = XDocument.Parse(bodyContent);
            }
            catch
            {
                // If XML parsing fails, leave RequestBody as null
            }
        }
        
        return request;
    }
    
    static async Task ApplyPropFindResponseToContext(PropFindResponse response, HttpContext context)
    {
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = response.ContentType;
        
        if (response.ResponseBody.Root != null)
        {
            await context.Response.WriteAsync(response.ResponseBody.ToString(), Encoding.UTF8);
        }
    }

    public async Task<PropFindResponse> HandleAsync(PropFindRequest request)
    {
        var targetPath = request.Path.TrimStart('/');
        var resource = await _storage.GetResource(targetPath);

        if (resource == null || !resource.IsValid)
        {
            return new PropFindResponse
            {
                StatusCode = StatusCodes.Status404NotFound
            };
        }

        var davNamespace = XNamespace.Get("DAV:");

        var responseXml = new XDocument(
            new XElement(davNamespace + "multistatus",
                new XAttribute(XNamespace.Xmlns + "D", davNamespace.NamespaceName),
                await GenerateResponse(targetPath, request.Depth)
            )
        );

        return new PropFindResponse
        {
            StatusCode = StatusCodes.Status207MultiStatus,
            ContentType = "application/xml; charset=utf-8",
            ResponseBody = responseXml
        };
    }

    async Task<XElement> GenerateResponse(string path, string depth)
    {
        var responses = new List<XElement>();

        ResourceInfo resource = await _storage.GetResource(path);
        if (resource != null && resource.IsValid)
        {
            responses.Add(CreateResponseElement(resource));

            if (resource.IsDirectory && (depth == "1" || depth == "infinity"))
            {
                var children = await _storage.GetChildren(path);
                foreach (var child in children)
                {
                    responses.Add(CreateResponseElement(child));
                }
            }
        }

        XNamespace ns = "DAV:";
        return new XElement(ns + "multistatus", responses);
    }

    static XElement CreateResponseElement(ResourceInfo resource)
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