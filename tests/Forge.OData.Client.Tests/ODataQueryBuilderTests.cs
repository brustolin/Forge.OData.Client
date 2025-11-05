using Forge.OData.Client;

namespace Forge.OData.Client.Tests
{
    public class ODataQueryBuilderTests
    {
        [Fact]
        public void Build_WithFilter_ReturnsFilterQuery()
        {
            // Arrange
            var builder = new ODataQueryBuilder();
            builder.AddFilter("Price gt 10");

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("$filter=%28Price%20gt%2010%29", result);
        }

        [Fact]
        public void Build_WithOrderBy_ReturnsOrderByQuery()
        {
            // Arrange
            var builder = new ODataQueryBuilder();
            builder.AddOrderBy("Name", ascending: true);

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("$orderby=Name%20asc", result);
        }

        [Fact]
        public void Build_WithTopAndSkip_ReturnsTopSkipQuery()
        {
            // Arrange
            var builder = new ODataQueryBuilder();
            builder.SetTop(10);
            builder.SetSkip(20);

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("$top=10&$skip=20", result);
        }

        [Fact]
        public void Build_WithSelect_ReturnsSelectQuery()
        {
            // Arrange
            var builder = new ODataQueryBuilder();
            builder.SetSelect(new List<string> { "Name", "Price" });

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("$select=Name%2CPrice", result);
        }

        [Fact]
        public void Build_WithExpand_ReturnsExpandQuery()
        {
            // Arrange
            var builder = new ODataQueryBuilder();
            builder.AddExpand("Orders");

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("$expand=Orders", result);
        }

        [Fact]
        public void Build_WithMultipleFilters_ReturnsCombinedFilterQuery()
        {
            // Arrange
            var builder = new ODataQueryBuilder();
            builder.AddFilter("Price gt 10");
            builder.AddFilter("InStock eq true");

            // Act
            var result = builder.Build();

            // Assert
            Assert.Contains("$filter=", result);
            Assert.Contains("Price%20gt%2010", result);
            Assert.Contains("InStock%20eq%20true", result);
            Assert.Contains("and", result);
        }

        [Fact]
        public void Clone_CreatesIndependentCopy()
        {
            // Arrange
            var original = new ODataQueryBuilder();
            original.AddFilter("Price gt 10");
            original.SetTop(5);

            // Act
            var clone = original.Clone();
            clone.AddFilter("InStock eq true");
            clone.SetTop(10);

            // Assert
            var originalResult = original.Build();
            var cloneResult = clone.Build();
            
            Assert.NotEqual(originalResult, cloneResult);
            Assert.Contains("$top=5", originalResult);
            Assert.Contains("$top=10", cloneResult);
        }
    }
}
