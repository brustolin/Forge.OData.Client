using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Forge.OData.Generator
{
    public class PartialClientGenerator
    {
        public string GeneratePartialClient(ODataSchema schema, string className, string namespaceName, string defaultEndpoint)
        {
            // Create namespace
            var namespaceDeclaration = NamespaceDeclaration(
                ParseName(namespaceName))
                .WithUsings(List(new[]
                {
                    UsingDirective(ParseName("System")),
                    UsingDirective(ParseName("System.Collections.Generic")),
                    UsingDirective(ParseName("System.Linq")),
                    UsingDirective(ParseName("System.Net.Http")),
                    UsingDirective(ParseName("System.Text.Json")),
                    UsingDirective(ParseName("System.Threading.Tasks")),
                    UsingDirective(ParseName("Forge.OData.Client"))
                }));

            // Create partial class
            var classDeclaration = ClassDeclaration(className)
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.PartialKeyword))
                .AddMembers(GenerateClientMembers(schema, className, defaultEndpoint).ToArray());

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

        private IEnumerable<MemberDeclarationSyntax> GenerateClientMembers(ODataSchema schema, string className, string defaultEndpoint)
        {
            var members = new List<MemberDeclarationSyntax>();

            // Add private fields
            members.Add(GenerateHttpClientField());
            members.Add(GenerateBaseUrlField());

            // Add constructor
            members.Add(GenerateConstructor(className, defaultEndpoint));

            // Add entity set properties
            if (schema.EntityContainer != null)
            {
                foreach (var entitySet in schema.EntityContainer.EntitySets)
                {
                    members.Add(GenerateEntitySetProperty(entitySet, schema.Namespace));
                }
            }

            return members;
        }

        private FieldDeclarationSyntax GenerateHttpClientField()
        {
            return FieldDeclaration(
                VariableDeclaration(
                    IdentifierName("HttpClient"))
                .AddVariables(
                    VariableDeclarator(
                        Identifier("_httpClient"))))
                .AddModifiers(
                    Token(SyntaxKind.PrivateKeyword),
                    Token(SyntaxKind.ReadOnlyKeyword));
        }

        private FieldDeclarationSyntax GenerateBaseUrlField()
        {
            return FieldDeclaration(
                VariableDeclaration(
                    PredefinedType(Token(SyntaxKind.StringKeyword)))
                .AddVariables(
                    VariableDeclarator(
                        Identifier("_baseUrl"))))
                .AddModifiers(
                    Token(SyntaxKind.PrivateKeyword),
                    Token(SyntaxKind.ReadOnlyKeyword));
        }

        private ConstructorDeclarationSyntax GenerateConstructor(string className, string defaultEndpoint)
        {
            var constructor = ConstructorDeclaration(
                Identifier(className))
                .AddModifiers(Token(SyntaxKind.PublicKeyword));

            // If default endpoint is provided, use it, otherwise require both parameters
            if (!string.IsNullOrEmpty(defaultEndpoint))
            {
                // Constructor with just httpClient, use default endpoint
                constructor = constructor
                    .AddParameterListParameters(
                        Parameter(Identifier("httpClient"))
                            .WithType(IdentifierName("HttpClient")))
                    .WithBody(Block(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("_httpClient"),
                                IdentifierName("httpClient"))),
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("_baseUrl"),
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal(defaultEndpoint))))));
            }
            else
            {
                // Constructor requiring both httpClient and baseUrl
                constructor = constructor
                    .AddParameterListParameters(
                        Parameter(Identifier("httpClient"))
                            .WithType(IdentifierName("HttpClient")),
                        Parameter(Identifier("baseUrl"))
                            .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))))
                    .WithBody(Block(
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("_httpClient"),
                                IdentifierName("httpClient"))),
                        ExpressionStatement(
                            AssignmentExpression(
                                SyntaxKind.SimpleAssignmentExpression,
                                IdentifierName("_baseUrl"),
                                IdentifierName("baseUrl")))));
            }

            return constructor;
        }

        private PropertyDeclarationSyntax GenerateEntitySetProperty(ODataEntitySet entitySet, string namespaceName)
        {
            // Extract entity type name without namespace
            var entityTypeName = entitySet.EntityType.Split('.').Last();
            var queryableType = $"ODataQueryable<{entityTypeName}>";

            // Generate property with getter that creates new ODataQueryable instance
            return PropertyDeclaration(
                ParseTypeName(queryableType),
                Identifier(entitySet.Name))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        ObjectCreationExpression(
                            ParseTypeName(queryableType))
                        .AddArgumentListArguments(
                            Argument(IdentifierName("_httpClient")),
                            Argument(IdentifierName("_baseUrl")),
                            Argument(LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                Literal(entitySet.Name))))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }
    }
}
