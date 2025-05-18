namespace WebDava.Models;

using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using WebDava.Models.Requests;
using WebDava.Models.Responses;
using WebDava.ApiHandlers;

public static class WebDavModelConverter
{
    /// <summary>
    /// Creates a PropFindRequest from an HttpContext
    /// </summary>
    public static async Task<PropFindRequest> CreatePropFindRequestFromContext(HttpContext context)
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
    
    /// <summary>
    /// Applies a PropFindResponse to an HttpContext
    /// </summary>
    public static async Task ApplyPropFindResponseToContext(PropFindResponse response, HttpContext context)
    {
        context.Response.StatusCode = response.StatusCode;
        context.Response.ContentType = response.ContentType;
        
        if (response.ResponseBody.Root != null)
        {
            await context.Response.WriteAsync(response.ResponseBody.ToString(), Encoding.UTF8);
        }
    }
}
