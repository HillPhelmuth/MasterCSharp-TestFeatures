using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MasterCsharpHosted.Shared
{
    public class CustomCompletionList
    {
        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("defaultSpan")]
        public Span DefaultSpan { get; set; }

        [JsonProperty("span")]
        public Span Span { get; set; }

        [JsonProperty("rules")]
        public CustomCompletionListRules Rules { get; set; }

        [JsonProperty("suggestionModeItem")]
        public object SuggestionModeItem { get; set; }
        public static CustomCompletionList FromJson(string json) => JsonConvert.DeserializeObject<CustomCompletionList>(json, Converter.Settings);
    }

    public class Span
    {
        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }

        [JsonProperty("length")]
        public long Length { get; set; }

        [JsonProperty("isEmpty")]
        public bool IsEmpty { get; set; }
    }

    public class Item
    {
        [JsonProperty("displayText")]
        public string DisplayText { get; set; }

        [JsonProperty("displayTextPrefix")]
        public string DisplayTextPrefix { get; set; }

        [JsonProperty("displayTextSuffix")]
        public string DisplayTextSuffix { get; set; }

        [JsonProperty("filterText")]
        public string FilterText { get; set; }

        [JsonProperty("sortText")]
        public string SortText { get; set; }

        [JsonProperty("inlineDescription")]
        public string InlineDescription { get; set; }

        [JsonProperty("span")]
        public Span Span { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("rules")]
        public ItemRules Rules { get; set; }
    }

    public class Properties
    {
        [JsonProperty("InsertionText")]
        public string InsertionText { get; set; }

        [JsonProperty("SymbolKind")]
        public string SymbolKind { get; set; }

        [JsonProperty("ShouldProvideParenthesisCompletion", NullValueHandling = NullValueHandling.Ignore)]
        public string ShouldProvideParenthesisCompletion { get; set; }

        [JsonProperty("ContextPosition")]
        public string ContextPosition { get; set; }

        [JsonProperty("SymbolName")]
        public string SymbolName { get; set; }

        [JsonProperty("IsGeneric", NullValueHandling = NullValueHandling.Ignore)]
        public string IsGeneric { get; set; }
    }

    public class ItemRules
    {
        [JsonProperty("filterCharacterRules")]
        public List<CharacterRule> FilterCharacterRules { get; set; }

        [JsonProperty("commitCharacterRules")]
        public List<CharacterRule> CommitCharacterRules { get; set; }

        [JsonProperty("enterKeyRule")]
        public long EnterKeyRule { get; set; }

        [JsonProperty("formatOnCommit")]
        public bool FormatOnCommit { get; set; }

        [JsonProperty("matchPriority")]
        public long MatchPriority { get; set; }

        [JsonProperty("selectionBehavior")]
        public long SelectionBehavior { get; set; }
    }

    public class CharacterRule
    {
        [JsonProperty("kind")]
        public long Kind { get; set; }

        [JsonProperty("characters")]
        public List<string> Characters { get; set; }
    }

    public class CustomCompletionListRules
    {
        [JsonProperty("dismissIfEmpty")]
        public bool DismissIfEmpty { get; set; }

        [JsonProperty("dismissIfLastCharacterDeleted")]
        public bool DismissIfLastCharacterDeleted { get; set; }

        [JsonProperty("defaultCommitCharacters")]
        public List<string> DefaultCommitCharacters { get; set; }

        [JsonProperty("defaultEnterKeyRule")]
        public long DefaultEnterKeyRule { get; set; }

        [JsonProperty("snippetsRule")]
        public long SnippetsRule { get; set; }
    }

    public static class Serialize
    {
        public static string ToJson(this CustomCompletionList self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
