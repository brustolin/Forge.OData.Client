using Forge.OData.CLI.Utilities;
using Xunit;

namespace Forge.OData.CLI.Tests;

public class ProjectHelperTests
{
    [Fact]
    public void FindProjectFile_WithValidProjectPath_ReturnsFullPath()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var projectPath = Path.Combine(tempDir, "TestProject.csproj");
        File.WriteAllText(projectPath, "<Project Sdk=\"Microsoft.NET.Sdk\"></Project>");

        try
        {
            // Act
            var result = ProjectHelper.FindProjectFile(projectPath);

            // Assert
            Assert.Equal(Path.GetFullPath(projectPath), result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void FindProjectFile_WithNonExistentPath_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "nonexistent", "project.csproj");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => ProjectHelper.FindProjectFile(nonExistentPath));
    }

    [Fact]
    public void FindProjectFile_WithMultipleProjectsInDirectory_ThrowsInvalidOperationException()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var currentDir = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(tempDir);
            File.WriteAllText(Path.Combine(tempDir, "Project1.csproj"), "<Project />");
            File.WriteAllText(Path.Combine(tempDir, "Project2.csproj"), "<Project />");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => ProjectHelper.FindProjectFile(null));
        }
        finally
        {
            // Cleanup
            Directory.SetCurrentDirectory(currentDir);
            Directory.Delete(tempDir, true);
        }
    }
}
