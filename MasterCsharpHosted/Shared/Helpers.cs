using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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
        public static string ToSplitCamelCase(this Enum kind)
        {
            string input = kind.ToString();
            return Regex.Replace(input, "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled).Trim();

        }
        public static object GetPropValue<TItem>(this TItem item, string propName)
        {
            if (string.IsNullOrWhiteSpace(propName))
                return item;
            var property = typeof(TItem).GetProperty(propName);
            return property == null ? $"Property {propName} not found in type {nameof(item)}" : property.GetValue(item);
        }
        public static TResult GetPropValueAs<TItem, TResult>(this TItem item, string propName)
        {
            if (string.IsNullOrWhiteSpace(propName))
                return default;
            var property = typeof(TItem).GetProperty(propName);
            if (property == null) return default;
            return (TResult)property.GetValue(item);
        }
    }
}