using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Forge.OData.Generator
{
    public class JsonConverterGenerator
    {
        public Dictionary<string, string> GenerateConverters(ODataSchema schema)
        {
            var converters = new Dictionary<string, string>();

            // Generate converters for entity types
            foreach (var entityType in schema.EntityTypes)
            {
                var code = GenerateConverterClass(entityType, schema.Namespace);
                converters[$"{entityType.Name}Converter.g.cs"] = code;
            }

            // Generate converters for complex types
            foreach (var complexType in schema.ComplexTypes)
            {
                var code = GenerateConverterClass(complexType, schema.Namespace);
                converters[$"{complexType.Name}Converter.g.cs"] = code;
            }

            return converters;
        }

        private string GenerateConverterClass(ODataEntityType entityType, string namespaceName)
        {
            var className = $"{entityType.Name}Converter";

            // Create namespace
            var namespaceDeclaration = NamespaceDeclaration(
                ParseName(namespaceName))
                .WithUsings(List(new[]
                {
                    UsingDirective(ParseName("System")),
                    UsingDirective(ParseName("System.Text.Json")),
                    UsingDirective(ParseName("System.Text.Json.Serialization"))
                }));

            // Create class that inherits from JsonConverter<T>
            var classDeclaration = ClassDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(
                    SimpleBaseType(ParseTypeName($"JsonConverter<{entityType.Name}>")))
                .AddMembers(
                    GenerateReadMethod(entityType).ToArray())
                .AddMembers(
                    GenerateWriteMethod(entityType));

            // Add to namespace
            namespaceDeclaration = namespaceDeclaration.AddMembers(classDeclaration);

            // Create compilation unit
            var compilationUnit = CompilationUnit()
                .AddMembers(namespaceDeclaration)
                .NormalizeWhitespace();

            return compilationUnit.ToFullString();
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateReadMethod(ODataEntityType entityType)
        {
            // Read method signature:
            // public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            
            var statements = new List<StatementSyntax>();

            // if (reader.TokenType != JsonTokenType.StartObject)
            //     throw new JsonException();
            statements.Add(
                IfStatement(
                    BinaryExpression(
                        SyntaxKind.NotEqualsExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("reader"),
                            IdentifierName("TokenType")),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("JsonTokenType"),
                            IdentifierName("StartObject"))),
                    ThrowStatement(
                        ObjectCreationExpression(IdentifierName("JsonException"))
                            .WithArgumentList(ArgumentList()))));

            // var result = new T();
            statements.Add(
                LocalDeclarationStatement(
                    VariableDeclaration(
                        IdentifierName("var"))
                    .AddVariables(
                        VariableDeclarator(Identifier("result"))
                            .WithInitializer(
                                EqualsValueClause(
                                    ObjectCreationExpression(IdentifierName(entityType.Name))
                                        .WithArgumentList(ArgumentList()))))));

            // while (reader.Read())
            var whileBody = new List<StatementSyntax>();

            // if (reader.TokenType == JsonTokenType.EndObject)
            //     break;
            whileBody.Add(
                IfStatement(
                    BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("reader"),
                            IdentifierName("TokenType")),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("JsonTokenType"),
                            IdentifierName("EndObject"))),
                    BreakStatement()));

            // if (reader.TokenType != JsonTokenType.PropertyName)
            //     continue;
            whileBody.Add(
                IfStatement(
                    BinaryExpression(
                        SyntaxKind.NotEqualsExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("reader"),
                            IdentifierName("TokenType")),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("JsonTokenType"),
                            IdentifierName("PropertyName"))),
                    ContinueStatement()));

            // var propertyName = reader.GetString();
            whileBody.Add(
                LocalDeclarationStatement(
                    VariableDeclaration(
                        IdentifierName("var"))
                    .AddVariables(
                        VariableDeclarator(Identifier("propertyName"))
                            .WithInitializer(
                                EqualsValueClause(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("reader"),
                                            IdentifierName("GetString"))))))));

            // reader.Read();
            whileBody.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("reader"),
                            IdentifierName("Read")))));

            // Switch statement for properties
            var switchSections = new List<SwitchSectionSyntax>();

            // Add cases for each property
            foreach (var property in entityType.Properties)
            {
                switchSections.Add(GeneratePropertyCase(property));
            }

            // Add cases for navigation properties
            foreach (var navProperty in entityType.NavigationProperties)
            {
                switchSections.Add(GenerateNavigationPropertyCase(navProperty));
            }

            // Default case: skip
            switchSections.Add(
                SwitchSection()
                    .AddLabels(DefaultSwitchLabel())
                    .AddStatements(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("reader"),
                                    IdentifierName("Skip")))),
                        BreakStatement()));

            whileBody.Add(
                SwitchStatement(IdentifierName("propertyName"))
                    .WithSections(List(switchSections)));

            statements.Add(
                WhileStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("reader"),
                            IdentifierName("Read"))),
                    Block(whileBody)));

            // return result;
            statements.Add(ReturnStatement(IdentifierName("result")));

            yield return MethodDeclaration(
                    IdentifierName(entityType.Name),
                    Identifier("Read"))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("reader"))
                        .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                        .WithType(IdentifierName("Utf8JsonReader")),
                    Parameter(Identifier("typeToConvert"))
                        .WithType(IdentifierName("Type")),
                    Parameter(Identifier("options"))
                        .WithType(IdentifierName("JsonSerializerOptions")))
                .WithBody(Block(statements));
        }

        private SwitchSectionSyntax GeneratePropertyCase(ODataProperty property)
        {
            var statements = new List<StatementSyntax>();

            // Get the appropriate reader method based on type
            var readerExpression = GetReaderExpressionForProperty(property);

            // result.PropertyName = reader.GetXxx();
            statements.Add(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("result"),
                            IdentifierName(property.Name)),
                        readerExpression)));

            statements.Add(BreakStatement());

            return SwitchSection()
                .AddLabels(
                    CaseSwitchLabel(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(property.Name))))
                .WithStatements(List(statements));
        }

        private SwitchSectionSyntax GenerateNavigationPropertyCase(ODataNavigationProperty navProperty)
        {
            // For navigation properties, we'll deserialize using JsonSerializer
            var statements = new List<StatementSyntax>();

            var typeName = MapNavigationType(navProperty.Type, navProperty.Nullable);

            // result.NavProperty = JsonSerializer.Deserialize<Type>(ref reader, options);
            statements.Add(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("result"),
                            IdentifierName(navProperty.Name)),
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("JsonSerializer"),
                                GenericName(
                                    Identifier("Deserialize"))
                                .WithTypeArgumentList(
                                    TypeArgumentList(
                                        SingletonSeparatedList(ParseTypeName(typeName))))))
                        .AddArgumentListArguments(
                            Argument(IdentifierName("reader"))
                                .WithRefOrOutKeyword(Token(SyntaxKind.RefKeyword)),
                            Argument(IdentifierName("options"))))));

            statements.Add(BreakStatement());

            return SwitchSection()
                .AddLabels(
                    CaseSwitchLabel(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(navProperty.Name))))
                .WithStatements(List(statements));
        }

        private ExpressionSyntax GetReaderExpressionForProperty(ODataProperty property)
        {
            var baseType = property.Type.Replace("Edm.", "");

            var readerMethodName = baseType switch
            {
                "String" => "GetString",
                "Int32" => "GetInt32",
                "Int64" => "GetInt64",
                "Int16" => "GetInt16",
                "Byte" => "GetByte",
                "Boolean" => "GetBoolean",
                "Decimal" => "GetDecimal",
                "Double" => "GetDouble",
                "Single" => "GetSingle",
                "Guid" => "GetGuid",
                "Date" => "GetDateTime",
                "DateTime" => "GetDateTime",
                "DateTimeOffset" => "GetDateTimeOffset",
                _ => "GetString"
            };

            // For nullable value types (not strings), we need to check if the token is null first
            if (IsNullableValueType(property))
            {
                // reader.TokenType == JsonTokenType.Null ? null : reader.GetXxx()
                return ConditionalExpression(
                    BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("reader"),
                            IdentifierName("TokenType")),
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("JsonTokenType"),
                            IdentifierName("Null"))),
                    LiteralExpression(SyntaxKind.NullLiteralExpression),
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("reader"),
                            IdentifierName(readerMethodName))));
            }
            
            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("reader"),
                    IdentifierName(readerMethodName)));
        }

        private MethodDeclarationSyntax GenerateWriteMethod(ODataEntityType entityType)
        {
            var statements = new List<StatementSyntax>();

            // writer.WriteStartObject();
            statements.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WriteStartObject")))));

            // Write each property
            foreach (var property in entityType.Properties)
            {
                statements.AddRange(GeneratePropertyWrite(property));
            }

            // Write navigation properties
            foreach (var navProperty in entityType.NavigationProperties)
            {
                statements.AddRange(GenerateNavigationPropertyWrite(navProperty));
            }

            // writer.WriteEndObject();
            statements.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WriteEndObject")))));

            return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    Identifier("Write"))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.OverrideKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("writer"))
                        .WithType(IdentifierName("Utf8JsonWriter")),
                    Parameter(Identifier("value"))
                        .WithType(IdentifierName(entityType.Name)),
                    Parameter(Identifier("options"))
                        .WithType(IdentifierName("JsonSerializerOptions")))
                .WithBody(Block(statements));
        }

        private IEnumerable<StatementSyntax> GeneratePropertyWrite(ODataProperty property)
        {
            var baseType = property.Type.Replace("Edm.", "");

            var writeMethod = baseType switch
            {
                "String" => "WriteString",
                "Int32" or "Int64" or "Int16" or "Byte" => "WriteNumber",
                "Boolean" => "WriteBoolean",
                "Decimal" or "Double" or "Single" => "WriteNumber",
                "DateTime" or "Date" or "DateTimeOffset" or "Guid" => "WriteString",
                _ => "WriteString"
            };

            // For nullable value types (not strings), we need to check if value is null first
            if (IsNullableValueType(property))
            {
                // For nullable types, we need to check if value is null first
                // if (value.PropertyName.HasValue)
                //     writer.WriteXxx("PropertyName", value.PropertyName.Value);
                // else
                //     writer.WriteNull("PropertyName");
                
                yield return IfStatement(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("value"),
                            IdentifierName(property.Name)),
                        IdentifierName("HasValue")),
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("writer"),
                                IdentifierName(writeMethod)))
                        .AddArgumentListArguments(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal(property.Name))),
                            Argument(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName("value"),
                                        IdentifierName(property.Name)),
                                    IdentifierName("Value"))))),
                    ElseClause(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("writer"),
                                    IdentifierName("WriteNull")))
                            .AddArgumentListArguments(
                                Argument(
                                    LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        Literal(property.Name)))))));
            }
            else
            {
                // Non-nullable or String types: writer.WriteXxx("PropertyName", value.PropertyName);
                yield return ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName(writeMethod)))
                    .AddArgumentListArguments(
                        Argument(
                            LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(property.Name))),
                        Argument(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("value"),
                                IdentifierName(property.Name)))));
            }
        }

        private IEnumerable<StatementSyntax> GenerateNavigationPropertyWrite(ODataNavigationProperty navProperty)
        {
            // writer.WritePropertyName("NavPropertyName");
            yield return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WritePropertyName")))
                .AddArgumentListArguments(
                    Argument(
                        LiteralExpression(
                            SyntaxKind.StringLiteralExpression,
                            Literal(navProperty.Name)))));

            // JsonSerializer.Serialize(writer, value.NavProperty, options);
            yield return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("JsonSerializer"),
                        IdentifierName("Serialize")))
                .AddArgumentListArguments(
                    Argument(IdentifierName("writer")),
                    Argument(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("value"),
                            IdentifierName(navProperty.Name))),
                    Argument(IdentifierName("options"))));
        }

        private bool IsNullableValueType(ODataProperty property)
        {
            if (!property.Nullable)
                return false;

            var baseType = property.Type.Replace("Edm.", "");
            // String is a reference type, not a value type
            return baseType != "String";
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
    }
}
