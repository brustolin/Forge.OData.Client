using System.ComponentModel.DataAnnotations;

namespace Forge.OData.Service.Models;

public class Order
{
    [Key]
    public int Id { get; set; }
    
    public string OrderNumber { get; set; } = string.Empty;
    
    public int ProductId { get; set; }
    
    public int CustomerId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public DateTimeOffset OrderDate { get; set; }
    
    public Product? Product { get; set; }
    
    public Customer? Customer { get; set; }
}
