namespace WebDava.Models.Responses;

public class WebDavResponseBase
{
    /// <summary>
    /// HTTP status code to return
    /// </summary>
    public int StatusCode { get; set; } = 200;
    
    /// <summary>
    /// Content type for the response
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
}
