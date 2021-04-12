using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MasterCsharpHosted.Shared
{
    public class CustomSuggestion
    {
        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("insertText")]
        public string InsertText { get; set; }

        [JsonPropertyName("kind")]
        public string Kind { get; set; }

        [JsonPropertyName("detail")]
        public string Detail { get; set; }

        [JsonPropertyName("documentation")]
        public string Documentation { get; set; }
    }

    public class CustomSuggestionList
    {
        [JsonPropertyName("items")]
        public List<CustomSuggestion> Items { get; set; }
    }
}
