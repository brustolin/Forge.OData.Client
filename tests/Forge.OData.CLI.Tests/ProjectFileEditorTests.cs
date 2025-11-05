using Forge.OData.CLI.Utilities;
using System.Xml.Linq;
using Xunit;

namespace Forge.OData.CLI.Tests;

public class ProjectFileEditorTests
{
    [Fact]
    public void AddAdditionalFile_AddsFileToProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, "TestProject.csproj");
        var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>";
        File.WriteAllText(projectPath, projectContent);

        try
        {
            // Act
            ProjectFileEditor.AddAdditionalFile(projectPath, "metadata.xml");

            // Assert
            var doc = XDocument.Load(projectPath);
            var additionalFiles = doc.Root!.Descendants("AdditionalFiles")
                .Where(e => e.Attribute("Include")?.Value == "metadata.xml");
            Assert.Single(additionalFiles);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void AddAdditionalFile_WhenFileAlreadyExists_DoesNotDuplicate()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, "TestProject.csproj");
        var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <AdditionalFiles Include=""metadata.xml"" />
  </ItemGroup>
</Project>";
        File.WriteAllText(projectPath, projectContent);

        try
        {
            // Act
            ProjectFileEditor.AddAdditionalFile(projectPath, "metadata.xml");

            // Assert
            var doc = XDocument.Load(projectPath);
            var additionalFiles = doc.Root!.Descendants("AdditionalFiles")
                .Where(e => e.Attribute("Include")?.Value == "metadata.xml");
            Assert.Single(additionalFiles);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void AddCompileFile_AddsFileToProject()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, "TestProject.csproj");
        var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
</Project>";
        File.WriteAllText(projectPath, projectContent);

        try
        {
            // Act
            ProjectFileEditor.AddCompileFile(projectPath, "Client.cs");

            // Assert
            var doc = XDocument.Load(projectPath);
            var compileFiles = doc.Root!.Descendants("Compile")
                .Where(e => e.Attribute("Include")?.Value == "Client.cs");
            Assert.Single(compileFiles);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
