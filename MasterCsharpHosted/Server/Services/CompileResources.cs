using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MasterCsharpHosted.Server.Services
{
    public static class CompileResources
    {
        public static List<PortableExecutableReference> PortableExecutableReferences =>
            AppDomain.CurrentDomain.GetAssemblies().Where(x =>
                !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location) &&
                (x.FullName.Contains("System"))).Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();
        public static List<PortableExecutableReference> PortableExecutableCompletionReferences =>
           AppDomain.CurrentDomain.GetAssemblies().Where(x =>
               !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location)).Select(assembly => MetadataReference.CreateFromFile(assembly.Location, documentation:DocumentationProvider.Default)).ToList();
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
