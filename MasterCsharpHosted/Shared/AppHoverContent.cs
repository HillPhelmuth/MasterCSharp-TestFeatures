using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MasterCsharpHosted.Shared
{
    public static class AppHoverContent
    {
       
        private static List<AppHoverModel> _allAppHoverItems;

        public static List<AppHoverModel> AllAppHoverItems
        {
            get => _allAppHoverItems ?? AllAppHoverContentItems();
            private set => _allAppHoverItems = value;
        }

        private static List<AppHoverModel> AllAppHoverContentItems()
        {
            string fileName = "AppHoverContentItems.json";
            string contentJson = Helpers.GetJsonContentFromFile(fileName);
            try
            {
                var allAppHovers = JsonSerializer.Deserialize<AppHoverContentList>(contentJson);
                AllAppHoverItems = allAppHovers.AppHoverContents;
                return allAppHovers.AppHoverContents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}\r\nStack Trace: {ex.StackTrace}\r\nInner: {ex.InnerException}");
                throw;
            }
        }
    }

    public class AppHoverModel
    {
        [JsonConstructor]
        public AppHoverModel() { }
        public AppHoverModel(string name, Dictionary<string, List<string>> keyWordMessages, int zIndex = 0)
        {
            Name = name;
            ZIndex = zIndex;
            KeyWordMessages = keyWordMessages;
        }
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("ZIndex")]
        public int ZIndex { get; set; }

        [JsonPropertyName("KeyWordMessages")]
        public Dictionary<string, List<string>> KeyWordMessages { get; set; }
    }
    public class AppHoverContentList
    {
        [JsonPropertyName("AppHoverContents")]
        public List<AppHoverModel> AppHoverContents { get; set; }
    }
}
