using System.Collections.Generic;

namespace MasterCsharpHosted.Shared;

public class SimpleSyntaxTree
{
    public string RawCode { get; set; }
    public string Kind { get; set; }
    public string Name { get; set; }
    public List<SimpleSyntaxTree> Members { get; set; } = new();
}

