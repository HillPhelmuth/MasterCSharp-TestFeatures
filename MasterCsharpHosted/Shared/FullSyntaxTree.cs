using System.Collections.Generic;

namespace MasterCsharpHosted.Shared;

public class FullSyntaxTree
{
    public string RawCode { get; set; }
    public string Kind { get; set; }
    public string Name { get; set; }
    public List<FullSyntaxTree> Members { get; set; } = new();
}