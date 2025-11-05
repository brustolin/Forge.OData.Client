using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Forge.OData.Service.Data;
using Forge.OData.Service.Models;

namespace Forge.OData.Service.Controllers;

public class CustomersController : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(SampleDataStore.Instance.Customers.AsQueryable());
    }

    [EnableQuery]
    public IActionResult Get(int key)
    {
        var customer = SampleDataStore.Instance.Customers.FirstOrDefault(c => c.Id == key);
        if (customer == null)
        {
            return NotFound();
        }
        return Ok(customer);
    }
}
