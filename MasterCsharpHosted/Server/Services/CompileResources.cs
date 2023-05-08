using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MasterCsharpHosted.Server.Services
{
    public class CompileResources
    {
        public CompileResources()
        {
            Parallel.Invoke(
                () =>
                {
                    Console.WriteLine("Invoke For Parallel 1");
                },
                () =>
                {
                    Console.WriteLine("Invoke For Parallel 2");
                });
        }
        public List<PortableExecutableReference> PortableExecutableReferences
        {
            get
            {
                _portableExecutableReferences ??= AppDomain.CurrentDomain.GetAssemblies()
                    .Where(x => x.FullName != null && !x.IsDynamic && !string.IsNullOrWhiteSpace(x.Location) &&
                                (x.FullName.Contains("System") || x.FullName.Contains("Microsoft.CodeAnalysis")))
                    .Select(assembly => MetadataReference.CreateFromFile(assembly.Location)).ToList();
                return _portableExecutableReferences;
            }
        }
        private List<PortableExecutableReference> _portableExecutableReferences;
    }
}
