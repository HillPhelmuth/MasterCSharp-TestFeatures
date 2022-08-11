using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MasterCsharpHosted.Shared
{
    public class ChallengeModel : IEquatable<ChallengeModel>
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("difficulty")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Difficulty Difficulty { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("examples")]
        public string Examples { get; set; }

        [JsonPropertyName("snippet")]
        public string Snippet { get; set; }

        [JsonPropertyName("solution")]
        public string Solution { get; set; }

        [JsonPropertyName("tests")]
        public List<Test> Tests { get; set; }

        [JsonPropertyName("addedBy")]
        public string AddedBy { get; set; }

        [JsonPropertyName("userCompleted")]
        public bool UserCompleted { get; set; }
        public static List<ChallengeModel> FromJson(string json)
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };
            return JsonSerializer.Deserialize<List<ChallengeModel>>(json, options);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((ChallengeModel) obj);
        }

        public static List<ChallengeModel> GetChallengesFromFile()
        {
            string challengeJson = Helpers.GetJsonContentFromFile("Challenges.json");
            return FromJson(challengeJson);
        }

        public bool Equals(ChallengeModel other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && Description == other.Description && AddedBy == other.AddedBy;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description, AddedBy);
        }
    }

    public class Test
    {
        [JsonPropertyName("append")]
        public string Append { get; set; }

        [JsonPropertyName("testAgainst")]
        public string TestAgainst { get; set; }
    }
    public enum Difficulty { Easier, Easiest, Easy, Mid, Hard };
    public enum TestResult { None, Fail, Pass }
    
}
