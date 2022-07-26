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
        [JsonPropertyName("Enums")]
        public List<EnumInfo> Enums { get; set; } = new();
        [JsonPropertyName("MaxLevel")]
        public int MaxLevel { get; set; }

    }
    public class NameSpaceInfo : SyntaxInfoBase
    {
        [JsonPropertyName("Classes")]
        public List<ClassInfo> Classes { get; private set; } = new();
        public void AddClasses(List<ClassInfo> classes)
        {
            classes.ForEach(c => c.GroupId = this.Id);
            Classes.AddRange(classes);
        }
        [JsonPropertyName("Enums")]
        public List<EnumInfo> Enums { get; private set; } = new();
        public void AddEnums(List<EnumInfo> enums)
        {
            enums.ForEach(c => c.GroupId = this.Id);
            Enums.AddRange(enums);
        }
    }
    public class EnumInfo : SyntaxInfoBase
    {
        [JsonPropertyName("Fields")]
        public List<PropertyInfo> Fields { get; private set; } = new();
        public void AddFields(List<PropertyInfo> fields)
        {
            fields.ForEach(c => c.GroupId = this.Id);
            Fields.AddRange(fields);
        }
    }
    public class ClassInfo : SyntaxInfoBase
    {
        [JsonPropertyName("ParentName")]
        public string ParentName { get; set; }
        [JsonPropertyName("NestedClasses")]
        public List<ClassInfo> NestedClasses { get; private set; } = new();
        [JsonPropertyName("Constructors")]
        public List<MethodInfo> Constructors { get; set; } = new();
        public void AddNestedClasses(List<ClassInfo> classes)
        {
            classes.ForEach(c =>
            {
                c.GroupId = this.Id;
            });
            NestedClasses.AddRange(classes);
        }
        public void AddNestedClass(ClassInfo cls)
        {
            cls.GroupId = this.Id;
            NestedClasses.Add(cls);
        }
        [JsonPropertyName("Methods")]
        public List<MethodInfo> Methods { get; private set; } = new();
        public void AddMethods(List<MethodInfo> methods)
        {
            methods.ForEach(m => m.GroupId = this.Id);
            Methods.AddRange(methods);
        }
        [JsonPropertyName("Properties")]
        public List<PropertyInfo> Properties { get; private set; } = new();
        public void AddProperties(List<PropertyInfo> properties)
        {
            properties.ForEach(m => m.GroupId = this.Id);
            Properties.AddRange(properties);
        }
        [JsonPropertyName("Fields")]
        public List<PropertyInfo> Fields { get; private set; } = new();
        public void AddFields(List<PropertyInfo> fields)
        {
            fields.ForEach(m => m.GroupId = this.Id);
            Fields.AddRange(fields);
        }
        [JsonPropertyName("Enums")]
        public List<EnumInfo> Enums { get; private set; } = new();
        public void AddEnums(List<EnumInfo> enums)
        {
            enums.ForEach(c => c.GroupId = this.Id);
            Enums.AddRange(enums);
        }
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

        [JsonPropertyName("BodySyntax")]
        public List<GlobalDeclarationInfo> BodySyntax { get; private set; } = new();
        public void AddMethodMemebers(List<GlobalDeclarationInfo> globalDeclarations)
        {
            globalDeclarations.ForEach(m => m.GroupId = this.Id);
            BodySyntax.AddRange(globalDeclarations);
        }
    }

    public class GlobalDeclarationInfo : SyntaxInfoBase
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; }
    }
    public class SyntaxInfoBase
    {
        [JsonConstructor]
        public SyntaxInfoBase() { }
        public SyntaxInfoBase(bool placeholder = false)
        {
            Id = Guid.NewGuid().ToString();
        }
        [JsonPropertyName("Id")]
        public string Id { get; }
        
        [JsonPropertyName("RawCode")]
        public string RawCode { get; set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("RootLevel")]
        public int RootLevel { get; set; }
        [JsonPropertyName("Column")]
        public int Column { get; set; }
        [JsonPropertyName("GroupId")]
        public string GroupId { get; internal set; }
    }
}
