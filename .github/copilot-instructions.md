# Copilot Instructions for Forge.OData.Client

This repository contains Forge.OData.Client, a .NET 9 OData client library with a Roslyn source generator that automatically generates type-safe model classes and client code from OData $metadata XML files.

## Project Overview

Forge.OData.Client enables developers to connect to OData services by generating everything needed—models, clients, and type-safe queries—from the service metadata. The project consists of four main components:

1. **Forge.OData.Generator**: Roslyn incremental source generator that generates C# code during compilation
2. **Forge.OData.Client**: Runtime library providing ODataQueryable<T> and LINQ-to-OData query translation
3. **Forge.OData.Attributes**: Attribute library for declarative client generation
4. **Forge.OData.CLI**: dotnet tool for managing OData metadata in projects

## Architecture & Key Concepts

### Source Generator Workflow

The generator runs during compilation and:
- Detects `.xml` files marked as `AdditionalFiles` in project files
- Parses OData $metadata XML to extract EntityTypes, ComplexTypes, and EntitySets
- Generates C# code using SyntaxFactory API (not string concatenation)
- Creates model classes with `[Key]` attributes and custom JSON converters
- Generates client classes with entity set properties returning `ODataQueryable<T>`

### Code Generation Approaches

1. **File-Based**: `.xml` metadata files without attributes → generates standalone `{FileName}Client` class
2. **Attribute-Based**: Classes with `[ODataClient]` attribute → generates partial class members

### Key Technologies

- **.NET 9**: Target framework
- **Roslyn**: IIncrementalGenerator for source generation
- **SyntaxFactory**: Type-safe code generation API
- **System.CommandLine**: CLI parsing
- **Buildalyzer**: MSBuild project analysis
- **xUnit**: Testing framework

## Repository Structure

```
Forge.OData.Client/
├── src/
│   ├── Forge.OData.Generator/    # Roslyn source generator
│   ├── Forge.OData.Client/       # Runtime client library
│   ├── Forge.OData.Attributes/   # Attribute library
│   └── Forge.OData.CLI/          # Command-line tool
├── tests/
│   ├── Forge.OData.Client.Tests/ # Runtime library tests
│   └── Forge.OData.CLI.Tests/    # CLI tool tests
├── sample/                       # Sample projects demonstrating usage
├── scripts/                      # Build and release automation
└── .github/workflows/            # CI/CD workflows
```

## Building and Testing

### Build Commands

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/Forge.OData.Generator/Forge.OData.Generator.csproj

# Build in Release mode
dotnet build --configuration Release
```

### Test Commands

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test project
dotnet test tests/Forge.OData.Client.Tests/Forge.OData.Client.Tests.csproj
```

### Running Samples

```bash
# Basic metadata file approach
dotnet run --project sample/Forge.OData.Sample/Forge.OData.Sample.csproj

# Attribute-based approach
dotnet run --project sample/Forge.OData.AttributeSample/Forge.OData.AttributeSample.csproj

# Test OData service (runs on http://localhost:5000)
dotnet run --project sample/Forge.OData.Service/Forge.OData.Service.csproj
```

## Coding Standards and Conventions

### Naming Conventions

- **PascalCase**: Public members, types, namespaces
- **camelCase**: Private fields (prefix with underscore: `_fieldName`)
- **Use meaningful names**: Prefer `clientGenerator` over `cg` or `gen`

### Code Style

- Follow .NET naming conventions
- Use default .NET formatting (no custom formatters required)
- Enable nullable reference types and handle nulls appropriately
- Add XML documentation comments (`///`) for all public APIs
- Prefer SyntaxFactory API over string concatenation for code generation

### Key Patterns

- **Incremental Generator Pattern**: Use `IIncrementalGenerator` for better performance and caching
- **Immutability**: ODataQueryable returns new instances for method chaining
- **Fluent APIs**: Builder patterns for query construction
- **Expression Visitors**: For LINQ-to-OData translation

## Important Files and Their Purposes

### Generator Files (src/Forge.OData.Generator/)

- `ODataSourceGenerator.cs`: Main incremental generator entry point
- `ODataMetadataParser.cs`: Parses OData $metadata XML files
- `ModelGenerator.cs`: Generates model classes for EntityTypes and ComplexTypes
- `JsonConverterGenerator.cs`: Generates custom JSON converters (no reflection)
- `ClientGenerator.cs`: Generates standalone client classes
- `PartialClientGenerator.cs`: Generates partial client classes for attribute approach

### Runtime Files (src/Forge.OData.Client/)

