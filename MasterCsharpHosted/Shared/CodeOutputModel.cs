using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MasterCsharpHosted.Shared
{
    public class CodeOutputModel
    {
        [JsonPropertyName("outputs")]
        public List<Output> Outputs { get; set; }
    }
    public class Output
    {
        [JsonPropertyName("testIndex")]
        public int TestIndex { get; set; }
        
        [JsonPropertyName("codeout")]
        public string Codeout { get; set; }

        [JsonPropertyName("testResult")]
        public bool TestResult { get; set; }
        [JsonIgnore]
        public string CssClass { get; set; }
    }
    public class CodeInputModel
    {
        [JsonPropertyName("solution")]
        public string Solution { get; set; }
        
    }

}
