using System.ComponentModel.DataAnnotations;

namespace Forge.OData.Service.Models;

public class Customer
{
    [Key]
    public int Id { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    
    public DateOnly? DateOfBirth { get; set; }
    
    public List<Order> Orders { get; set; } = new();
}
