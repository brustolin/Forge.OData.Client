using System.CommandLine;
using Forge.OData.CLI.Commands;

namespace Forge.OData.CLI;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("OData CLI tool for managing OData metadata in projects");

        // Add command
        var addCommand = new Command("add", "Add OData metadata to a project");
        var endpointOption = new Option<string>(
            aliases: new[] { "--endpoint", "-e" },
            description: "The OData service endpoint URL")
        {
            IsRequired = true
        };
        var projectOption = new Option<string?>(
            aliases: new[] { "--project", "-p" },
            description: "Path to the project file (defaults to current directory)");
        var clientNameOption = new Option<string?>(
            aliases: new[] { "--client-name", "-n" },
            description: "Name for the generated client class");

        addCommand.AddOption(endpointOption);
        addCommand.AddOption(projectOption);
        addCommand.AddOption(clientNameOption);
        addCommand.SetHandler(async (endpoint, project, clientName) =>
        {
            await AddCommand.Execute(endpoint, project, clientName);
        }, endpointOption, projectOption, clientNameOption);

        // Update command
        var updateCommand = new Command("update", "Update OData metadata from the server");
        var updateProjectOption = new Option<string?>(
            aliases: new[] { "--project", "-p" },
            description: "Path to the project file (defaults to current directory)");

        updateCommand.AddOption(updateProjectOption);
        updateCommand.SetHandler(async (project) =>
        {
            await UpdateCommand.Execute(project);
        }, updateProjectOption);

        rootCommand.AddCommand(addCommand);
        rootCommand.AddCommand(updateCommand);

        return await rootCommand.InvokeAsync(args);
    }
}
