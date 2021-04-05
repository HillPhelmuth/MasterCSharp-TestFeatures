using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MasterCsharpHosted.Shared
{
    public class AppHoverContent
    {
        public static AppHoverModel SimpleTypesHoverContent = new("Simple Data Types", new Dictionary<string, List<string>>{
            { "string", new List<string> { "#### *string* ####\nSIZE = *2 bytes/char*", "EXAMPLE: **s = \"reference\";** " } },
            { "int", new List<string> { "#### *integer* ####", "SIZE = 4 bytes","EXAMPLE: **val = 700;**" } },
            { "bool", new List<string> { "#### *boolean* ####", "EXAMPLE: **b = true;**" } },
            { "char", new List<string> { "#### *char* ####", "SIZE = 2 bytes "," EXAMPLE: **ch = 'a';**" } },
            { "byte", new List<string> { "#### *byte* ####", "SIZE = 1 byte "," EXAMPLE: **b = 0x78;**" } },
            { "long", new List<string> { "#### *long* ####", "SIZE = 8 bytes "," EXAMPLE: **val = 70;**" } },
            { "float", new List<string> { "#### *float* ####", "SIZE = 4 bytes "," EXAMPLE: **val = 70.0F;**"} },
            { "double", new List<string> { "#### *double* ####", "SIZE = 8 bytes "," EXAMPLE: **val = 70.0D;**"} },
            { "decimal", new List<string> { "#### *decimal* ####", "SIZE = 16 bytes ","EXAMPLE: **val = 70.0M;**" } }});

        private static List<AppHoverModel> _allAppHoverItems;

        public static List<AppHoverModel> AllAppHoverItems
        {
            get => _allAppHoverItems ?? AllAppHoverContentItems();
            private set => _allAppHoverItems = value;
        }

        public static AppHoverModel StringOperationsContent =>
            AllAppHoverContentItems().Find(x => x.Name == "String Operations");
        private static List<AppHoverModel> AllAppHoverContentItems()
        {
            string fileName = "AppHoverContentItems.json";
            string contentJson = Helpers.GetJsonContentFromFile(fileName);
            try
            {
                var allAppHovers = JsonConvert.DeserializeObject<AppHoverContentList>(contentJson);
                AllAppHoverItems = allAppHovers.AppHoverContents;
                return allAppHovers.AppHoverContents;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}\r\nStack Trace: {ex.StackTrace}\r\nInner: {ex.InnerException}");
                throw;
            }
        }

        private static string GetJsonContentFromFile(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string fileResource = assembly.GetManifestResourceNames()
                .SingleOrDefault(s => s.EndsWith(fileName));
            string contentJson = "";
            using var fileStream = assembly.GetManifestResourceStream(fileResource);
            using var reader = new StreamReader(fileStream);
            contentJson = reader.ReadToEnd();

            return contentJson;
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
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("ZIndex")]
        public int ZIndex { get; set; }

        [JsonProperty("KeyWordMessages")]
        public Dictionary<string, List<string>> KeyWordMessages { get; set; }
    }
    public class AppHoverContentList
    {
        [JsonProperty("AppHoverContents")]
        public List<AppHoverModel> AppHoverContents { get; set; }
    }
}
