using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace InjectorMicroService
{

    public static class YamlInspector
    {
        /// <summary>
        /// Given a YAML string, extract the "tags" key as a List<string>.
        /// If YAML is malformed or "tags" is missing, returns null.
        /// </summary>
        public static List<string> ExtractPropertiesFromYaml(
            string yaml,
            string propertyName)
        {
            if (string.IsNullOrWhiteSpace(yaml))
                return new List<string>();

            try
            {
                var deserializer = new DeserializerBuilder().Build();
                var data = deserializer.Deserialize<Dictionary<string, object>>(yaml);

                if (!data.TryGetValue(propertyName, out var tagsObj))
                    return new List<string>();

                var tags = new List<string>();
                if (tagsObj is IEnumerable<object> tagList)
                {
                    foreach (var item in tagList)
                    {
                        if (item is string s)
                        {
                            tags.Add(s);
                        }
                    }
                }
                return tags;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Add a new tag to the "tags" list in the given YAML string
        /// and return the updated YAML as a string.
        /// If YAML is invalid or "tags" is not a list, return the original string.
        /// </summary>
        public static string AddTagToYaml(
            string yaml, 
            string newTag)
        {
            if (string.IsNullOrWhiteSpace(yaml))
                return yaml;

            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(
                        CamelCaseNamingConvention.Instance)
                    .Build();

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(
                        CamelCaseNamingConvention.Instance)
                    .Build();

                var data = deserializer.Deserialize<Dictionary<string, object>>(yaml);

                if (!data.TryGetValue("tags", out var tagsObj))
                {
                    // If no "tags" key, create a new list and add it.
                    data["tags"] = new List<string> { newTag };
                }
                else if (tagsObj is List<object> tagList)
                {
                    // Convert object list to strings,
                    // then back to objects.
                    var tags = new List<string>();
                    foreach (var item in tagList)
                    {
                        if (item is string s)
                            tags.Add(s);
                    }

                    // Add the new tag if it doesn't already exist.
                    if (!tags.Contains(newTag))
                        tags.Add(newTag);

                    // Rebuild as List<object> for the serializer.
                    data["tags"] = tags.ConvertAll<object>(x => x);
                }
                else
                {
                    // If "tags" exists but is not a list, don't touch YAML.
                    return yaml;
                }

                StringWriter writer = null;
                try
                {
                    writer = new StringWriter();
                    serializer.Serialize(writer, data);
                    return writer.ToString();
                }
                finally
                {
                    if (writer != null)
                        writer.Dispose();
                }
            }
            catch
            {
                // On error, return original YAML unchanged.
                return yaml;
            }
        }
    }
}
