using System.IO;
using System.Linq;
using System.Reflection;

namespace MasterCsharpHosted.Shared
{
    public static class Helpers
    {
        public static string GetJsonContentFromFile(string fileName)
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
        public static string GetStringPropValue<TItem>(this TItem item, string propName)
        {
            if (string.IsNullOrWhiteSpace(propName))
                return item.ToString();
            var property = typeof(TItem).GetProperty(propName);
            return property == null ? $"Property {propName} not found in type {nameof(item)}" : property.GetValue(item)?.ToString();
        }
    }
}