using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Forge.OData.Generator
{
    public class ODataMetadataParser
    {
        private static readonly XNamespace EdmNamespace = "http://docs.oasis-open.org/odata/ns/edm";
        private static readonly XNamespace EdmxNamespace = "http://docs.oasis-open.org/odata/ns/edmx";

        public ODataSchema Parse(string xmlContent)
        {
            var doc = XDocument.Parse(xmlContent);
            var schema = new ODataSchema();

            // Find all Schema elements
            var schemaElements = doc.Descendants()
                .Where(e => e.Name.LocalName == "Schema")
                .ToList();

            foreach (var schemaElement in schemaElements)
            {
                var ns = schemaElement.Attribute("Namespace")?.Value ?? "Default";

                // Parse EntityTypes
                var entityTypes = schemaElement.Elements()
                    .Where(e => e.Name.LocalName == "EntityType")
                    .ToList();

                foreach (var entityType in entityTypes)
                {
                    var entity = ParseEntityType(entityType);
                    schema.EntityTypes.Add(entity);
                }

                // Parse ComplexTypes
                var complexTypes = schemaElement.Elements()
                    .Where(e => e.Name.LocalName == "ComplexType")
                    .ToList();

                foreach (var complexType in complexTypes)
                {
                    var entity = ParseComplexType(complexType);
                    schema.ComplexTypes.Add(entity);
                }

                // Only set the namespace if this schema contains entity types or complex types
                // This ensures we use the namespace of the schema with entity definitions,
                // not the container schema
                if (entityTypes.Any() || complexTypes.Any())
                {
                    schema.Namespace = ns;
                }

                // Parse EntityContainer
                var containerElement = schemaElement.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == "EntityContainer");

                if (containerElement != null)
                {
                    schema.EntityContainer = ParseEntityContainer(containerElement);
                }
            }

            return schema;
        }

        private ODataEntityType ParseEntityType(XElement element)
        {
            var entity = new ODataEntityType
            {
                Name = element.Attribute("Name")?.Value ?? "Unknown",
                BaseType = element.Attribute("BaseType")?.Value
            };

            // Parse Properties
            var properties = element.Elements()
                .Where(e => e.Name.LocalName == "Property")
                .ToList();

            foreach (var prop in properties)
            {
                entity.Properties.Add(ParseProperty(prop));
            }

            // Parse NavigationProperties
            var navProps = element.Elements()
                .Where(e => e.Name.LocalName == "NavigationProperty")
                .ToList();

            foreach (var navProp in navProps)
            {
                entity.NavigationProperties.Add(ParseNavigationProperty(navProp));
            }

            // Parse Keys
            var keyElement = element.Elements()
                .FirstOrDefault(e => e.Name.LocalName == "Key");

            if (keyElement != null)
            {
                var keyProps = keyElement.Elements()
                    .Where(e => e.Name.LocalName == "PropertyRef")
                    .Select(e => e.Attribute("Name")?.Value)
                    .Where(n => n != null)
                    .ToList();

                entity.Keys.AddRange(keyProps);
            }

            return entity;
        }

        private ODataEntityType ParseComplexType(XElement element)
        {
            var entity = new ODataEntityType
            {
                Name = element.Attribute("Name")?.Value ?? "Unknown",
                IsComplexType = true
            };

            var properties = element.Elements()
                .Where(e => e.Name.LocalName == "Property")
                .ToList();

            foreach (var prop in properties)
            {
                entity.Properties.Add(ParseProperty(prop));
            }

            return entity;
        }

        private ODataProperty ParseProperty(XElement element)
        {
            return new ODataProperty
            {
                Name = element.Attribute("Name")?.Value ?? "Unknown",
                Type = element.Attribute("Type")?.Value ?? "Edm.String",
                Nullable = ParseBool(element.Attribute("Nullable")?.Value, true),
                MaxLength = element.Attribute("MaxLength")?.Value
            };
        }

        private ODataNavigationProperty ParseNavigationProperty(XElement element)
        {
            return new ODataNavigationProperty
            {
                Name = element.Attribute("Name")?.Value ?? "Unknown",
                Type = element.Attribute("Type")?.Value ?? "Unknown",
                Nullable = ParseBool(element.Attribute("Nullable")?.Value, true)
            };
        }

        private ODataEntityContainer ParseEntityContainer(XElement element)
        {
            var container = new ODataEntityContainer
            {
                Name = element.Attribute("Name")?.Value ?? "Container"
            };

            var entitySets = element.Elements()
                .Where(e => e.Name.LocalName == "EntitySet")
                .ToList();

            foreach (var entitySet in entitySets)
            {
                container.EntitySets.Add(new ODataEntitySet
                {
                    Name = entitySet.Attribute("Name")?.Value ?? "Unknown",
                    EntityType = entitySet.Attribute("EntityType")?.Value ?? "Unknown"
                });
            }

            return container;
        }

        private bool ParseBool(string value, bool defaultValue)
        {
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class ODataSchema
    {
        public string Namespace { get; set; } = "Default";
        public List<ODataEntityType> EntityTypes { get; set; } = new List<ODataEntityType>();
        public List<ODataEntityType> ComplexTypes { get; set; } = new List<ODataEntityType>();
        public ODataEntityContainer EntityContainer { get; set; }
    }

    public class ODataEntityType
    {
        public string Name { get; set; }
        public string BaseType { get; set; }
        public bool IsComplexType { get; set; }
        public List<ODataProperty> Properties { get; set; } = new List<ODataProperty>();
        public List<ODataNavigationProperty> NavigationProperties { get; set; } = new List<ODataNavigationProperty>();
        public List<string> Keys { get; set; } = new List<string>();
    }

    public class ODataProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Nullable { get; set; }
        public string MaxLength { get; set; }
    }

    public class ODataNavigationProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Nullable { get; set; }
    }

    public class ODataEntityContainer
    {
        public string Name { get; set; }
        public List<ODataEntitySet> EntitySets { get; set; } = new List<ODataEntitySet>();
    }

    public class ODataEntitySet
    {
        public string Name { get; set; }
        public string EntityType { get; set; }
    }
}
