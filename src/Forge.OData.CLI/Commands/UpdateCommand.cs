using System.Reflection;
using Forge.OData.Attributes;
using Forge.OData.CLI.Utilities;

namespace Forge.OData.CLI.Commands;

public static class UpdateCommand
{
    public static async Task Execute(string? projectPath)
    {
        try
        {
            Console.WriteLine("Updating OData metadata...");

            // Find the project file
            var project = ProjectHelper.FindProjectFile(projectPath);
            Console.WriteLine($"Using project: {project}");

            // Build the project
            Console.WriteLine("Building project...");
            var assemblyPath = await ProjectBuilder.BuildProject(project);
            Console.WriteLine($"Built assembly: {assemblyPath}");

            // Load the assembly and find classes with ODataClientAttribute
            Console.WriteLine("Scanning for OData clients...");
            var clients = AssemblyScanner.FindODataClients(assemblyPath);

            if (clients.Count == 0)
            {
                Console.WriteLine("No OData client classes found with [ODataClient] attribute.");
                Console.WriteLine("Use 'dotnet odata add' to add OData metadata to your project.");
                return;
            }

            Console.WriteLine($"Found {clients.Count} OData client(s)");

            var projectDir = Path.GetDirectoryName(project)!;
            var updatedCount = 0;

            foreach (var client in clients)
            {
                Console.WriteLine();
                Console.WriteLine($"Processing: {client.ClassName}");
                Console.WriteLine($"  Endpoint: {client.Endpoint}");
                Console.WriteLine($"  Metadata file: {client.MetadataFile}");

                try
                {
                    // Download updated metadata
                    var metadataUrl = client.Endpoint.TrimEnd('/') + "/$metadata";
                    Console.WriteLine($"  Downloading from {metadataUrl}...");
                    var metadata = await MetadataDownloader.DownloadMetadata(metadataUrl);

                    // Update the metadata file
                    var metadataFilePath = Path.Combine(projectDir, client.MetadataFile);
                    await File.WriteAllTextAsync(metadataFilePath, metadata);
                    Console.WriteLine($"  ✓ Updated {client.MetadataFile}");
                    updatedCount++;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"  ✗ Failed to update: {ex.Message}");
                }
            }

            Console.WriteLine();
            if (updatedCount > 0)
            {
                Console.WriteLine($"✓ Successfully updated {updatedCount} metadata file(s)!");
                Console.WriteLine();
                Console.WriteLine("Next steps:");
                Console.WriteLine("1. Rebuild your project to regenerate the OData client code");
            }
            else
            {
                Console.WriteLine("No metadata files were updated.");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }
}
