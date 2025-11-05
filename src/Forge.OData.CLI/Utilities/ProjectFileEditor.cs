using System.Xml.Linq;

namespace Forge.OData.CLI.Utilities;

public static class ProjectFileEditor
{
    public static void AddAdditionalFile(string projectPath, string fileName)
    {
        var doc = XDocument.Load(projectPath);
        var project = doc.Root ?? throw new InvalidOperationException("Invalid project file");
        
        // Check if AdditionalFiles ItemGroup exists
        var additionalFilesGroup = project.Elements("ItemGroup")
            .FirstOrDefault(g => g.Elements("AdditionalFiles").Any());

        if (additionalFilesGroup == null)
        {
            // Create new ItemGroup for AdditionalFiles
            additionalFilesGroup = new XElement("ItemGroup");
            project.Add(additionalFilesGroup);
        }

        // Check if the file is already added
        var existingFile = additionalFilesGroup.Elements("AdditionalFiles")
            .FirstOrDefault(e => e.Attribute("Include")?.Value == fileName);

        if (existingFile == null)
        {
            // Add the new AdditionalFiles entry
            additionalFilesGroup.Add(new XElement("AdditionalFiles",
                new XAttribute("Include", fileName)));
        }

        doc.Save(projectPath);
    }

    public static void AddCompileFile(string projectPath, string fileName)
    {
        var doc = XDocument.Load(projectPath);
        var project = doc.Root;

        if (project == null)
        {
            throw new InvalidOperationException("Invalid project file");
        }

        // Check if the file already exists in any Compile ItemGroup
        var existingFile = project.Elements("ItemGroup")
            .SelectMany(g => g.Elements("Compile"))
            .FirstOrDefault(e => e.Attribute("Include")?.Value == fileName);

        if (existingFile != null)
        {
            // File already exists, no need to add
            return;
        }

        // Find or create an ItemGroup for Compile items
        var compileGroup = project.Elements("ItemGroup")
            .FirstOrDefault(g => g.Elements("Compile").Any());

        if (compileGroup == null)
        {
            // Create new ItemGroup for Compile
            compileGroup = new XElement("ItemGroup");
            project.Add(compileGroup);
        }

        // Add the new Compile entry
        compileGroup.Add(new XElement("Compile",
            new XAttribute("Include", fileName)));

        doc.Save(projectPath);
    }
}
