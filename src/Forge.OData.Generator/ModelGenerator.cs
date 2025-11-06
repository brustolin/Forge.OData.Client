using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Forge.OData.Generator
{
    public class ModelGenerator
    {
        public Dictionary<string, string> GenerateModels(ODataSchema schema)
        {
            var models = new Dictionary<string, string>();

            // Generate entity types
            foreach (var entityType in schema.EntityTypes)
            {
                var code = GenerateEntityClass(entityType, schema.Namespace);
                models[$"{entityType.Name}.g.cs"] = code;
            }

            // Generate complex types
            foreach (var complexType in schema.ComplexTypes)
            {
                var code = GenerateEntityClass(complexType, schema.Namespace);
                models[$"{complexType.Name}.g.cs"] = code;
            }

            return models;
        }

        private string GenerateEntityClass(ODataEntityType entityType, string namespaceName)
        {
            // Create namespace
            var namespaceDeclaration = NamespaceDeclaration(
                ParseName(namespaceName))
                .WithUsings(List(new[]
                {
                    UsingDirective(ParseName("System")),
                    UsingDirective(ParseName("System.Collections.Generic")),
                    UsingDirective(ParseName("System.ComponentModel.DataAnnotations")),
                    UsingDirective(ParseName("System.Text.Json.Serialization"))
                }));

            // Create class with JsonConverter attribute
            var classDeclaration = ClassDeclaration(entityType.Name)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAttributeLists(
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("JsonConverter"))
                            .AddArgumentListArguments(
                                AttributeArgument(
                                    TypeOfExpression(
                                        IdentifierName($"{entityType.Name}Converter")))))))
                .AddMembers(GenerateProperties(entityType).ToArray());

            // Add to namespace
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

            // Create compilation unit with #nullable enable directive
            var compilationUnit = CompilationUnit()
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace()
                .WithLeadingTrivia(
                    ParseLeadingTrivia("#nullable enable\n"));

            return compilationUnit.ToFullString();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateProperties(ODataEntityType entityType)
        {
            var members = new List<MemberDeclarationSyntax>();

            // Generate regular properties
            foreach (var property in entityType.Properties)
            {
                var propertyDeclaration = GenerateProperty(property, entityType.Keys.Contains(property.Name));
                members.Add(propertyDeclaration);
            }

            // Generate navigation properties
            foreach (var navProperty in entityType.NavigationProperties)
            {
                var propertyDeclaration = GenerateNavigationProperty(navProperty);
                members.Add(propertyDeclaration);
            }

            return members;
        }

        private PropertyDeclarationSyntax GenerateProperty(ODataProperty property, bool isKey)
        {
            var propertyType = MapODataType(property.Type, property.Nullable);
            
            var propertyDeclaration = PropertyDeclaration(
                ParseTypeName(propertyType),
                Identifier(property.Name))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

            // Add [Key] attribute for key properties
            if (isKey)
            {
                propertyDeclaration = propertyDeclaration.AddAttributeLists(
                    AttributeList(SingletonSeparatedList(
                        Attribute(IdentifierName("Key")))));
            }

            return propertyDeclaration;
        }

        private PropertyDeclarationSyntax GenerateNavigationProperty(ODataNavigationProperty navProperty)
        {
            var propertyType = MapNavigationType(navProperty.Type, navProperty.Nullable);

            var propertyDeclaration = PropertyDeclaration(
                ParseTypeName(propertyType),
                Identifier(navProperty.Name))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

            return propertyDeclaration;
        }

        private string MapODataType(string odataType, bool nullable)
        {
            var baseType = odataType.Replace("Edm.", "");
            
            string clrType = baseType switch
            {
                "String" => "string",
                "Int32" => "int",
                "Int64" => "long",
                "Int16" => "short",
                "Byte" => "byte",
                "Boolean" => "bool",
                "DateTime" => "DateTime",
                "DateTimeOffset" => "DateTimeOffset",
                "Decimal" => "decimal",
                "Double" => "double",
                "Single" => "float",
                "Guid" => "Guid",
                "Binary" => "byte[]",
                "TimeOfDay" => "TimeSpan",
                "Date" => "DateTime",
                _ => baseType.StartsWith("Collection(") ? MapCollectionType(baseType) : baseType
            };

            // Handle nullable value types
            if (nullable && IsValueType(clrType) && clrType != "string" && clrType != "byte[]")
            {
                return clrType + "?";
            }

            return clrType;
        }

        private string MapCollectionType(string collectionType)
        {
            // Collection(Edm.String) -> List<string>
            var innerType = collectionType
                .Replace("Collection(", "")
                .Replace(")", "")
                .Trim();

            var mappedInnerType = MapODataType(innerType, false);
            return $"List<{mappedInnerType}>";
        }

        private string MapNavigationType(string navType, bool nullable)
        {
            if (navType.StartsWith("Collection("))
            {
                // Collection(Namespace.EntityType) -> List<EntityType>
                var innerType = navType
                    .Replace("Collection(", "")
                    .Replace(")", "")
                    .Split('.')
                    .Last();
                return $"List<{innerType}>";
            }

            // Remove namespace prefix if present
            var typeName = navType.Split('.').Last();
            return typeName + (nullable ? "?" : "");
        }

        private bool IsValueType(string clrType)
        {
            var valueTypes = new HashSet<string>
            {
                "int", "long", "short", "byte",
                "bool", "DateTime", "DateTimeOffset",
                "decimal", "double", "float",
                "Guid", "TimeSpan"
            };

            return valueTypes.Contains(clrType);
        }
    }
}
