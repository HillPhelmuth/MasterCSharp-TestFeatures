using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MasterCsharpHosted.Server.Services
{
    public class CompileResources
    {
        public List<PortableExecutableReference> PortableExecutableReferences => GetPortableExecutableReferences();
        public List<PortableExecutableReference> PortableExecutableCompletionReferences => GetPortableExecutableCompletionReferences();

        private List<PortableExecutableReference> portableExecutableReferences;
        private List<PortableExecutableReference> portableExecutableCompletionReferences;
        private List<PortableExecutableReference> GetPortableExecutableReferences()
        {
            portableExecutableReferences ??= AppDomain.CurrentDomain.GetAssemblies().Where(x =>
                !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location) &&
                (x.FullName.Contains("System") || x.FullName.Contains("Microsoft.CodeAnalysis"))).Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();
            return portableExecutableReferences;
        }
        private List<PortableExecutableReference> GetPortableExecutableCompletionReferences()
        {
            portableExecutableCompletionReferences ??= AppDomain.CurrentDomain.GetAssemblies().Where(x =>
               !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)).Select(assembly => MetadataReference.CreateFromFile(assembly.Location, documentation: DocumentationProvider.Default)).ToList();
            return portableExecutableCompletionReferences;
        }
    }
    
    public static class Extension
    {
        public static IEnumerable<T> Concatenate<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == null) return second;
            return second == null ? first : first.Concat(second).Distinct();
        }
    }
}
