# Forge.OData.AttributeSample

This sample demonstrates the **attribute-based** approach to generating OData clients using the `ODataClientAttribute`.

## How It Works

Instead of letting the generator create a standalone `ODataClient` class, you can create your own partial class and annotate it with `[ODataClient]`. The generator will then create a matching partial class with all the client functionality.

### Benefits

1. **Custom Class Names**: Use your own class name instead of the default `ODataClient`
2. **Custom Namespaces**: Place the client in any namespace you want
3. **Extensibility**: Add your own properties and methods to the client class
4. **Default Endpoint**: Configure the endpoint URL directly in the attribute

## Usage

### 1. Create a Partial Class

```csharp
using Forge.OData.Attributes;

namespace MyApp.Services
{
    [ODataClient(MetadataFile = "SampleMetadata.xml", Endpoint = "https://services.odata.org/V4/OData/OData.svc")]
    public partial class SampleServiceClient
    {
        // You can add custom methods and properties here
        public string ServiceName => "Sample OData Service";
    }
}
```

### 2. Configure Your Project

Add the necessary references and metadata file to your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="../../src/Forge.OData.Attributes/Forge.OData.Attributes.csproj" />
  <ProjectReference Include="../../src/Forge.OData.Client.Core/Forge.OData.Client.Core.csproj" />
  <ProjectReference Include="../../src/Forge.OData.Generator/Forge.OData.Generator.csproj" 
                    OutputItemType="Analyzer" 
                    ReferenceOutputAssembly="false" />
</ItemGroup>

<ItemGroup>
  <AdditionalFiles Include="SampleMetadata.xml" />
</ItemGroup>
```

### 3. Use the Client

```csharp
using var httpClient = new HttpClient();

// The endpoint is already configured via the attribute
var client = new SampleServiceClient(httpClient);

// Use the generated entity sets
var products = await client.Products
    .Where(p => p.Price > 10)
    .OrderBy(p => p.Name)
    .ToListAsync();
```

## Generated Code

The generator creates:

1. **Partial Client Class**: Adds constructor, private fields, and entity set properties to your class
2. **Model Classes**: Entity and complex types from your metadata
3. **JSON Converters**: Custom `JsonConverter<T>` for each model to avoid reflection during deserialization

### Example Generated Partial Class

```csharp
public partial class SampleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    
    public SampleServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _baseUrl = "https://services.odata.org/V4/OData/OData.svc";
    }

    public ODataQueryable<Product> Products => new ODataQueryable<Product>(_httpClient, _baseUrl, "Products");
    public ODataQueryable<Order> Orders => new ODataQueryable<Order>(_httpClient, _baseUrl, "Orders");
    public ODataQueryable<Customer> Customers => new ODataQueryable<Customer>(_httpClient, _baseUrl, "Customers");
}
```

### Example Generated Model with JsonConverter

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

## Performance Benefits

The generated `JsonConverter` classes provide:

- **No Reflection**: Direct property access during deserialization
- **Type Safety**: Compile-time type checking
- **Better Performance**: Faster than reflection-based deserializers

## Comparison with Metadata File Approach

| Feature | Metadata File | Attribute-Based |
|---------|--------------|-----------------|
| Class Name | Fixed (`ODataClient`) | Customizable |
| Namespace | From metadata | Your choice |
| Endpoint | Constructor parameter | Attribute property (optional) |
| Extensibility | Limited | Full (partial class) |
| Generated Output | Standalone class | Partial class |

Both approaches generate:
- ✅ Model classes
- ✅ JSON converters
- ✅ Entity set properties
