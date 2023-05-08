using System.Collections.Generic;

namespace MasterCsharpHosted.Shared;

public interface ICodeGenMethod : ICodeGen
{
    string ReturnType { get; set; }
    List<ParamOrProp> ParamsOrProps { get; set; }
}

public interface ICodeGenClass : ICodeGen
{
    List<CodeGenMethod> Methods { get; set; }
    List<ParamOrProp> ParamsOrProps { get; set; }
}

public interface ICodeGen
{
    string Instructions { get; set; }
    string Name { get; set; }
    string ToSystemPromptText();
}