# Changelog

## [Unreleased]

## [0.0.2] - 2025-11-05

### Forge.OData.Client
A .NET 9 OData client utility library providing:
- **ODataQueryable<T>**: Fluent API for building OData queries with LINQ support
- **ODataExpressionVisitor**: Converts LINQ expressions to OData filter syntax
- **ODataQueryBuilder**: Builds OData query strings with support for $filter, $select, $expand, $orderby, $skip, and $top

### Forge.OData.Generator
A source generator that reads OData $metadata XML files and generates:
- **Model Classes**: Automatically generated C# classes for EntityTypes and ComplexTypes with proper attributes
- **JSON Converters**: Custom `JsonConverter<T>` for each model to avoid reflection during deserialization
- **Client Classes**: Type-safe client classes with entity set properties (standalone or partial)
- **SyntaxFactory Usage**: All code generation uses Roslyn's SyntaxFactory API for clean, maintainable code
- Supports both file-based generation (generates `{FileName}Client`) and attribute-based generation (partial classes)

### Forge.OData.CLI
Command-line tool for managing OData metadata in .NET projects:
- **dotnet tool**: Can be installed globally or locally as a .NET tool
- **add command**: Downloads OData metadata from an endpoint and configures project
  - Downloads $metadata XML from specified OData endpoint
  - Creates client class file with `[ODataClient]` attribute
  - Adds files to project's .csproj
  - Configures project to use the OData generator
  - Supports custom client names, namespaces, and output paths
- **update command**: Updates existing metadata files from their configured endpoints
  - Builds the project to discover all `[ODataClient]` attributes
  - Downloads updated metadata from each configured endpoint
  - Updates the corresponding metadata XML files

### Forge.OData.Attributes
Attribute library for declarative client generation:
- **ODataClientAttribute**: Marks partial classes for OData client generation
  - Supports custom class names
  - Supports custom namespaces
  - Supports default endpoint configuration
  - Supports custom metadata file paths

## [0.0.1] - Initial Development

### Added
- Initial project structure
- Core OData client functionality
- Source generator implementation
- CLI tool for metadata management
- Attribute-based client generation
- Type-safe LINQ to OData translation
- Custom JSON converters for performance
- Sample projects and test service
- Comprehensive test suite
