namespace WebDava.Models.Requests;

using System.Collections.Generic;

public class WebDavRequestBase
{
    /// <summary>
    /// The path of the resource being accessed
    /// </summary>
    public string Path { get; set; } = string.Empty;
    
    /// <summary>
    /// Headers from the HTTP request
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
}
