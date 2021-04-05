using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MasterCsharpHosted.Shared
{
    public class ChallengeModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("difficulty")]
        public Difficulty Difficulty { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("examples")]
        public string Examples { get; set; }

        [JsonProperty("snippet")]
        public string Snippet { get; set; }

        [JsonProperty("solution")]
        public string Solution { get; set; }

        [JsonProperty("tests")]
        public List<Test> Tests { get; set; }

        [JsonProperty("addedBy")]
        public string AddedBy { get; set; }

        [JsonProperty("userCompleted")]
        public bool UserCompleted { get; set; }
        public static List<ChallengeModel> FromJson(string json) => JsonConvert.DeserializeObject<List<ChallengeModel>>(json, Converter.Settings);

        public static List<ChallengeModel> GetChallengesFromFile()
        {
            string challengeJson = Helpers.GetJsonContentFromFile("Challenges.json");
            return FromJson(challengeJson);
        }
    }

    public class Test
    {
        [JsonProperty("append")]
        public string Append { get; set; }

        [JsonProperty("testAgainst")]
        public string TestAgainst { get; set; }
    }
    public enum Difficulty { Easier, Easiest, Easy, Mid, Hard };
    public enum TestResult { None, Fail, Pass }
    public static class Serialize
    {
        public static string ToJson(this List<ChallengeModel> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                DifficultyConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    

    internal class DifficultyConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Difficulty) || t == typeof(Difficulty?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            return value switch
            {
                "Easier" => Difficulty.Easier,
                "Easiest" => Difficulty.Easiest,
                "Easy" => Difficulty.Easy,
                "Hard" => Difficulty.Hard,
                "Mid" => Difficulty.Mid,
                _ => throw new Exception("Cannot unmarshal type Difficulty")
            };
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Difficulty)untypedValue;
            switch (value)
            {
                case Difficulty.Easier:
                    serializer.Serialize(writer, "Easier");
                    return;
                case Difficulty.Easiest:
                    serializer.Serialize(writer, "Easiest");
                    return;
                case Difficulty.Easy:
                    serializer.Serialize(writer, "Easy");
                    return;
                case Difficulty.Hard:
                    serializer.Serialize(writer, "Hard");
                    return;
                case Difficulty.Mid:
                    serializer.Serialize(writer, "Mid");
                    return;
                default:
                    throw new Exception("Cannot marshal type Difficulty");
            }
            
        }

        public static readonly DifficultyConverter Singleton = new();
    }
}
