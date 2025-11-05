# Forge.OData.CLI

A .NET CLI tool for managing OData metadata in projects.

## Overview

The `odata` tool automates the process of adding and updating OData metadata in your .NET projects, similar to how `dotnet ef` manages Entity Framework migrations.

## Installation

Install the tool globally:

```bash
dotnet tool install --global Forge.OData.CLI
```

Or install it locally in your project:

```bash
dotnet new tool-manifest  # if you don't have one already
dotnet tool install --local Forge.OData.CLI
```

## Usage

### Adding OData Metadata

Download metadata from an OData endpoint and configure your project:

```bash
dotnet odata add --endpoint https://api.example.com/odata --client-name MyService
```

This will:
- Download the `$metadata` from the endpoint
- Save it as `MyServiceMetadata.xml`
- Create a `MyService.cs` file with the `[ODataClient]` attribute
- Add both files to your `.csproj`
- Configure the project for code generation

**Options:**
- `--endpoint, -e` (required): The OData service endpoint URL
- `--client-name, -n` (optional): Name for the client class (auto-generated if not specified)
- `--project, -p` (optional): Path to project file (defaults to current directory)
- `--output-path, -o` (optional): Path where the generated client class should be saved (relative to project directory)
- `--namespace, -ns` (optional): Namespace for the generated client class (if not specified, derived from output path)

### Updating Metadata

Update all OData metadata files in your project from their configured endpoints:

```bash
dotnet odata update
```

This will:
- Build your project
- Find all classes with `[ODataClient]` attribute
- Download updated metadata from each endpoint
- Update the corresponding XML files

**Options:**
- `--project, -p` (optional): Path to project file (defaults to current directory)

## Example Workflow

```bash
# Add OData service to your project
cd MyProject
dotnet odata add --endpoint https://api.example.com/odata --client-name ApiClient

# Build to generate the client code
dotnet build

# Use the generated client
# (Your code here using ApiClient class)

# Later, when the API schema changes
dotnet odata update

# Rebuild to regenerate with updated metadata
dotnet build
```

## Advanced Usage

### Organizing Clients in Subdirectories

Place generated client classes in a specific directory and automatically derive the namespace:

```bash
# Creates: MyProject/OData/Clients/ApiClient.cs
# Namespace: MyProject.OData.Clients
dotnet odata add --endpoint https://api.example.com/odata --client-name ApiClient --output-path OData/Clients
```

### Custom Namespace

Specify a custom namespace for the generated client class:

```bash
# Uses namespace: MyCompany.Services.OData
dotnet odata add --endpoint https://api.example.com/odata --client-name ApiClient --namespace MyCompany.Services.OData
```

### Combining Output Path and Namespace

```bash
# Creates file in Services/OData directory with custom namespace
dotnet odata add --endpoint https://api.example.com/odata \
  --client-name ApiClient \
  --output-path Services/OData \
  --namespace MyCompany.ExternalServices
```

## Requirements

- .NET 9.0 or later
- Your project must reference:
  - `Forge.OData.Attributes`
  - `Forge.OData.Generator` (as analyzer)
  - `Forge.OData.Client`

## How It Works

The CLI tool works in conjunction with the OData source generator:

1. **Add Command**: Downloads metadata, creates a partial class with `[ODataClient]` attribute, and configures the project
2. **Source Generator**: Reads the metadata file and generates client code during compilation
3. **Update Command**: Refreshes metadata files by examining compiled assemblies for `[ODataClient]` attributes

## License

MIT
