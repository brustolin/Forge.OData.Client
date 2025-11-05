# Forge.OData.Sample

Sample application demonstrating how to use the OData client generator with a real OData service.

## Overview

This sample shows the OData source generator capabilities:
1. Model classes are generated from OData metadata XML
2. Client class is generated based on the XML filename (SampleMetadata.xml → SampleMetadataClient)
3. LINQ expressions are translated to OData query syntax

## Running the Sample

### Prerequisites

You need to have the OData service running first.

### Step 1: Start the OData Service

In one terminal, start the OData service:

```bash
dotnet run --project tests/Forge.OData.Service/Forge.OData.Service.csproj
```

The service will start at `http://localhost:5000` and provide OData endpoints with sample data.

### Step 2: Run the Sample App

In another terminal, run the sample application:

```bash
dotnet run --project tests/Forge.OData.Sample/Forge.OData.Sample.csproj
```

## What the Sample Demonstrates

The sample application performs several OData queries:

1. **Fetch all products** - Simple ToListAsync() call
2. **Filter and sort** - LINQ Where() and OrderBy() translated to OData $filter and $orderby
3. **Expand navigation properties** - Expand() translated to OData $expand
4. **Get specific entity** - Where() with ID filter

## Generated Code

The source generator creates:
- `Product`, `Order`, `Customer` model classes
- `SampleMetadataClient` class (based on SampleMetadata.xml) with entity sets: `Products`, `Orders`, `Customers`
- Custom `JsonConverter<T>` for each model for efficient deserialization

All generated code is in the `obj/Debug/net9.0/Forge.OData.Generator/` folder.

## Example Output

```
OData Client Sample
===================

Example 1: Fetching all products...
Found 10 products:
  - Laptop: $1299.99 (Electronics)
  - Wireless Mouse: $29.99 (Electronics)
  - Mechanical Keyboard: $89.99 (Electronics)

Example 2: Querying products with price > $50 and in stock...
Found 4 products:
  - Laptop: $1299.99
  - Mechanical Keyboard: $89.99
  - Monitor Stand: $59.99
  - Webcam: $79.99

✓ All examples completed successfully!
```
