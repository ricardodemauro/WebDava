using System;

namespace WebDava;

public static class PathStringExtensions
{
    public static string NormalizedString(this PathString path)
    {
        return path.ToString().TrimStart('/');
    }
}
