using System.Reflection;
using System.IO;

namespace WebDava.Helpers;

public static class XsdHelper
{
    public static Stream OpenWebDavXsdStream()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "WebDava.xsds.webdav.xsd";

        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");
        }

        return stream;
    }
}