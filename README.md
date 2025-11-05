# Forge.OData.Client

A .NET 9 OData client with source generator that automatically generates model classes and client code from OData $metadata XML files.

## Features

- **Source Generator**: Automatically generates C# model classes from OData metadata
- **Type-Safe Client**: Generated client class with strongly-typed entity sets
- **LINQ to OData**: Translate LINQ expressions to OData query syntax
- **Uses SyntaxFactory**: Clean code generation using Roslyn's SyntaxFactory API
- **Modern .NET**: Built for .NET 9 with latest C# features
- **Attribute-Based Generation**: Use `[ODataClient]` attribute for custom client classes
- **Performance Optimized**: Custom JSON converters avoid reflection during deserialization

## Projects

### Forge.OData.Generator

A source generator project that reads OData $metadata XML files and generates:
- Model classes for EntityTypes and ComplexTypes with `[JsonConverter]` attributes
- Custom `JsonConverter<T>` for each model for efficient deserialization
- Client class with entity set properties (standalone or partial)
- All code generation uses Roslyn's SyntaxFactory for clean, maintainable code

### Forge.OData.Client.Core

Utility library that provides:
- `ODataQueryable<T>`: Fluent API for building OData queries
- `ODataExpressionVisitor`: Converts LINQ expressions to OData filter syntax
- `ODataQueryBuilder`: Builds OData query strings

### Forge.OData.Attributes

Attribute library for declarative client generation:
- `ODataClientAttribute`: Mark partial classes for OData client generation
- Supports custom class names, namespaces, and default endpoints

### Forge.OData.Service

Test service project that provides:
- ASP.NET Core Web API with OData v4 endpoints
- In-memory sample data for Products, Orders, and Customers
- Full OData query capabilities for testing and validation
- Can be used to test the generated client code against a real OData service

### Forge.OData.CLI

Command-line tool for managing OData metadata in projects:
- **dotnet tool**: Install globally or locally as a .NET tool
- **add command**: Download metadata from an OData endpoint and set up a new client
- **update command**: Update existing metadata files from their configured endpoints
- Automates the process of adding and maintaining OData clients in your projects

## Using the CLI Tool

The Forge.OData.CLI tool provides a convenient way to manage OData metadata in your projects, similar to how `dotnet ef` manages Entity Framework migrations.

### Installation

Install the tool globally:

```bash
dotnet tool install --global Forge.OData.CLI
```

Or install it locally in your project:

```bash
dotnet new tool-manifest  # if you don't have one already
dotnet tool install --local Forge.OData.CLI
```

### Adding OData Metadata to Your Project

Use the `add` command to download metadata from an OData endpoint and configure your project:

```bash
dotnet odata add --endpoint https://services.odata.org/V4/TripPinServiceRW --client-name TripPinService
```

This command will:
1. Download the metadata XML from the endpoint
2. Save it as `TripPinServiceMetadata.xml` in your project
3. Create a `TripPinService.cs` file with the `[ODataClient]` attribute
4. Add both files to your project's `.csproj` file
5. Configure the project to use the OData generator

Options:
- `--endpoint` or `-e` (required): The OData service endpoint URL
- `--project` or `-p` (optional): Path to the project file (defaults to current directory)
- `--client-name` or `-n` (optional): Name for the generated client class (auto-generated from endpoint if not specified)

### Updating Existing Metadata

Use the `update` command to refresh metadata from the server for all OData clients in your project:

```bash
dotnet odata update
```

This command will:
1. Build your project
2. Find all classes with the `[ODataClient]` attribute
3. Download updated metadata from each configured endpoint
4. Update the corresponding metadata XML files

Options:
- `--project` or `-p` (optional): Path to the project file (defaults to current directory)

### Example Workflow

```bash
# Navigate to your project directory
cd MyProject

# Add OData metadata from a service
dotnet odata add --endpoint https://api.example.com/odata --client-name ExampleService

# Build your project to generate the client code
dotnet build

# Later, update the metadata when the service schema changes
dotnet odata update

# Rebuild to regenerate the client with the new metadata
dotnet build
```

## Getting Started

You can use the generator in two ways:

### Option 1: Metadata File Approach (Simple)

This approach generates a client class named based on your XML filename (e.g., `MyService.xml` generates `MyServiceClient`).

#### 1. Add the Generator to Your Project

```xml
<ItemGroup>
  <ProjectReference Include="path/to/Forge.OData.Generator/Forge.OData.Generator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
  <ProjectReference Include="path/to/Forge.OData.Client.Core/Forge.OData.Client.Core.csproj" />
</ItemGroup>
```

#### 2. Add Your OData Metadata File

Add your OData $metadata XML file to your project as an additional file:

```xml
<ItemGroup>
  <AdditionalFiles Include="YourMetadata.xml" />
</ItemGroup>
```

#### 3. Use the Generated Client

The client class name is based on the XML filename. For example, `YourMetadata.xml` generates `YourMetadataClient`:

