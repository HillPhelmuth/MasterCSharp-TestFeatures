using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared;

public class CodeGenForm
{
    public string Prompt { get; set; }

    public string CodeSnippet { get; set; } = "";
}

public class CodeGenSnippet : ICodeGen
{
    [Required]
    public string Instructions { get; set; }
    public string Name { get; set; } = "";

    public string ToSystemPromptText()
    {
        return $"Generate a c# code snippet to accomplish:\n{Instructions}";
    }
}

public class CodeGenClass : ICodeGenClass
{
    [Required]
    public string Instructions { get; set; }
    [Required]
    public string Name { get; set; }
    public List<ParamOrProp> ParamsOrProps { get; set; } = new();
    public List<CodeGenMethod> Methods { get; set; } = new();
    public string ToSystemPromptText()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"create a c# class named {Name}.\nThe class should have the following public properties with both getter and setter:\n{string.Join("\n", ParamsOrProps.Select(x => $"- type: {x.Type}, name: {x.Name}"))}");
        if (Methods.Any())
        {
            sb.AppendLine("The class should have the following public methods:");
            foreach (var method in Methods)
            {
                sb.AppendLine(method.ToSystemPromptText());
            }
        }
        sb.AppendLine($"Additionall instructions for the class: {Instructions}");
        return sb.ToString();
    }
}

public class CodeGenMethod : ICodeGenMethod
{
    public CodeGenMethod()
    {
        Id = Guid.NewGuid();
    }
    [Required]
    public string Instructions { get; set; }
    [Required]
    public string Name { get; set; }
    public Guid Id { get; set; }
    public string ToSystemPromptText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Generate a c# method with the following traits:");
        sb.AppendLine($"- name: {Name}");
        sb.AppendLine($"- return type: {ReturnType}");
        sb.AppendLine($"- parameters: {string.Join(", ", ParamsOrProps.Select(x => $"{x.Type} {x.Name}"))}");
        sb.AppendLine($"- instructions: {Instructions}");
        return sb.ToString();
    }

    [Required] 
    public string ReturnType { get; set; } = "void";

    public List<ParamOrProp> ParamsOrProps { get; set; } = new();
}

public record ParamOrProp(int Id)
{
    [Required]
    public string Name { get; set; }
    [Required]
    public string Type { get; set; }
}
public enum CodeGenType
{
    Snippet,
    Class,
    Method
}