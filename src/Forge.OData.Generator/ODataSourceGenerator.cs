using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Forge.OData.Generator
{
    [Generator]
    public class ODataSourceGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Look for classes with ODataClientAttribute
            var classesWithAttribute = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => IsCandidateClass(s),
                    transform: static (ctx, _) => GetClassWithAttribute(ctx))
                .Where(static m => m is not null);

            // Collect metadata filenames referenced by attributes
            var attributedMetadataFiles = classesWithAttribute
                .Select((classInfo, _) => classInfo.HasValue ? classInfo.Value.MetadataFile : null)
                .Where(fileName => fileName != null)
                .Collect();

            // Filter additional files to only include .xml files
            var xmlFiles = context.AdditionalTextsProvider
                .Where(file => Path.GetExtension(file.Path).Equals(".xml", StringComparison.OrdinalIgnoreCase));

            // Filter further to only OData metadata files
            var odataMetadataFiles = xmlFiles
                .Select((file, cancellationToken) =>
                {
                    var text = file.GetText(cancellationToken);
                    if (text == null) return default((AdditionalText, string)?);

                    var xmlContent = text.ToString();
                    if (!IsODataMetadata(xmlContent))
                        return default((AdditionalText, string)?);

                    return (file, xmlContent);
                })
                .Where(result => result.HasValue)
                .Select((result, _) => result!.Value);

            // Combine with attributed metadata files to filter them out
            var filteredMetadataFiles = odataMetadataFiles
                .Combine(attributedMetadataFiles)
                .Select((combined, _) =>
                {
                    var ((file, xmlContent), referencedFiles) = combined;
                    var fileName = Path.GetFileName(file.Path);
                    
                    // Skip this file if it's referenced by an attributed class
                    if (referencedFiles.Any(rf => 
                        string.Equals(rf, fileName, StringComparison.OrdinalIgnoreCase)))
                    {
                        return default((AdditionalText, string)?);
                    }
                    
                    return (file, xmlContent);
                })
                .Where(result => result.HasValue)
                .Select((result, _) => result!.Value);

            // Generate source code from metadata files not referenced by attributes
            context.RegisterSourceOutput(filteredMetadataFiles, (spc, source) =>
            {
                try
                {
                    var (file, xmlContent) = source;
                    
                    var parser = new ODataMetadataParser();
                    var schema = parser.Parse(xmlContent);

                    var generator = new ModelGenerator();
                    var models = generator.GenerateModels(schema);

                    foreach (var model in models)
                    {
                        var sourceText = SourceText.From(model.Value, Encoding.UTF8);
                        spc.AddSource(model.Key, sourceText);
                    }

                    // Generate JSON converters
                    var converterGenerator = new JsonConverterGenerator();
                    var converters = converterGenerator.GenerateConverters(schema);

                    foreach (var converter in converters)
                    {
                        var sourceText = SourceText.From(converter.Value, Encoding.UTF8);
                        spc.AddSource(converter.Key, sourceText);
                    }

                    // Generate client with name based on XML filename
                    var fileName = Path.GetFileNameWithoutExtension(file.Path);
                    var clientClassName = GenerateClientClassName(fileName);
                    
                    var clientGenerator = new ClientGenerator();
                    var client = clientGenerator.GenerateClient(schema, clientClassName);
                    spc.AddSource($"{clientClassName}.g.cs", SourceText.From(client, Encoding.UTF8));
                }
                catch (Exception ex)
                {
                    // Report diagnostic for any errors
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "ODATA001",
                            "OData Generator Error",
                            "Error generating OData client: {0}",
                            "ODataGenerator",
                            DiagnosticSeverity.Error,
                            true),
                        Location.None,
                        ex.Message));
                }
            });

            // Combine with additional files to get metadata content
            var attributeWithMetadata = classesWithAttribute
                .Combine(context.AdditionalTextsProvider.Collect());

            // Generate partial classes from attributes
            context.RegisterSourceOutput(attributeWithMetadata, (spc, source) =>
            {
                try
                {
                    var (classInfo, additionalFiles) = source;
                    if (classInfo == null) return;

                    GenerateFromAttribute(spc, classInfo.Value, additionalFiles);
                }
                catch (Exception ex)
                {
                    spc.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "ODATA002",
                            "OData Attribute Generator Error",
                            "Error generating OData client from attribute: {0}",
                            "ODataGenerator",
                            DiagnosticSeverity.Error,
                            true),
                        Location.None,
                        ex.Message));
                }
            });
        }

        private bool IsODataMetadata(string xmlContent)
        {
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var root = doc.Root;
                
                // Check for OData/EDM namespace
                return root != null && 
                       (root.Name.LocalName == "Edmx" || 
                        root.Name.LocalName == "Schema" ||
                        root.Name.NamespaceName.Contains("edm") ||
                        root.Name.NamespaceName.Contains("odata"));
            }
            catch
            {
                return false;
            }
        }

        private static bool IsCandidateClass(SyntaxNode node)
        {
            return node is Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl
                && classDecl.AttributeLists.Count > 0;
        }

        private static (string ClassName, string Namespace, string MetadataFile, string Endpoint)? GetClassWithAttribute(GeneratorSyntaxContext context)
        {
            var classDecl = (Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax)context.Node;
            
            foreach (var attributeList in classDecl.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var symbol = context.SemanticModel.GetSymbolInfo(attribute).Symbol;
                    if (symbol is not IMethodSymbol attributeSymbol)
                        continue;

                    var attributeClass = attributeSymbol.ContainingType;
                    var fullName = attributeClass.ToDisplayString();

                    if (fullName == "Forge.OData.Attributes.ODataClientAttribute")
                    {
                        var className = classDecl.Identifier.Text;
                        var namespaceName = GetNamespace(classDecl);
                        
                        // Extract attribute arguments
                        string metadataFile = "";
                        string endpoint = "";

                        if (attribute.ArgumentList != null)
                        {
                            foreach (var arg in attribute.ArgumentList.Arguments)
                            {
                                var argName = arg.NameEquals?.Name.Identifier.Text;
                                var argValue = context.SemanticModel.GetConstantValue(arg.Expression);

                                if (argName == "MetadataFile" && argValue.HasValue)
                                {
                                    metadataFile = argValue.Value?.ToString() ?? "";
                                }
                                else if (argName == "Endpoint" && argValue.HasValue)
                                {
                                    endpoint = argValue.Value?.ToString() ?? "";
                                }
                            }
                        }

                        return (className, namespaceName, metadataFile, endpoint);
                    }
                }
            }

            return null;
        }

        private static string GetNamespace(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax classDecl)
        {
            var namespaceDecl = classDecl.Parent as Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax;
            if (namespaceDecl != null)
                return namespaceDecl.Name.ToString();

            var fileScopedNamespace = classDecl.Parent as Microsoft.CodeAnalysis.CSharp.Syntax.FileScopedNamespaceDeclarationSyntax;
            if (fileScopedNamespace != null)
                return fileScopedNamespace.Name.ToString();

            return "Global";
        }

        private void GenerateFromAttribute(
            SourceProductionContext context, 
            (string ClassName, string Namespace, string MetadataFile, string Endpoint) classInfo,
            ImmutableArray<AdditionalText> additionalFiles)
        {
            // Find the metadata file
            var metadataFile = additionalFiles.FirstOrDefault(f => 
                Path.GetFileName(f.Path).Equals(classInfo.MetadataFile, StringComparison.OrdinalIgnoreCase));

            if (metadataFile == null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "ODATA003",
                        "Metadata File Not Found",
                        "Metadata file '{0}' not found in additional files",
                        "ODataGenerator",
                        DiagnosticSeverity.Error,
                        true),
                    Location.None,
                    classInfo.MetadataFile));
                return;
            }

            var xmlContent = metadataFile.GetText()?.ToString();
            if (xmlContent == null)
                return;

            // Parse metadata
            var parser = new ODataMetadataParser();
            var schema = parser.Parse(xmlContent);

            // Override namespace with the class's namespace
            var originalNamespace = schema.Namespace;
            schema.Namespace = classInfo.Namespace;

            // Generate models in the same namespace
            var generator = new ModelGenerator();
            var models = generator.GenerateModels(schema);

            foreach (var model in models)
            {
                var sourceText = SourceText.From(model.Value, Encoding.UTF8);
                context.AddSource($"{classInfo.ClassName}_{model.Key}", sourceText);
            }

            // Generate JSON converters
            var converterGenerator = new JsonConverterGenerator();
            var converters = converterGenerator.GenerateConverters(schema);

            foreach (var converter in converters)
            {
                var sourceText = SourceText.From(converter.Value, Encoding.UTF8);
                context.AddSource($"{classInfo.ClassName}_{converter.Key}", sourceText);
            }

            // Generate partial class with client functionality
            var partialClientGenerator = new PartialClientGenerator();
            var partialClient = partialClientGenerator.GeneratePartialClient(
                schema, 
                classInfo.ClassName, 
                classInfo.Namespace,
                classInfo.Endpoint);
            context.AddSource($"{classInfo.ClassName}.g.cs", SourceText.From(partialClient, Encoding.UTF8));
        }

        /// <summary>
        /// Generates a valid C# class name from a filename by:
        /// 1. Removing the extension
        /// 2. Keeping only valid C# identifier characters (letters, digits, underscore)
        /// 3. Prefixing with underscore if name starts with a digit
        /// 4. Appending "Client" suffix
        /// </summary>
        private static string GenerateClientClassName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "ODataClient";

            // Keep only valid characters for C# identifiers
            // Valid characters: letters, digits, underscore
            // Must start with letter or underscore (digits are prefixed with underscore)
            var sb = new StringBuilder();
            
            foreach (char c in fileName)
            {
                // Keep only valid C# identifier characters
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }

            var baseName = sb.ToString();
            
            // Ensure it starts with a letter or underscore
            if (baseName.Length > 0 && char.IsDigit(baseName[0]))
            {
                baseName = "_" + baseName;
            }

            // If the result is empty or invalid, use default
            if (string.IsNullOrWhiteSpace(baseName))
            {
                return "ODataClient";
            }

            // Append "Client" suffix
            return baseName + "Client";
        }
    }
}
