namespace Forge.OData.CLI.Utilities;

public static class ProjectHelper
{
    public static string FindProjectFile(string? projectPath)
    {
        // If a specific project path is provided, use it
        if (!string.IsNullOrWhiteSpace(projectPath))
        {
            if (File.Exists(projectPath))
            {
                return Path.GetFullPath(projectPath);
            }
            throw new FileNotFoundException($"Project file not found: {projectPath}");
        }

        // Otherwise, search in the current directory
        var currentDir = Directory.GetCurrentDirectory();
        var projectFiles = Directory.GetFiles(currentDir, "*.csproj");

        if (projectFiles.Length == 0)
        {
            throw new InvalidOperationException(
                "No .csproj file found in the current directory. " +
                "Please specify the project path with --project or run from a project directory.");
        }

        if (projectFiles.Length > 1)
        {
            throw new InvalidOperationException(
                $"Multiple .csproj files found in the current directory. " +
                $"Please specify which project to use with --project option.");
        }

        return Path.GetFullPath(projectFiles[0]);
    }
}
