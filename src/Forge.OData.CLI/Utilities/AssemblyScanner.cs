using System.Reflection;
using Forge.OData.Attributes;

namespace Forge.OData.CLI.Utilities;

public class ODataClientInfo
{
    public string ClassName { get; set; } = string.Empty;
    public string MetadataFile { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
}

public static class AssemblyScanner
{
    public static List<ODataClientInfo> FindODataClients(string assemblyPath)
    {
        var clients = new List<ODataClientInfo>();
        ResolveEventHandler? resolveHandler = null;

        try
        {
            // Load the assembly
            var assembly = Assembly.LoadFrom(assemblyPath);

            // Load dependencies from the same directory
            var assemblyDir = Path.GetDirectoryName(assemblyPath)!;
            resolveHandler = (sender, args) =>
            {
                var assemblyName = new AssemblyName(args.Name);
                var dllPath = Path.Combine(assemblyDir, assemblyName.Name + ".dll");
                if (File.Exists(dllPath))
                {
                    return Assembly.LoadFrom(dllPath);
                }
                return null;
            };
            
            AppDomain.CurrentDomain.AssemblyResolve += resolveHandler;

            // Find all types with ODataClientAttribute
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<ODataClientAttribute>();
                if (attribute != null)
                {
                    clients.Add(new ODataClientInfo
                    {
                        ClassName = type.Name,
                        MetadataFile = attribute.MetadataFile,
                        Endpoint = attribute.Endpoint
                    });
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            // Some types may fail to load, but we can still process the ones that loaded
            foreach (var type in ex.Types)
            {
                if (type != null)
                {
                    var attribute = type.GetCustomAttribute<ODataClientAttribute>();
                    if (attribute != null)
                    {
                        clients.Add(new ODataClientInfo
                        {
                            ClassName = type.Name,
                            MetadataFile = attribute.MetadataFile,
                            Endpoint = attribute.Endpoint
                        });
                    }
                }
            }
        }
        finally
        {
            // Clean up the event handler
            if (resolveHandler != null)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= resolveHandler;
            }
        }

        return clients;
    }
}
