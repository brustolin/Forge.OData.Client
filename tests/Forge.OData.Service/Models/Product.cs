using System.ComponentModel.DataAnnotations;

namespace Forge.OData.Service.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public decimal Price { get; set; }
    
    public string? Category { get; set; }
    
    public bool InStock { get; set; }
    
    public DateTimeOffset CreatedDate { get; set; }
    
    public List<Order> Orders { get; set; } = new();
}
