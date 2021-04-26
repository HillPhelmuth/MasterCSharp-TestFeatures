using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared
{
    public class SyntaxTreeInfo
    {
        [JsonPropertyName("SourceCode")]
        public string SourceCode { get; set; }
        [JsonPropertyName("Usings")]
        public List<string> Usings { get; set; } = new();
        [JsonPropertyName("NameSpaces")]
        public List<NameSpaceInfo> NameSpaces { get; set; } = new();
        [JsonPropertyName("Classes")]
        public List<ClassInfo> Classes { get; set; } = new();
        [JsonPropertyName("Methods")]
        public List<MethodInfo> Methods { get; set; } = new();
        [JsonPropertyName("Properties")]
        public List<PropertyInfo> Properties { get; set; } = new();

        [JsonPropertyName("GlobalDeclarations")]
        public List<GlobalDeclarationInfo> GlobalDeclarations { get; set; } = new();

    }
    public class NameSpaceInfo : SyntaxInfoBase
    {
        [JsonPropertyName("Classes")]
        public List<ClassInfo> Classes { get; set; }
    }

    public class ClassInfo : SyntaxInfoBase
    {
        [JsonPropertyName("ParentName")]
        public string ParentName { get; set; }
        [JsonPropertyName("NestedClasses")]
        public List<ClassInfo> NestedClasses { get; set; } = new();
        [JsonPropertyName("Methods")]
        public List<MethodInfo> Methods { get; set; } = new();
        [JsonPropertyName("Properties")]
        public List<PropertyInfo> Properties { get; set; } = new();
        [JsonPropertyName("Fields")]
        public List<PropertyInfo> Fields { get; set; } = new();
    }

    public class PropertyInfo : SyntaxInfoBase
    {
        [JsonPropertyName("ParentName")]
        public string ParentName { get; set; }
        [JsonPropertyName("Type")]
        public string Type { get; set; }
    }
    
    public class MethodInfo : SyntaxInfoBase
    {
        [JsonPropertyName("ParentName")]
        public string ParentName { get; set; }
        [JsonPropertyName("Body")]
        public string Body { get; set; }
        [JsonPropertyName("ReturnType")]
        public string ReturnType { get; set; }
        [JsonPropertyName("Parameters")]
        public Dictionary<string, string> Parameters { get; set; }
    }

    public class GlobalDeclarationInfo : SyntaxInfoBase
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; }
    }
    public class SyntaxInfoBase
    {
        [JsonPropertyName("RawCode")]
        public string RawCode { get; set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("RootLevel")]
        public int RootLevel { get; set; }
    }
}
