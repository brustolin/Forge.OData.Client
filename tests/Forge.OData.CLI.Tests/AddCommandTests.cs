using Forge.OData.CLI.Commands;
using Xunit;

namespace Forge.OData.CLI.Tests;

public class AddCommandTests
{
    [Fact]
    public void DeriveNamespaceFromPath_WithEmptyPath_ReturnsProjectName()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            // Act
            var result = InvokeDeriveNamespaceFromPath(tempDir, "");

            // Assert
            Assert.Equal("MyProject", result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DeriveNamespaceFromPath_WithSingleFolder_ReturnsCombinedNamespace()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            // Act
            var result = InvokeDeriveNamespaceFromPath(tempDir, "odata");

            // Assert
            Assert.Equal("MyProject.odata", result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DeriveNamespaceFromPath_WithNestedFolders_ReturnsDottedNamespace()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            // Act
            var result = InvokeDeriveNamespaceFromPath(tempDir, "odata/clients");

            // Assert
            Assert.Equal("MyProject.odata.clients", result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DeriveNamespaceFromPath_WithBackslashes_ReturnsDottedNamespace()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, "MyProject.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            // Act
            var result = InvokeDeriveNamespaceFromPath(tempDir, "odata\\clients");

            // Assert
            Assert.Equal("MyProject.odata.clients", result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void DeriveNamespaceFromPath_WithNoProjectFile_ReturnsDefaultNamespace()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = InvokeDeriveNamespaceFromPath(tempDir, "odata");

            // Assert
            Assert.Equal("ODataClients.odata", result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    // Helper method to invoke the private DeriveNamespaceFromPath method using reflection
    private static string InvokeDeriveNamespaceFromPath(string projectDir, string relativePath)
    {
        var method = typeof(AddCommand).GetMethod(
            "DeriveNamespaceFromPath",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        if (method == null)
        {
            throw new InvalidOperationException("DeriveNamespaceFromPath method not found");
        }

        var result = method.Invoke(null, new object[] { projectDir, relativePath });
        return result?.ToString() ?? string.Empty;
    }
}
