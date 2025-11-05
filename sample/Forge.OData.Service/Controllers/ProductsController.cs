using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Forge.OData.Service.Data;
using Forge.OData.Service.Models;

namespace Forge.OData.Service.Controllers;

public class ProductsController : ODataController
{
    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(SampleDataStore.Instance.Products.AsQueryable());
    }

    [EnableQuery]
    public IActionResult Get(int key)
    {
        var product = SampleDataStore.Instance.Products.FirstOrDefault(p => p.Id == key);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }
}
