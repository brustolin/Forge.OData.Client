using System;

namespace Forge.OData.Attributes
{
    /// <summary>
    /// Attribute to mark a partial class for OData client generation.
    /// The generator will create a partial class matching the annotated class
    /// and add all the client functions to it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ODataClientAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the file path to the OData metadata XML file.
        /// </summary>
        public string MetadataFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the endpoint URL that should be used as the base URL for the client.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;
    }
}
