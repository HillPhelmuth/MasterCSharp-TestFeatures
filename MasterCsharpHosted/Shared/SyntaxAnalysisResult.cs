using System.Collections.Generic;

namespace MasterCsharpHosted.Shared;

public class SyntaxAnalysisResult
{
    public SyntaxAnalysisResult(SyntaxTreeInfo syntaxTree, List<FullSyntaxTree> fullSyntaxTrees)
    {
        SyntaxTree = syntaxTree;
        FullSyntaxTrees = fullSyntaxTrees;
    }

    public SyntaxTreeInfo SyntaxTree { get; }
    public List<FullSyntaxTree> FullSyntaxTrees { get; }
}