using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Forge.OData.Service.Models;

var builder = WebApplication.CreateBuilder(args);

// Build the EDM model
var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Product>("Products");
modelBuilder.EntitySet<Order>("Orders");
modelBuilder.EntitySet<Customer>("Customers");
var edmModel = modelBuilder.GetEdmModel();

// Add OData services
builder.Services.AddControllers()
    .AddOData(options => options
        .Select()
        .Filter()
        .OrderBy()
        .Expand()
        .Count()
        .SetMaxTop(100)
        .AddRouteComponents("odata", edmModel));

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseRouting();

app.MapControllers();

app.Run();
