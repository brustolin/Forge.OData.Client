# Contributing to Forge.OData.Client

Thank you for your interest in contributing to Forge.OData.Client! This guide will help you understand the project architecture, organization, and how to make effective contributions.

## Table of Contents

- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Repository Organization](#repository-organization)
- [Development Setup](#development-setup)
- [Building and Testing](#building-and-testing)
- [Code Generation Workflow](#code-generation-workflow)
- [Contributing Guidelines](#contributing-guidelines)
- [Release Process](#release-process)
- [Project Structure Details](#project-structure-details)

## Project Overview

Forge.OData.Client is a .NET 9 OData client library with a Roslyn source generator that automatically generates type-safe model classes and client code from OData $metadata XML files. The project focuses on:

- **Performance**: Custom JSON converters eliminate reflection during deserialization
- **Type Safety**: Strongly-typed entity sets and LINQ-to-OData query translation
- **Developer Experience**: Clean code generation using Roslyn's SyntaxFactory API
- **Modern .NET**: Built for .NET 9 with latest C# features

## Architecture

The solution consists of four main components that work together:

### 1. Forge.OData.Generator (Source Generator)

**Location**: `src/Forge.OData.Generator/`

**Purpose**: Roslyn source generator that runs during compilation to generate C# code from OData metadata.

**Key Components**:
- `ODataSourceGenerator.cs`: Main incremental generator implementation
- `ODataMetadataParser.cs`: Parses OData $metadata XML files
- `ModelGenerator.cs`: Generates model classes for EntityTypes and ComplexTypes
- `JsonConverterGenerator.cs`: Generates custom JSON converters for each model
- `ClientGenerator.cs`: Generates standalone client classes
- `PartialClientGenerator.cs`: Generates partial client classes for attribute-based approach

**How It Works**:
1. Detects `.xml` files marked as `AdditionalFiles` in project
2. Validates that XML contains OData metadata
3. Parses metadata to extract EntityTypes, ComplexTypes, and EntitySets
4. Generates C# code using SyntaxFactory for:
   - Model classes with `[Key]` and `[JsonConverter]` attributes
   - Custom `JsonConverter<T>` implementations
   - Client classes with entity set properties
5. Adds generated code to compilation

### 2. Forge.OData.Client (Runtime Library)

**Location**: `src/Forge.OData.Client/`

**Purpose**: Provides runtime utilities for executing OData queries.

**Key Components**:
- `ODataQueryable<T>`: Fluent API for building OData queries
  - Supports LINQ methods: `Where`, `OrderBy`, `OrderByDescending`, `Skip`, `Take`, `Select`, `Expand`
  - Returns new immutable instances for method chaining
  - Executes HTTP requests and deserializes responses
- `ODataExpressionVisitor`: Converts LINQ expressions to OData filter syntax
  - Translates binary expressions (`>`, `<`, `==`, `&&`, `||`)
  - Handles method calls (`StartsWith`, `EndsWith`, `Contains`)
  - Supports property access and constants
- `ODataQueryBuilder`: Builds OData query strings
  - Manages query options (`$filter`, `$select`, `$expand`, `$orderby`, `$skip`, `$top`)
  - Supports cloning for immutable query building

### 3. Forge.OData.Attributes (Attributes Library)

**Location**: `src/Forge.OData.Attributes/`

**Purpose**: Provides declarative attributes for client generation.

**Key Attributes**:
- `ODataClientAttribute`: Marks partial classes for OData client generation
  - `MetadataFile`: Specifies the metadata XML file path
  - `Endpoint`: Sets the default OData service endpoint
  - Enables custom client names and namespaces

### 4. Forge.OData.CLI (Command-Line Tool)

**Location**: `src/Forge.OData.CLI/`

**Purpose**: dotnet tool for managing OData metadata in projects.

**Commands**:
- `add`: Downloads metadata from an OData endpoint and configures the project
  - Downloads `$metadata` XML
  - Creates client class file with `[ODataClient]` attribute
  - Updates `.csproj` to include files and configure generator
- `update`: Updates existing metadata files from their configured endpoints
  - Builds project to discover `[ODataClient]` attributes
  - Re-downloads metadata from each endpoint
  - Updates metadata XML files

**Key Technologies**:
- System.CommandLine for CLI parsing
- Buildalyzer for MSBuild project analysis
- Microsoft.Build for project file manipulation

## Repository Organization

```
Forge.OData.Client/
├── src/                          # Source projects
│   ├── Forge.OData.Generator/    # Roslyn source generator
│   ├── Forge.OData.Client/       # Runtime client library
│   ├── Forge.OData.Attributes/   # Attribute library
│   └── Forge.OData.CLI/          # Command-line tool
├── tests/                        # Test projects
│   ├── Forge.OData.Client.Tests/ # Unit tests for client library
│   └── Forge.OData.CLI.Tests/    # Unit tests for CLI tool
├── sample/                       # Sample projects
│   ├── Forge.OData.Sample/       # Basic metadata file approach
│   ├── Forge.OData.AttributeSample/ # Attribute-based approach
│   └── Forge.OData.Service/      # Test OData service
├── scripts/                      # Build and release scripts
│   ├── update-version.sh         # Updates version in project files
│   └── get-changelog-entry.sh    # Extracts changelog for releases
├── .github/                      # GitHub workflows
│   └── workflows/
│       └── release.yml           # Automated release workflow
├── README.md                     # User documentation
├── CHANGELOG.md                  # Version history
├── CONTRIBUTE.md                 # This file
└── Forge.OData.Client.slnx       # Solution file
```

## Development Setup

### Prerequisites

- **.NET 9 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Git**: For version control
- **IDE**: Visual Studio 2022, Visual Studio Code, or Rider

### Getting Started

1. **Clone the repository**:
   ```bash
   git clone https://github.com/brustolin/Forge.OData.Client.git
   cd Forge.OData.Client
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the solution**:
   ```bash
   dotnet build
   ```

4. **Run tests**:
   ```bash
   dotnet test
   ```

### IDE Setup

**Visual Studio 2022**:
- Open `Forge.OData.Client.slnx`
- Set solution configuration to Debug
- Use Test Explorer for running tests

**Visual Studio Code**:
- Install C# Dev Kit extension
- Open the repository folder
- Use built-in test explorer

## Building and Testing

### Building

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/Forge.OData.Generator/Forge.OData.Generator.csproj

# Build in Release mode
dotnet build --configuration Release
```

### Testing

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests for specific project
dotnet test tests/Forge.OData.Client.Tests/Forge.OData.Client.Tests.csproj

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**Test Framework**: xUnit

**Test Projects**:
- `Forge.OData.Client.Tests`: Tests for runtime library (ODataQueryable, ODataExpressionVisitor, ODataQueryBuilder)
- `Forge.OData.CLI.Tests`: Tests for CLI commands

### Running Sample Projects

**Basic Sample (Metadata File Approach)**:
```bash
dotnet run --project sample/Forge.OData.Sample/Forge.OData.Sample.csproj
```

**Attribute Sample (Attribute-Based Approach)**:
```bash
dotnet run --project sample/Forge.OData.AttributeSample/Forge.OData.AttributeSample.csproj
```

**Test OData Service**:
```bash
dotnet run --project sample/Forge.OData.Service/Forge.OData.Service.csproj
# Service runs on http://localhost:5000
# Metadata: http://localhost:5000/odata/$metadata
```

## Code Generation Workflow

Understanding the code generation workflow is crucial for contributing to the generator:

### Generation Triggers

1. **File-Based Generation** (ClientGenerator):
   - Triggered by: `.xml` files in `<AdditionalFiles>` without corresponding `[ODataClient]` attribute
   - Generates: Standalone client class named `{FileName}Client`
   - Example: `MyService.xml` → `MyServiceClient.cs`

2. **Attribute-Based Generation** (PartialClientGenerator):
   - Triggered by: Classes with `[ODataClient]` attribute
   - Generates: Partial client class matching the existing class name
   - Example: `[ODataClient(MetadataFile = "MyService.xml")] partial class MyClient` → generates partial class members

### Generated Code Structure

For each OData metadata file, the generator creates:

1. **Model Classes** (`{EntityName}.g.cs`):
   ```csharp
   [JsonConverter(typeof(ProductConverter))]
   public class Product
   {
       [Key]
       public int Id { get; set; }
       public string Name { get; set; }
       // ... other properties
   }
   ```

2. **JSON Converters** (`{EntityName}Converter.g.cs`):
   ```csharp
   public class ProductConverter : JsonConverter<Product>
   {
       public override Product Read(ref Utf8JsonReader reader, ...)
       {
           // Custom deserialization without reflection
       }
       public override void Write(Utf8JsonWriter writer, ...)
       {
           // Custom serialization
       }
   }
   ```

3. **Client Class** (`{ClientName}.g.cs` or partial class):
   ```csharp
   public partial class MyServiceClient
   {
       private readonly HttpClient _httpClient;
       private readonly string _baseUrl;
       
       public ODataQueryable<Product> Products => 
           new ODataQueryable<Product>(_httpClient, _baseUrl, "Products");
       // ... other entity sets
   }
   ```

### Debugging the Generator

Source generators run during compilation, which requires special debugging:

1. **Enable generator logging**:
   - Add to project file:
     ```xml
     <PropertyGroup>
       <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
       <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>
     </PropertyGroup>
     ```
   - Generated files appear in `obj/Debug/net9.0/GeneratedFiles/`

2. **Attach debugger**:
   - In Visual Studio: Debug → Attach to Process → select `VBCSCompiler.exe` or `dotnet.exe`
   - Add breakpoints in generator code
   - Trigger compilation in the target project

3. **Unit test the generator components**:
   - Test `ODataMetadataParser` independently
   - Test `ModelGenerator` with sample schemas
   - Verify SyntaxFactory output

## Contributing Guidelines

### Code Style

- **Naming Conventions**: Follow .NET naming conventions
  - PascalCase for public members
  - camelCase for private fields (prefix with `_`)
  - Use meaningful, descriptive names
- **Formatting**: Use default .NET formatting
- **Comments**: Add XML documentation for public APIs
- **Nullability**: Enable nullable reference types, handle nulls appropriately

### Making Changes

1. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**:
   - Keep changes focused and minimal
   - Write tests for new functionality
   - Update documentation if needed

3. **Test your changes**:
   ```bash
   dotnet build
   dotnet test
   ```

4. **Commit your changes**:
   ```bash
   git add .
   git commit -m "feat: add your feature description"
   ```
   
   Use conventional commit messages:
   - `feat:` for new features
   - `fix:` for bug fixes
   - `docs:` for documentation changes
   - `test:` for test additions/changes
   - `refactor:` for code refactoring
   - `chore:` for maintenance tasks

5. **Push and create a pull request**:
   ```bash
   git push origin feature/your-feature-name
   ```

### Pull Request Guidelines

- **Title**: Use conventional commit format
- **Description**: Clearly explain what and why
- **Tests**: Include tests for new functionality
- **Documentation**: Update README.md or XML docs as needed
- **Breaking Changes**: Clearly document any breaking changes

### Areas for Contribution

**High Priority**:
- Additional OData query operators support
- Performance optimizations
- Better error handling and messages
- Documentation improvements

**Generator Improvements**:
- Support for more OData types
- Function imports support
- Action imports support
- Better handling of nullable types

**CLI Enhancements**:
- More configuration options
- Better error messages
- Progress indicators for long operations

**Testing**:
- Increase test coverage
- Integration tests
- Performance benchmarks

## Release Process

The project uses an automated release workflow:

### Creating a Release

1. **Update CHANGELOG.md**:
   - Document changes under `[Unreleased]`
   - Follow existing format

2. **Trigger Release Workflow**:
   - Go to GitHub Actions
   - Run "Create Release" workflow
   - Provide version number (e.g., `1.0.0` or `1.0.0-beta.1`)

3. **Automated Steps**:
   - Creates release branch
   - Updates version in project files via `scripts/update-version.sh`
   - Commits version changes
   - Creates and pushes git tag
   - Extracts changelog via `scripts/get-changelog-entry.sh`
   - Creates GitHub release
   - Builds and packs NuGet packages
   - Uploads packages as artifacts
   - Merges release branch to main
   - Deletes temporary release branch

### Version Numbering

Follow semantic versioning (semver):
- **Major.Minor.Patch** (e.g., `1.0.0`)
- **Major.Minor.Patch-Prerelease** (e.g., `1.0.0-beta.1`)

**When to increment**:
- **Major**: Breaking changes
- **Minor**: New features (backwards compatible)
- **Patch**: Bug fixes (backwards compatible)

## Project Structure Details

### Source Generator Implementation

**Incremental Generator Pattern**:
- Uses `IIncrementalGenerator` for better performance
- Caches parsed metadata
- Only regenerates when inputs change
- Filters files efficiently before parsing

**SyntaxFactory Usage**:
All code generation uses Roslyn's SyntaxFactory API:
```csharp
var classDeclaration = ClassDeclaration(className)
    .AddModifiers(Token(SyntaxKind.PublicKeyword))
    .AddMembers(properties.ToArray());
```

Benefits:
- Type-safe code generation
- Automatic formatting
- Easier to maintain than string concatenation

### OData Type Mapping

| OData Type | C# Type |
|------------|---------|
| Edm.String | string |
| Edm.Int32 | int |
| Edm.Int64 | long |
| Edm.Boolean | bool |
| Edm.Decimal | decimal |
| Edm.Double | double |
| Edm.DateTime | DateTime |
| Edm.DateTimeOffset | DateTimeOffset |
| Edm.Guid | Guid |
| Collection(T) | List&lt;T&gt; |

### LINQ to OData Translation

The `ODataExpressionVisitor` translates LINQ expressions:

| LINQ | OData |
|------|-------|
| `x => x.Price > 10` | `Price gt 10` |
| `x => x.Name == "Test"` | `Name eq 'Test'` |
| `x => x.InStock && x.Price < 100` | `InStock eq true and Price lt 100` |
| `x => x.Name.StartsWith("A")` | `startswith(Name, 'A')` |

## Questions or Issues?

- **Bug Reports**: Open an issue on GitHub
- **Feature Requests**: Open an issue with detailed description
- **Questions**: Use GitHub Discussions

## License

This project is licensed under the MIT License. See the repository for details.

---

Thank you for contributing to Forge.OData.Client! 🚀
