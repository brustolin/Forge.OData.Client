using System.Text;
using Forge.OData.CLI.Utilities;

namespace Forge.OData.CLI.Commands;

public static class AddCommand
{
    public static async Task Execute(string endpoint, string? projectPath, string? clientName)
    {
        try
        {
            Console.WriteLine($"Adding OData metadata from endpoint: {endpoint}");

            // Find the project file
            var project = ProjectHelper.FindProjectFile(projectPath);
            Console.WriteLine($"Using project: {project}");

            // Download metadata from endpoint
            Console.WriteLine("Downloading metadata...");
            var metadataUrl = endpoint.TrimEnd('/') + "/$metadata";
            var metadata = await MetadataDownloader.DownloadMetadata(metadataUrl);
            
            // Determine client name
            var effectiveClientName = clientName ?? GenerateClientName(endpoint);
            Console.WriteLine($"Client class name: {effectiveClientName}");

            // Save metadata file
            var projectDir = Path.GetDirectoryName(project)!;
            var metadataFileName = $"{effectiveClientName}Metadata.xml";
            var metadataFilePath = Path.Combine(projectDir, metadataFileName);
            
            if (File.Exists(metadataFilePath))
            {
                Console.WriteLine($"Warning: File {metadataFileName} already exists. Overwriting...");
            }
            
            await File.WriteAllTextAsync(metadataFilePath, metadata);
            Console.WriteLine($"Metadata saved to: {metadataFileName}");

            // Add metadata file to project
            ProjectFileEditor.AddAdditionalFile(project, metadataFileName);
            Console.WriteLine($"Added {metadataFileName} to project as AdditionalFiles");

            // Generate client class file
            var clientClassName = effectiveClientName;
            var clientFileName = $"{clientClassName}.cs";
            var clientFilePath = Path.Combine(projectDir, clientFileName);

            if (File.Exists(clientFilePath))
            {
                Console.WriteLine($"Warning: File {clientFileName} already exists. Skipping class generation.");
            }
            else
            {
                var clientCode = GenerateClientClass(clientClassName, metadataFileName, endpoint);
                await File.WriteAllTextAsync(clientFilePath, clientCode);
                Console.WriteLine($"Generated client class: {clientFileName}");

                // Add client class to project
                ProjectFileEditor.AddCompileFile(project, clientFileName);
                Console.WriteLine($"Added {clientFileName} to project");
            }

            Console.WriteLine();
            Console.WriteLine("✓ OData metadata added successfully!");
            Console.WriteLine($"  Client class: {clientClassName}");
            Console.WriteLine($"  Metadata file: {metadataFileName}");
            Console.WriteLine();
            Console.WriteLine("Next steps:");
            Console.WriteLine("1. Build your project to generate the OData client code");
            Console.WriteLine($"2. Use the {clientClassName} class to access the OData service");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static string GenerateClientName(string endpoint)
    {
        // Try to extract a meaningful name from the endpoint URL
        var uri = new Uri(endpoint);
        var segments = uri.Segments
            .Where(s => s != "/" && !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim('/'))
            .ToList();

        if (segments.Any())
        {
            var lastSegment = segments.Last();
            // Remove common OData path segments
            if (lastSegment.Equals("odata", StringComparison.OrdinalIgnoreCase))
            {
                lastSegment = segments.Count > 1 ? segments[^2] : "OData";
            }

            // Sanitize the name
            var sanitized = SanitizeClassName(lastSegment);
            return sanitized + "Client";
        }

        return "ODataClient";
    }

    private static string SanitizeClassName(string name)
    {
        // Remove invalid characters and ensure valid C# identifier
        var sb = new StringBuilder();
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }

        var result = sb.ToString();
        
        // Ensure it doesn't start with a digit
        if (result.Length > 0 && char.IsDigit(result[0]))
        {
            result = "_" + result;
        }

        return string.IsNullOrEmpty(result) ? "OData" : result;
    }

    private static string GenerateClientClass(string className, string metadataFile, string endpoint)
    {
        // Get the namespace from the current directory or use a default
        var defaultNamespace = "ODataClients";

        return $@"using Forge.OData.Attributes;

namespace {defaultNamespace}
{{
    /// <summary>
    /// OData client for {endpoint}
    /// </summary>
    [ODataClient(MetadataFile = ""{metadataFile}"", Endpoint = ""{endpoint}"")]
    public partial class {className}
    {{
        // Custom properties and methods can be added here
    }}
}}
";
    }
}
