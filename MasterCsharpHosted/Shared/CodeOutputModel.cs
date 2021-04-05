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
        [JsonPropertyName("test")]
        public Test Test { get; set; }

        [JsonPropertyName("codeout")]
        public string Codeout { get; set; }

        [JsonPropertyName("testResult")]
        public bool TestResult { get; set; }

        [JsonIgnore]
        public string CssClass => TestResult ? "pass" : "fail";
    }
    public class CodeInputModel
    {
        [JsonPropertyName("solution")]
        public string Solution { get; set; }
        [JsonPropertyName("tests")]
        public List<Test> Tests { get; set; }
    }

}
