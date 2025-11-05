using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Forge.OData.Service.Data;
using Forge.OData.Service.Models;

namespace Forge.OData.Service.Controllers;

public class OrdersController : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(SampleDataStore.Instance.Orders.AsQueryable());
    }

    [EnableQuery]
    public IActionResult Get(int key)
    {
        var order = SampleDataStore.Instance.Orders.FirstOrDefault(o => o.Id == key);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }
}
