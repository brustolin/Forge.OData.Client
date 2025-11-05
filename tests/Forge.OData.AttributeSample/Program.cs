using System;
using System.Net.Http;
using MyApp.Services;

Console.WriteLine("Forge.OData.Client - Attribute-Based Generation Demo");
Console.WriteLine("================================================");
Console.WriteLine();

// Create an HttpClient (in a real app, use IHttpClientFactory)
using var httpClient = new HttpClient();

// Create the client using the generated partial class
// The endpoint is already configured via the attribute, so we just need the HttpClient
var client = new SampleServiceClient(httpClient);

Console.WriteLine($"Service: {client.ServiceName}");
Console.WriteLine($"Endpoint configured via attribute");
Console.WriteLine();

// The client has all the entity set properties generated from metadata
// For example: client.Products, client.Orders, client.Customers
Console.WriteLine("Generated entity sets:");
Console.WriteLine("  - Products (ODataQueryable<Product>)");
Console.WriteLine("  - Orders (ODataQueryable<Order>)");
Console.WriteLine("  - Customers (ODataQueryable<Customer>)");
Console.WriteLine();

Console.WriteLine("Each model class has a custom JsonConverter for efficient deserialization.");
Console.WriteLine("No reflection needed!");
Console.WriteLine();

// In a real application, you would use the client like this:
// var products = await client.Products
//     .Where(p => p.Price > 10)
//     .OrderBy(p => p.Name)
//     .ToListAsync();

Console.WriteLine("Sample completed successfully!");
