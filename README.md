# Forge.OData.Client

**Your project needs to connect to an OData service?** Just point our CLI at the endpoint, and everything gets generated for you—models, client, type-safe queries. **No manual work. No boilerplate. Just instant OData integration.**

## ⚡ Quick Start

### 1. Install the CLI Tool

```bash
dotnet tool install --global Forge.OData.CLI
```

### 2. Add an OData Client to Your Project

Navigate to your project and run:

```bash
dotnet odata add --endpoint https://services.odata.org/V4/TripPinServiceRW
```

### 3. Build and Code

```bash
dotnet build
```

That's it! You now have a fully functional, type-safe OData client ready to use:

```csharp
using var httpClient = new HttpClient();
var client = new TripPinServiceRWClient(httpClient);

// Query with LINQ - it just works!
var people = await client.People
    .Where(p => p.FirstName.StartsWith("R"))
    .OrderBy(p => p.LastName)
    .Take(10)
    .ToListAsync();

foreach (var person in people)
{
    Console.WriteLine($"{person.FirstName} {person.LastName}");
}
```

## 🎯 Why Forge.OData.Client?

**Stop writing boilerplate.** Stop manually creating DTOs. Stop fighting with HTTP requests and JSON parsing.

You have an OData API. You want to use it. The OData CLI does the heavy lifting:

- ✅ **Downloads the metadata** from your OData service
- ✅ **Generates all model classes** with proper types and attributes  
- ✅ **Creates a type-safe client** with IntelliSense support
- ✅ **Translates LINQ to OData queries** automatically
- ✅ **Handles JSON serialization** with optimized converters

All this happens during your normal `dotnet build`. No runtime reflection. No performance overhead.

## 📦 Installation

Install the Forge OData CLI as a global tool:

```bash
dotnet tool install --global Forge.OData.CLI
```

Or install it locally in your project for team consistency:

```bash
dotnet new tool-manifest  # if you don't have one already
dotnet tool install --local Forge.OData.CLI
```

## 🚀 Usage Examples

### Basic Usage: Connect to Any OData Service

```bash
# Simple: Just provide the endpoint
dotnet odata add --endpoint https://services.odata.org/V4/Northwind/Northwind.svc
```

The CLI will:
1. Download the `$metadata` from the endpoint
2. Generate a client class named `NorthwindClient` (derived from URL)
3. Create all model classes for entities (Products, Orders, Customers, etc.)
4. Configure your project automatically

### Custom Client Name

```bash
# Give your client a meaningful name
dotnet odata add \
  --endpoint https://api.example.com/odata \
  --client-name CompanyDataService
```

Now you'll have a `CompanyDataService` class instead of a generic name.

### Organize Clients in Subdirectories

```bash
# Keep your OData clients organized
dotnet odata add \
  --endpoint https://api.example.com/odata \
  --client-name InventoryService \
  --output-path Services/OData
```

This creates:
- File: `Services/OData/InventoryService.cs`
- Namespace: `YourProject.Services.OData`

### Custom Namespace

```bash
# Control the namespace for better organization
dotnet odata add \
  --endpoint https://api.example.com/odata \
  --client-name ProductCatalog \
  --namespace MyCompany.External.Services
```

### Multiple OData Services in One Project

```bash
# Add multiple services - they all work together
dotnet odata add --endpoint https://api.products.com/odata --client-name ProductService
dotnet odata add --endpoint https://api.orders.com/odata --client-name OrderService
dotnet odata add --endpoint https://api.customers.com/odata --client-name CustomerService
```

Each client is independent, and you can use them side by side in your application.

## 🔄 Keeping Metadata Up to Date

When the OData service changes (new entities, modified properties), just update:

```bash
dotnet odata update
```

This command:
- Finds all OData clients in your project
- Re-downloads metadata from their endpoints
- Updates the metadata files
- Rebuild to regenerate clients with the latest schema

**Workflow example:**

```bash
# Initial setup
dotnet odata add --endpoint https://api.example.com/odata --client-name ApiClient
dotnet build

# ... time passes, API changes ...

# Update to latest schema
dotnet odata update
dotnet build  # Regenerates with new metadata
```

## 💡 Real-World Example

Let's say you're building an app that needs to fetch data from the TripPin OData service:

```bash
# Step 1: Add the client
cd MyTravelApp
dotnet odata add \
  --endpoint https://services.odata.org/V4/TripPinServiceRW \
  --client-name TripPinService \
  --output-path Services

# Step 2: Build
dotnet build
```

Now use it in your code:

