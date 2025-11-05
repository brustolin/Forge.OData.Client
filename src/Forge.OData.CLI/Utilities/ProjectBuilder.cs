using Buildalyzer;
using Microsoft.Build.Locator;

namespace Forge.OData.CLI.Utilities;

public static class ProjectBuilder
{
    private static bool _msbuildRegistered = false;

    public static Task<string> BuildProject(string projectPath)
    {
        // Register MSBuild - only do this once
        if (!_msbuildRegistered)
        {
            var instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
            if (instances.Length > 0)
            {
                MSBuildLocator.RegisterInstance(instances.OrderByDescending(x => x.Version).First());
                _msbuildRegistered = true;
            }
        }

        var manager = new AnalyzerManager();
        var analyzer = manager.GetProject(projectPath);

        // Build the project
        var results = analyzer.Build();
        var result = results.FirstOrDefault();

        if (result == null || !result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to build project. Check the project for compilation errors.");
        }

        // Get the output assembly path
        var assemblyPath = result.Properties.TryGetValue("TargetPath", out var path)
            ? path
            : throw new InvalidOperationException("Could not determine output assembly path");

        if (!File.Exists(assemblyPath))
        {
            throw new InvalidOperationException($"Assembly not found at: {assemblyPath}");
        }

        return Task.FromResult(assemblyPath);
    }
}
