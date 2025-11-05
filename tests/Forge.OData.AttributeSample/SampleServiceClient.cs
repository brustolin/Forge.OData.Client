using Forge.OData.Attributes;

namespace MyApp.Services
{
    /// <summary>
    /// Custom OData client for accessing the Sample Service.
    /// The generator will create a partial class with all client functionality.
    /// </summary>
    [ODataClient(MetadataFile = "SampleMetadata.xml", Endpoint = "https://services.odata.org/V4/OData/OData.svc")]
    public partial class SampleServiceClient
    {
        // You can add custom methods and properties here
        // The generator will add entity set properties and client functionality
        
        public string ServiceName => "Sample OData Service";
    }
}
