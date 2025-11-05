using Forge.OData.CLI.Commands;
using Xunit;

namespace Forge.OData.CLI.Tests;

public class AddCommandTests
{
    [Fact]
    public void DeriveNamespaceFromPath_WithEmptyPath_ReturnsProjectName()
    {
        // Arrange & Act
        var result = WithTempProject("MyProject", projectDir =>
        {
            return InvokeDeriveNamespaceFromPath(projectDir, "");
        });

        // Assert
        Assert.Equal("MyProject", result);
    }

    [Fact]
    public void DeriveNamespaceFromPath_WithSingleFolder_ReturnsCombinedNamespace()
    {
        // Arrange & Act
        var result = WithTempProject("MyProject", projectDir =>
        {
            return InvokeDeriveNamespaceFromPath(projectDir, "odata");
        });

        // Assert
        Assert.Equal("MyProject.odata", result);
    }

    [Fact]
    public void DeriveNamespaceFromPath_WithNestedFolders_ReturnsDottedNamespace()
    {
        // Arrange & Act
        var result = WithTempProject("MyProject", projectDir =>
        {
            return InvokeDeriveNamespaceFromPath(projectDir, "odata/clients");
        });

        // Assert
        Assert.Equal("MyProject.odata.clients", result);
    }

    [Fact]
    public void DeriveNamespaceFromPath_WithBackslashes_ReturnsDottedNamespace()
    {
        // Arrange & Act
        var result = WithTempProject("MyProject", projectDir =>
        {
            return InvokeDeriveNamespaceFromPath(projectDir, "odata\\clients");
        });

        // Assert
        Assert.Equal("MyProject.odata.clients", result);
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

    [Fact]
    public void ExtractNamespaceFromMetadata_WithValidMetadata_ReturnsNamespace()
    {
        // Arrange
        var metadata = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"" Version=""4.0"">
    <edmx:DataServices>
        <Schema xmlns=""http://docs.oasis-open.org/odata/ns/edm"" Namespace=""MyCompany.Services.Models"">
            <EntityType Name=""Product"">
                <Key><PropertyRef Name=""Id"" /></Key>
                <Property Name=""Id"" Type=""Edm.Int32"" Nullable=""false"" />
            </EntityType>
        </Schema>
    </edmx:DataServices>
</edmx:Edmx>";

        // Act
        var result = InvokeExtractNamespaceFromMetadata(metadata);

        // Assert
        Assert.Equal("MyCompany.Services.Models", result);
    }

    [Fact]
    public void ExtractNamespaceFromMetadata_WithMultipleSchemas_ReturnsFirstNamespace()
    {
        // Arrange
        var metadata = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"" Version=""4.0"">
    <edmx:DataServices>
        <Schema xmlns=""http://docs.oasis-open.org/odata/ns/edm"" Namespace=""First.Namespace"">
            <EntityType Name=""Product"" />
        </Schema>
        <Schema xmlns=""http://docs.oasis-open.org/odata/ns/edm"" Namespace=""Second.Namespace"">
            <EntityType Name=""Order"" />
        </Schema>
    </edmx:DataServices>
</edmx:Edmx>";

        // Act
        var result = InvokeExtractNamespaceFromMetadata(metadata);

        // Assert
        Assert.Equal("First.Namespace", result);
    }

    [Fact]
    public void ExtractNamespaceFromMetadata_WithNoNamespace_ReturnsNull()
    {
        // Arrange
        var metadata = @"<?xml version=""1.0"" encoding=""utf-8""?>
<edmx:Edmx xmlns:edmx=""http://docs.oasis-open.org/odata/ns/edmx"" Version=""4.0"">
    <edmx:DataServices>
        <Schema xmlns=""http://docs.oasis-open.org/odata/ns/edm"">
            <EntityType Name=""Product"" />
        </Schema>
    </edmx:DataServices>
</edmx:Edmx>";

        // Act
        var result = InvokeExtractNamespaceFromMetadata(metadata);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractNamespaceFromMetadata_WithInvalidXml_ReturnsNull()
    {
        // Arrange
        var metadata = "not valid xml";

        // Act
        var result = InvokeExtractNamespaceFromMetadata(metadata);

        // Assert
        Assert.Null(result);
    }

    // Helper method to create a temporary project directory and clean it up
    private static T WithTempProject<T>(string projectName, Func<string, T> action)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, $"{projectName}.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            return action(tempDir);
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

    // Helper method to invoke the private ExtractNamespaceFromMetadata method using reflection
    private static string? InvokeExtractNamespaceFromMetadata(string metadata)
    {
        var method = typeof(AddCommand).GetMethod(
            "ExtractNamespaceFromMetadata",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        
        if (method == null)
        {
            throw new InvalidOperationException("ExtractNamespaceFromMetadata method not found");
        }

        var result = method.Invoke(null, new object[] { metadata });
        return result?.ToString();
    }
}
