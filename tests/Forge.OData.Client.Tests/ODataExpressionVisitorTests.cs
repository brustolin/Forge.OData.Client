using Forge.OData.Client.Core;
using System.Linq.Expressions;

namespace Forge.OData.Client.Tests
{
    public class ODataExpressionVisitorTests
    {
        private readonly ODataExpressionVisitor _visitor;

        public ODataExpressionVisitorTests()
        {
            _visitor = new ODataExpressionVisitor();
        }

        [Fact]
        public void Visit_EqualExpression_ReturnsODataFilter()
        {
            // Arrange
            Expression<Func<TestEntity, bool>> expression = x => x.Name == "Test";

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("Name eq 'Test'", result);
        }

        [Fact]
        public void Visit_GreaterThanExpression_ReturnsODataFilter()
        {
            // Arrange
            Expression<Func<TestEntity, bool>> expression = x => x.Price > 100;

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("Price gt 100", result);
        }

        [Fact]
        public void Visit_AndAlsoExpression_ReturnsODataFilter()
        {
            // Arrange
            Expression<Func<TestEntity, bool>> expression = x => x.Price > 10 && x.InStock;

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("Price gt 10 and InStock", result);
        }

        [Fact]
        public void Visit_StartsWithExpression_ReturnsODataFilter()
        {
            // Arrange
            Expression<Func<TestEntity, bool>> expression = x => x.Name.StartsWith("Test");

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("startswith(Name, 'Test')", result);
        }

        [Fact]
        public void Visit_ContainsExpression_ReturnsODataFilter()
        {
            // Arrange
            Expression<Func<TestEntity, bool>> expression = x => x.Name.Contains("Test");

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("contains(Name, 'Test')", result);
        }

        [Fact]
        public void Visit_NotExpression_ReturnsODataFilter()
        {
            // Arrange
            Expression<Func<TestEntity, bool>> expression = x => !x.InStock;

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("not (InStock)", result);
        }

        [Fact]
        public void Visit_NestedPropertyAccess_ReturnsODataPathFilter()
        {
            // Arrange
            Expression<Func<OrderEntity, bool>> expression = o => o.Product != null && o.Product.InStock;

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("Product ne null and Product/InStock", result);
        }

        [Fact]
        public void Visit_DeeplyNestedPropertyAccess_ReturnsODataPathFilter()
        {
            // Arrange
            Expression<Func<OrderEntity, bool>> expression = o => o.Product.Name == "Test";

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("Product/Name eq 'Test'", result);
        }

        [Fact]
        public void Visit_CollectionCount_ReturnsODataCountFilter()
        {
            // Arrange
            Expression<Func<CustomerEntity, bool>> expression = c => c.Orders.Count > 3;

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("Orders/$count gt 3", result);
        }

        [Fact]
        public void Visit_CollectionCountWithNullCheck_ReturnsODataFilter()
        {
            // Arrange
            Expression<Func<CustomerEntity, bool>> expression = c => c.Orders != null && c.Orders.Count > 3;

            // Act
            var result = _visitor.Visit(expression);

            // Assert
            Assert.Equal("Orders ne null and Orders/$count gt 3", result);
        }

        private class TestEntity
        {
            public string Name { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public bool InStock { get; set; }
        }

        private class OrderEntity
        {
            public int Id { get; set; }
            public ProductEntity Product { get; set; } = new();
        }

        private class ProductEntity
        {
            public string Name { get; set; } = string.Empty;
            public bool InStock { get; set; }
        }

        private class CustomerEntity
        {
            public string Name { get; set; } = string.Empty;
            public List<OrderEntity> Orders { get; set; } = new();
        }
    }
}