```csharp
using System.Net.Http;
using YourNamespace;

var httpClient = new HttpClient();
var client = new YourMetadataClient(httpClient, "https://your-odata-service.com");

// Query with LINQ
var products = await client.Products
    .Where(p => p.Price > 10 && p.InStock)
    .OrderBy(p => p.Name)
    .Take(10)
    .ToListAsync();
```

**Note**: The generator sanitizes the filename to create a valid C# class name:
- Removes file extension (`.xml`)
- Removes dots and special characters (e.g., `Resources.OData.xml` becomes `ResourcesODataClient`)
- Adds underscore prefix if name starts with a digit (e.g., `123Service.xml` becomes `_123ServiceClient`)
- Appends `Client` suffix to the name

### Option 2: Attribute-Based Approach (Recommended)

This approach generates a partial class, allowing you to customize the client name, namespace, and add your own members.

#### 1. Add Required References

```xml
<ItemGroup>
  <ProjectReference Include="path/to/Forge.OData.Attributes/Forge.OData.Attributes.csproj" />
  <ProjectReference Include="path/to/Forge.OData.Generator/Forge.OData.Generator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
  <ProjectReference Include="path/to/Forge.OData.Client.Core/Forge.OData.Client.Core.csproj" />
</ItemGroup>

<ItemGroup>
  <AdditionalFiles Include="YourMetadata.xml" />
</ItemGroup>
```

#### 2. Create a Partial Class with the Attribute

```csharp
using Forge.OData.Attributes;

namespace MyApp.Services
{
    [ODataClient(MetadataFile = "YourMetadata.xml", Endpoint = "https://your-odata-service.com")]
    public partial class MyCustomClient
    {
        // Add your custom properties and methods here
        public string ServiceName => "My OData Service";
    }
}
```

#### 3. Use Your Custom Client

```csharp
using var httpClient = new HttpClient();

// Endpoint is configured via the attribute
var client = new MyCustomClient(httpClient);

// Use the generated entity sets
var products = await client.Products
    .Where(p => p.Price > 10)
    .OrderBy(p => p.Name)
    .ToListAsync();

// Access your custom members
Console.WriteLine(client.ServiceName);
```

## Code Generation Details

### Generated Models

All generated model classes include:
- Properties mapped from OData metadata
- `[Key]` attributes for entity keys
- `[JsonConverter]` attributes for custom serialization
- Navigation properties for related entities

Example:
```csharp
[JsonConverter(typeof(ProductConverter))]
public class Product
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    // ... other properties
}
```

### Generated JSON Converters

Each model gets a custom `JsonConverter<T>` that:
- Reads JSON directly without reflection
- Provides better performance than default serialization
- Handles nullable types correctly

Example converter excerpt:
```csharp
public class ProductConverter : JsonConverter<Product>
{
    public override Product Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Direct property reading without reflection
        var result = new Product();
        while (reader.Read())
        {
            // ... switch on property names and read values directly
        }
        return result;
    }
    // ... Write method
}
```

## Usage Examples

### Basic Query

```csharp
var products = await client.Products
    .ToListAsync();

// Expand navigation properties
var ordersWithProducts = await client.Orders
    .Expand(o => o.Product)
    .Where(o => o.TotalAmount > 100)
    .ToListAsync();

// Select specific properties
var productNames = await client.Products
    .Select(p => new { p.Name, p.Price })
    .ToListAsync();
```

## LINQ to OData Translation

The library supports translating LINQ expressions to OData query syntax:

| LINQ | OData |
|------|-------|
| `Where(x => x.Price > 10)` | `$filter=Price gt 10` |
| `OrderBy(x => x.Name)` | `$orderby=Name asc` |
| `OrderByDescending(x => x.Price)` | `$orderby=Price desc` |
| `Skip(10)` | `$skip=10` |
| `Take(20)` | `$top=20` |
| `Select(x => new { x.Name })` | `$select=Name` |
| `Expand(x => x.Orders)` | `$expand=Orders` |

## Supported OData Types

The generator maps OData types to C# types:

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

## Example Metadata

See `tests/Forge.OData.Sample/SampleMetadata.xml` for an example OData metadata file.

## Testing with Forge.OData.Service

The solution includes **Forge.OData.Service**, a simple ASP.NET Core Web API project with OData endpoints and sample data for testing and validating the library.

### Running the OData Service

```bash
dotnet run --project tests/Forge.OData.Service/Forge.OData.Service.csproj
```

The service will start on `http://localhost:5000` and provides:
- OData metadata endpoint: `http://localhost:5000/odata/$metadata`
- Sample data for Products, Orders, and Customers
- Full OData query support ($filter, $select, $expand, $orderby, etc.)

See [Forge.OData.Service README](tests/Forge.OData.Service/README.md) for more details and example queries.

## Building the Solution

```bash
dotnet build Forge.OData.Client.sln
```

## Running the Sample

```bash
dotnet run --project tests/Forge.OData.Sample/Forge.OData.Sample.csproj
```

## License

MIT

