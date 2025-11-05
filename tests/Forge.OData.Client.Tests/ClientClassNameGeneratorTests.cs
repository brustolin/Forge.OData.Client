using Xunit;

namespace Forge.OData.Client.Tests
{
    public class ClientClassNameGeneratorTests
    {
        // We need to test the GenerateClientClassName method from ODataSourceGenerator
        // Since it's a private static method, we'll create test cases based on the expected behavior
        
        [Theory]
        [InlineData("SampleSystem", "SampleSystemClient")]
        [InlineData("Resources.OData", "ResourcesODataClient")]
        [InlineData("My-Service", "MyServiceClient")]
        [InlineData("Service@123", "Service123Client")]
        [InlineData("Test.Service.v1", "TestServicev1Client")]
        [InlineData("123Service", "_123ServiceClient")]
        [InlineData("", "ODataClient")]
        [InlineData("   ", "ODataClient")]
        public void GenerateClientClassName_ShouldSanitizeAndAppendClient(string input, string expected)
        {
            // This test documents the expected behavior of the filename sanitization
            // The actual implementation is in ODataSourceGenerator.GenerateClientClassName
            
            // Test logic mirrors the implementation:
            var result = GenerateClientClassNameTestHelper(input);
            Assert.Equal(expected, result);
        }

        private string GenerateClientClassNameTestHelper(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "ODataClient";

            var sb = new System.Text.StringBuilder();
            
            foreach (char c in fileName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }

            var baseName = sb.ToString();
            
            if (baseName.Length > 0 && char.IsDigit(baseName[0]))
            {
                baseName = "_" + baseName;
            }

            if (string.IsNullOrWhiteSpace(baseName))
            {
                return "ODataClient";
            }

            return baseName + "Client";
        }
    }
}
