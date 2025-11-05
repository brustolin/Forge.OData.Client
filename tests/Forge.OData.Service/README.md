# Forge.OData.Service

A simple ASP.NET Core Web API project with OData endpoints and sample data for testing the Forge.OData.Client library.

## Overview

This service provides OData v4 endpoints with in-memory sample data that can be used to validate the Forge.OData.Client library functionality.

## Features

- **OData v4 Support**: Full OData query support including $filter, $select, $expand, $orderby, $top, $skip
- **Sample Data**: Pre-populated with Products, Orders, and Customers
- **In-Memory Data Store**: Simple singleton data store for quick testing
- **Entity Types**: Product, Order, and Customer with navigation properties

## Running the Service

```bash
dotnet run --project tests/Forge.OData.Service/Forge.OData.Service.csproj
```

By default, the service runs on `http://localhost:5000` (or as configured in `launchSettings.json`).

## Endpoints

### OData Metadata
- **URL**: `http://localhost:5000/odata/$metadata`
- **Description**: Returns the EDM (Entity Data Model) metadata

### Products
- **URL**: `http://localhost:5000/odata/Products`
- **Sample Data**: 10 products in Electronics and Office Supplies categories
- **Example Query**: `http://localhost:5000/odata/Products?$filter=Price gt 50&$orderby=Name`

### Orders
- **URL**: `http://localhost:5000/odata/Orders`
- **Sample Data**: 8 orders linked to products
- **Example Query**: `http://localhost:5000/odata/Orders?$expand=Product`

### Customers
- **URL**: `http://localhost:5000/odata/Customers`
- **Sample Data**: 5 customers
- **Example Query**: `http://localhost:5000/odata/Customers?$filter=contains(Email,'example')`

## Sample OData Queries

### Get all products with price greater than 50
```
GET http://localhost:5000/odata/Products?$filter=Price gt 50
```

### Get products sorted by name
```
GET http://localhost:5000/odata/Products?$orderby=Name
```

### Get in-stock products in Electronics category
```
GET http://localhost:5000/odata/Products?$filter=InStock eq true and Category eq 'Electronics'
```

### Get orders with expanded product information
```
GET http://localhost:5000/odata/Orders?$expand=Product
```

### Get specific product by ID
```
GET http://localhost:5000/odata/Products(1)
```

### Select specific fields
```
GET http://localhost:5000/odata/Products?$select=Name,Price
```

### Pagination
```
GET http://localhost:5000/odata/Products?$top=5&$skip=5
```

## Using with Forge.OData.Client

This service can be used to test the Forge.OData.Client library:

1. Start the service
2. Download the metadata: `curl http://localhost:5000/odata/$metadata > ServiceMetadata.xml`
3. Use the metadata with the Forge.OData.Generator to generate client code
4. Query the service using the generated client

## Data Models

### Product
- Id (int, key)
- Name (string)
- Description (string, nullable)
- Price (decimal)
- Category (string, nullable)
- InStock (bool)
- CreatedDate (DateTimeOffset)
- Orders (navigation to Order collection)

### Order
- Id (int, key)
- OrderNumber (string)
- ProductId (int)
- Quantity (int)
- TotalAmount (decimal)
- OrderDate (DateTimeOffset)
- Product (navigation to Product)

### Customer
- Id (int, key)
- FirstName (string)
- LastName (string)
- Email (string, nullable)
- DateOfBirth (DateOnly, nullable)
- Orders (navigation to Order collection)

## Technology Stack

- .NET 9
- ASP.NET Core
- Microsoft.AspNetCore.OData 9.4.1
