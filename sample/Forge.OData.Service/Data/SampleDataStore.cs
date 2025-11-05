using Forge.OData.Service.Models;

namespace Forge.OData.Service.Data;

public class SampleDataStore
{
    private static readonly Lazy<SampleDataStore> _instance = new(() => new SampleDataStore());
    
    public static SampleDataStore Instance => _instance.Value;
    
    public List<Product> Products { get; }
    public List<Order> Orders { get; }
    public List<Customer> Customers { get; }
    
    private SampleDataStore()
    {
        Products = new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Laptop",
                Description = "High-performance laptop for developers",
                Price = 1299.99m,
                Category = "Electronics",
                InStock = true,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-30)
            },
            new Product
            {
                Id = 2,
                Name = "Wireless Mouse",
                Description = "Ergonomic wireless mouse",
                Price = 29.99m,
                Category = "Electronics",
                InStock = true,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-45)
            },
            new Product
            {
                Id = 3,
                Name = "Mechanical Keyboard",
                Description = "RGB mechanical keyboard with blue switches",
                Price = 89.99m,
                Category = "Electronics",
                InStock = true,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-20)
            },
            new Product
            {
                Id = 4,
                Name = "USB-C Hub",
                Description = "Multi-port USB-C hub with HDMI",
                Price = 49.99m,
                Category = "Electronics",
                InStock = false,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new Product
            {
                Id = 5,
                Name = "Desk Lamp",
                Description = "LED desk lamp with adjustable brightness",
                Price = 39.99m,
                Category = "Office Supplies",
                InStock = true,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-60)
            },
            new Product
            {
                Id = 6,
                Name = "Monitor Stand",
                Description = "Adjustable monitor stand with storage",
                Price = 59.99m,
                Category = "Office Supplies",
                InStock = true,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-15)
            },
            new Product
            {
                Id = 7,
                Name = "Webcam",
                Description = "1080p webcam with built-in microphone",
                Price = 79.99m,
                Category = "Electronics",
                InStock = true,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-25)
            },
            new Product
            {
                Id = 8,
                Name = "Headphones",
                Description = "Noise-cancelling over-ear headphones",
                Price = 199.99m,
                Category = "Electronics",
                InStock = false,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-35)
            },
            new Product
            {
                Id = 9,
                Name = "Notebook",
                Description = "Premium hardcover notebook",
                Price = 14.99m,
                Category = "Office Supplies",
                InStock = true,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-5)
            },
            new Product
            {
                Id = 10,
                Name = "Pen Set",
                Description = "Set of 10 gel pens in assorted colors",
                Price = 9.99m,
                Category = "Office Supplies",
                InStock = true,
                CreatedDate = DateTimeOffset.UtcNow.AddDays(-50)
            }
        };
        
        
        Orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                OrderNumber = "ORD-2024-001",
                ProductId = 1,
                CustomerId = 1,
                Quantity = 1,
                TotalAmount = 1299.99m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-10)
            },
            new Order
            {
                Id = 2,
                OrderNumber = "ORD-2024-002",
                ProductId = 2,
                CustomerId = 5,
                Quantity = 2,
                TotalAmount = 59.98m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-8)
            },
            new Order
            {
                Id = 3,
                OrderNumber = "ORD-2024-003",
                ProductId = 3,
                CustomerId = 2,
                Quantity = 1,
                TotalAmount = 89.99m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-7)
            },
            new Order
            {
                Id = 4,
                OrderNumber = "ORD-2024-004",
                ProductId = 5,
                CustomerId = 3,
                Quantity = 3,
                TotalAmount = 119.97m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-5)
            },
            new Order
            {
                Id = 5,
                OrderNumber = "ORD-2024-005",
                ProductId = 7,
                CustomerId = 3,
                Quantity = 1,
                TotalAmount = 79.99m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-4)
            },
            new Order
            {
                Id = 6,
                OrderNumber = "ORD-2024-006",
                ProductId = 9,
                CustomerId = 4,
                Quantity = 5,
                TotalAmount = 74.95m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-3)
            },
            new Order
            {
                Id = 7,
                OrderNumber = "ORD-2024-007",
                ProductId = 10,
                CustomerId = 4,
                Quantity = 2,
                TotalAmount = 19.98m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-2)
            },
            new Order
            {
                Id = 8,
                OrderNumber = "ORD-2024-008",
                ProductId = 6,
                CustomerId = 4,
                Quantity = 1,
                TotalAmount = 59.99m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new Order
            {
                Id = 9,
                OrderNumber = "ORD-2024-009",
                ProductId = 6,
                CustomerId = 5,
                Quantity = 1,
                TotalAmount = 59.99m,
                OrderDate = DateTimeOffset.UtcNow.AddDays(-1)
            }
        };

        Customers = new List<Customer>
        {
            new Customer
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                DateOfBirth = new DateOnly(1990, 5, 15),
                Orders = new List<Order> { Orders[0] }
            },
            new Customer
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                DateOfBirth = new DateOnly(1985, 8, 22),
                Orders = new List<Order> { Orders[2] }
            },
            new Customer
            {
                Id = 3,
                FirstName = "Bob",
                LastName = "Johnson",
                Email = "bob.johnson@example.com",
                DateOfBirth = new DateOnly(1992, 3, 10),
                Orders = new List<Order> { Orders[3], Orders[4] }
            },
            new Customer
            {
                Id = 4,
                FirstName = "Alice",
                LastName = "Williams",
                Email = "alice.williams@example.com",
                DateOfBirth = new DateOnly(1988, 11, 30),
                Orders = new List<Order> { Orders[5], Orders[6], Orders[7] }
            },
            new Customer
            {
                Id = 5,
                FirstName = "Charlie",
                LastName = "Brown",
                Email = "charlie.brown@example.com",
                DateOfBirth = new DateOnly(1995, 7, 8),
                Orders = new List<Order> { Orders[1], Orders[8] }
            }
        };

        // Set up navigation properties
        foreach (var order in Orders)
        {
            order.Product = Products.FirstOrDefault(p => p.Id == order.ProductId);
            if (order.Product != null)
            {
                order.Product.Orders.Add(order);
            }
            
            order.Customer = Customers.FirstOrDefault(c => c.Id == order.CustomerId);
        }
    }
}
