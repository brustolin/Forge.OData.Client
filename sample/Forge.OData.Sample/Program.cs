using Forge.OData.Service.Models;

// This is a sample demonstrating how to use the OData client generator
// The generator will create model classes and a client class from the SampleMetadata.xml file

Console.WriteLine("OData Client Sample");
Console.WriteLine("===================");
Console.WriteLine();
Console.WriteLine("This sample demonstrates the OData source generator capabilities:");
Console.WriteLine("1. Model classes are generated from OData metadata XML");
Console.WriteLine("2. ODataClient class is generated to access entity sets");
Console.WriteLine("3. LINQ expressions are translated to OData query syntax");
Console.WriteLine();

// IMPORTANT: Start the OData service first:
// dotnet run --project tests/Forge.OData.Service/Forge.OData.Service.csproj
Console.WriteLine("Make sure the OData service is running at http://localhost:5000");
Console.WriteLine("Start it with: dotnet run --project tests/Forge.OData.Service/Forge.OData.Service.csproj");
Console.WriteLine();

try
{
    using var httpClient = new HttpClient();
    var client = new SampleMetadataClient(httpClient, "http://localhost:5000/odata");

    // Example 1: Query all products
    Console.WriteLine("Example 1: Fetching all products...");
    var allProducts = await client.Products.ToListAsync();
    Console.WriteLine($"Found {allProducts.Count} products:");
    foreach (var product in allProducts.Take(3))
    {
        Console.WriteLine($"  - {product.Name}: ${product.Price} ({product.Category})");
    }
    Console.WriteLine();

    // Example 2: Query products with LINQ filtering
    Console.WriteLine("Example 2: Querying products with price > $50 and in stock...");
    var products = await client.Products
        .Where(p => p.Price > 50 && p.InStock)
        .OrderBy(p => p.Name)
        .ToListAsync();
    
    Console.WriteLine($"Found {products.Count} products:");
    foreach (var product in products)
    {
        Console.WriteLine($"  - {product.Name}: ${product.Price}");
    }
    Console.WriteLine();

    // Example 3: Query with expansion
    Console.WriteLine("Example 3: Querying orders with expanded product information...");
    var ordersWithProducts = await client.Orders
        .Expand(o => o.Product)
        .Where(o => o.TotalAmount > 50)
        .ToListAsync();
    
    Console.WriteLine($"Found {ordersWithProducts.Count} orders:");
    foreach (var order in ordersWithProducts.Take(3))
    {
        Console.WriteLine($"  - Order {order.OrderNumber}: {order.Quantity}x {order.Product?.Name ?? "Unknown"} = ${order.TotalAmount}");
    }
    Console.WriteLine();

    // Example 4: Get specific product by ID
    Console.WriteLine("Example 4: Getting product with ID=1...");
    var specificProduct = await client.Products
        .Where(p => p.Id == 1)
        .ToListAsync();
    
    if (specificProduct.Any())
    {
        var product = specificProduct.First();
        Console.WriteLine($"Product: {product.Name}");
        Console.WriteLine($"  Price: ${product.Price}");
        Console.WriteLine($"  In Stock: {product.InStock}");
        Console.WriteLine($"  Category: {product.Category}");
    }
    Console.WriteLine();

    // Example 5: Query with nesting filters
    Console.WriteLine("Example 5: Querying order in which the product is on stock...");
    var ordersWithInStockProducts = await client.Orders
        .Expand(o => o.Product)
        .Where(o => o.Product != null && o.Product.InStock)
        .ToListAsync();

    foreach (var order in ordersWithInStockProducts.Take(3))
    {
        Console.WriteLine($"  - Order {order.OrderNumber}: {order.Quantity}x {order.Product?.Name ?? "Unknown"} = ${order.TotalAmount}");
    }

    // Example 6: Query with collection count
    Console.WriteLine("Example 6: Querying customer with more than 2 orders");
    var customersWithManyOrders = await client.Customers
        .Expand(c => c.Orders)
        .Where(c => c.Orders.Count >= 2)
        .ToListAsync();

    foreach (var customer in customersWithManyOrders)
    {
        Console.WriteLine($"  - Customer {customer.FirstName} has {customer.Orders?.Count ?? 0} orders");
    }

    Console.WriteLine("✓ All examples completed successfully!");
    Console.WriteLine();
    Console.WriteLine("Generated classes:");
    Console.WriteLine("- Product");
    Console.WriteLine("- Order");
    Console.WriteLine("- Customer");
    Console.WriteLine("- SampleMetadataClient with entity sets: Products, Orders, Customers");
}
catch (HttpRequestException ex)
{
    Console.WriteLine();
    Console.WriteLine("ERROR: Could not connect to the OData service.");
    Console.WriteLine("Make sure the service is running with:");
    Console.WriteLine("  dotnet run --project tests/Forge.OData.Service/Forge.OData.Service.csproj");
    Console.WriteLine();
    Console.WriteLine($"Error details: {ex.Message}");
}

