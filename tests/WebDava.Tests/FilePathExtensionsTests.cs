using FluentAssertions;
using WebDava;
using WebDava.Configurations;

namespace WebDava.Tests;

public class FilePathExtensionsTests
{
    [Fact]
    public void IsFullPath_WithFullPath_ReturnsTrue()
    {
        var fullPath = Path.Combine(Path.GetTempPath(), "file.txt");
        FilePathExtensions.IsFullPath(fullPath).Should().BeTrue();
    }

    [Fact]
    public void AsFullPath_WithRelativePath_CombinesStoragePath()
    {
        var options = new StorageOptions { StoragePath = "/root" };
        var result = "data/file.txt".AsFullPath(options);
        result.Should().Be(Path.Combine(options.StoragePath, "data/file.txt"));
    }
}