- `ODataQueryable.cs`: Fluent API for building and executing OData queries
- `ODataExpressionVisitor.cs`: Converts LINQ expressions to OData filter syntax
- `ODataQueryBuilder.cs`: Builds OData query strings ($filter, $select, etc.)

### CLI Files (src/Forge.OData.CLI/)

- `AddCommand.cs`: Downloads metadata and configures projects
- `UpdateCommand.cs`: Updates existing metadata from endpoints

## Testing Guidelines

- Use xUnit for all tests
- Test files should be in corresponding test projects
- Name tests descriptively: `MethodName_Scenario_ExpectedResult`
- Use `[Theory]` and `[InlineData]` for parameterized tests
- Mock HTTP clients and external dependencies
- Test generator components independently from compilation

## Common Tasks and How to Approach Them

### Adding New OData Type Support

1. Update type mapping in `ODataMetadataParser.cs`
2. Add corresponding C# type in model generation
3. Update JSON converter generation
4. Add tests for the new type

### Adding New LINQ Operators

1. Update `ODataExpressionVisitor.cs` to handle the operator
2. Add translation logic to OData syntax
3. Update `ODataQueryable.cs` if needed
4. Add tests for the new operator

### Debugging Source Generators

1. Add to project file:
   ```xml
   <PropertyGroup>
     <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
     <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)\GeneratedFiles</CompilerGeneratedFilesOutputPath>
   </PropertyGroup>
   ```
2. Generated files appear in `obj/Debug/net9.0/GeneratedFiles/`
3. Attach debugger to `VBCSCompiler.exe` or `dotnet.exe` process

## OData Type Mappings

| OData Type          | C# Type          |
|---------------------|------------------|
| Edm.String          | string           |
| Edm.Int32           | int              |
| Edm.Int64           | long             |
| Edm.Boolean         | bool             |
| Edm.Decimal         | decimal          |
| Edm.Double          | double           |
| Edm.DateTime        | DateTime         |
| Edm.DateTimeOffset  | DateTimeOffset   |
| Edm.Guid            | Guid             |
| Collection(T)       | List\<T\>        |

## LINQ to OData Translation Reference

| LINQ Expression                              | OData Query                        |
|---------------------------------------------|------------------------------------|
| `x => x.Price > 10`                         | `Price gt 10`                      |
| `x => x.Name == "Test"`                     | `Name eq 'Test'`                   |
| `x => x.InStock && x.Price < 100`           | `InStock eq true and Price lt 100` |
| `x => x.Name.StartsWith("A")`               | `startswith(Name, 'A')`            |
| `x => x.Name.EndsWith("Z")`                 | `endswith(Name, 'Z')`              |
| `x => x.Name.Contains("mid")`               | `contains(Name, 'mid')`            |

## Commit Message Conventions

Use conventional commits format:

- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `test:` - Test additions/changes
- `refactor:` - Code refactoring
- `chore:` - Maintenance tasks
- `perf:` - Performance improvements

## Things to Avoid

- ❌ **Don't use string concatenation for code generation** - Use SyntaxFactory API instead
- ❌ **Don't use reflection in generated code** - Performance is critical
- ❌ **Don't modify generated files** - They are auto-generated and will be overwritten
- ❌ **Don't break existing APIs** - This is a library used by others
- ❌ **Don't skip tests** - Maintain high test coverage
- ❌ **Don't ignore nullable warnings** - The project uses nullable reference types

## Documentation References

- **User Documentation**: See [README.md](../README.md)
- **Contribution Guide**: See [CONTRIBUTE.md](../CONTRIBUTE.md)
- **Changelog**: See [CHANGELOG.md](../CHANGELOG.md)
- **Sample Projects**: See `sample/` directory

## When Making Changes

1. ✅ Run `dotnet build` to ensure code compiles
2. ✅ Run `dotnet test` to ensure all tests pass
3. ✅ Add tests for new functionality
4. ✅ Update documentation if changing public APIs
5. ✅ Use meaningful commit messages
6. ✅ Keep changes focused and minimal
7. ✅ Follow existing code patterns and conventions
8. ✅ If code was changed, write a short description  in the CHANGELOG under the [Unreleased] section.

## Release Process

Releases are automated via GitHub Actions:
- Update CHANGELOG.md with changes under `[Unreleased]`
- Trigger "Create Release" workflow with version number
- Workflow handles versioning, tagging, building, and publishing

## Questions or Issues

- For architecture questions, refer to [CONTRIBUTE.md](../CONTRIBUTE.md)
- For usage questions, refer to [README.md](../README.md)
- For code examples, check the `sample/` directory
- For test examples, check the `tests/` directory