```csharp
using MyTravelApp.Services;

public class TravelService
{
    private readonly HttpClient _httpClient;
    
    public TravelService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    
    public async Task<List<Person>> GetTravelersAsync(string countryName)
    {
        var client = new TripPinService(_httpClient);
        
        // Type-safe LINQ queries
        return await client.People
            .Where(p => p.FavoriteFeature == "Feature1")
            .OrderBy(p => p.LastName)
            .ToListAsync();
    }
    
    public async Task<Person> GetPersonWithTripsAsync(string username)
    {
        var client = new TripPinService(_httpClient);
        
        // Expand navigation properties
        var people = await client.People
            .Where(p => p.UserName == username)
            .Expand(p => p.Trips)
            .ToListAsync();
            
        return people.FirstOrDefault();
    }
}
```

**That's it.** No manual DTOs. No string-based queries. Just clean, type-safe code with full IntelliSense.

## 🔍 What You Get

When you run `dotnet odata add`, the tool generates:

### 1. Model Classes

```csharp
public class Product
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool InStock { get; set; }
    // ... all properties from metadata
}
```

### 2. Optimized JSON Converters

Custom converters for each model that:
- Deserialize JSON without reflection (faster!)
- Handle nullable types correctly
- Support all OData types

### 3. Type-Safe Client

```csharp
public partial class YourServiceClient
{
    public ODataQueryable<Product> Products { get; }
    public ODataQueryable<Order> Orders { get; }
    public ODataQueryable<Customer> Customers { get; }
    // ... all entity sets from metadata
}
```

### 4. LINQ Support

Write normal C# LINQ queries:

```csharp
// This LINQ expression...
var query = client.Products
    .Where(p => p.Price > 10 && p.InStock)
    .OrderBy(p => p.Name)
    .Skip(20)
    .Take(10);

// ...becomes this OData query automatically:
// /Products?$filter=Price gt 10 and InStock eq true&$orderby=Name asc&$skip=20&$top=10
```

## 🎨 Supported LINQ Operations

| LINQ Expression | OData Query |
|----------------|-------------|
| `.Where(p => p.Price > 10)` | `$filter=Price gt 10` |
| `.Where(p => p.Name == "Test")` | `$filter=Name eq 'Test'` |
| `.Where(p => p.InStock && p.Price < 100)` | `$filter=InStock eq true and Price lt 100` |
| `.OrderBy(p => p.Name)` | `$orderby=Name asc` |
| `.OrderByDescending(p => p.Price)` | `$orderby=Price desc` |
| `.Skip(10)` | `$skip=10` |
| `.Take(20)` | `$top=20` |
| `.Select(p => new { p.Name, p.Price })` | `$select=Name,Price` |
| `.Expand(o => o.Product)` | `$expand=Product` |

String methods work too:
- `.Where(p => p.Name.StartsWith("A"))` → `startswith(Name, 'A')`
- `.Where(p => p.Name.EndsWith("Z"))` → `endswith(Name, 'Z')`  
- `.Where(p => p.Name.Contains("mid"))` → `contains(Name, 'mid')`

## 🛠️ Advanced Features

Need more control? The tool supports advanced scenarios:

### Attribute-Based Customization

After generating the initial client, you can customize it:

```csharp
using Forge.OData.Attributes;

namespace MyApp.Services
{
    [ODataClient(
        MetadataFile = "ApiMetadata.xml",
        Endpoint = "https://api.example.com/odata"
    )]
    public partial class ApiClient
    {
        // Add your custom methods
        public async Task<Product> GetFeaturedProductAsync()
        {
            return (await Products
                .Where(p => p.Featured)
                .OrderByDescending(p => p.Rating)
                .Take(1)
                .ToListAsync())
                .FirstOrDefault();
        }
        
        // Add custom properties
        public string ServiceVersion => "v2.0";
    }
}
```

### Working with Multiple Environments

```csharp
// Development
var devClient = new ApiClient(
    httpClient, 
    "https://dev-api.example.com/odata"
);

// Production
var prodClient = new ApiClient(
    httpClient, 
    "https://api.example.com/odata"
);
```

## 📚 More Information

- **Technical documentation**: See [CONTRIBUTE.md](CONTRIBUTE.md) for detailed architecture, project structure, and contribution guidelines
- **Examples**: Check the `/sample` directory for working examples
- **Changelog**: See [CHANGELOG.md](CHANGELOG.md) for version history

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTE.md](CONTRIBUTE.md) for detailed information on:
- Project architecture and structure
- Development setup
- Building and testing
- Code generation workflow
- Contribution guidelines

## 📄 License

MIT



