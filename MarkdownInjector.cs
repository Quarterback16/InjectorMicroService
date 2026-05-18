using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InjectorMicroService
{
    public class MarkdownInjector : IMarkdownInjector
    {
        private readonly string _stemFolder;
        internal string StartDelimiter { get; }
        internal string EndDelimiter { get; }

        public MarkdownInjector(
            string stemFolder = "c:\\developer\\Projects\\DESE-Vault\\DESE\\")
        {
            StartDelimiter = "{";
            EndDelimiter = "}";
            _stemFolder = stemFolder;
            if (!Directory.Exists(_stemFolder))
                throw new ArgumentException($"Stem folder {_stemFolder} does not exist.");
        }

        public string EndTag(string name) => 
            $"{StartDelimiter}/{name}{EndDelimiter}";

        public string StartTag(string name) =>
            $"{StartDelimiter}{name}{EndDelimiter}";

        public bool InjectMarkdown(
            string targetfile,
            string tagName,
            string markdown)
        {
            var fullTargetFile = FullFileName(targetfile);
            if (File.Exists(fullTargetFile))
            {
                var startTag = StartTag(tagName);
                var endTag = EndTag(tagName);
                var text = File.ReadAllText(fullTargetFile);
                var startTagIndex = text.IndexOf(startTag);
                var endTagIndex = text.IndexOf(endTag);
                var top = text.Substring(
                    0,
                    startTagIndex);
                var bott = text.Substring(
                    endTagIndex,
                    text.Length - endTagIndex);

                var sb = new StringBuilder();
                sb.Append(top);
                sb.Append(startTag);
                sb.AppendLine();
                sb.Append(markdown);
                sb.Append(bott);
                sb.AppendLine();

                using (StreamWriter outputFile = new StreamWriter(
                    fullTargetFile))
                {
                    outputFile.WriteLine(sb.ToString());
                }
            }
            else
                Console.WriteLine($"404: {fullTargetFile}");
            return true;
        }

        private string FullFileName(
            string targetfile) => $"{_stemFolder}{targetfile}";

        public bool ContainsTag(
            string targetfile, 
            string tagName)
        {
            var fullTargetFile = FullFileName(targetfile);
            if (File.Exists(fullTargetFile))
            {
                var startTag = StartTag(tagName);
                var endTag = EndTag(tagName);
                var text = File.ReadAllText(fullTargetFile);
                if (text.Contains(startTag) 
                    && text.Contains(endTag))
                    return true;
            }
            return false;
        }

        public bool ContainsProperties(
            string targetfile)
        {
            var fullTargetFile = FullFileName(targetfile);
            if (File.Exists(fullTargetFile))
            {
                var text = File.ReadAllText(fullTargetFile);
                if (text.Trim().StartsWith("---"))
                    return true;
            }
            return false;
        }

        public bool AppendTag(
            string targetfile, 
            string tagName,
            string heading = "")
        {
            var fullTargetFile = FullFileName(targetfile);
            if (File.Exists(fullTargetFile))
            {
                var sb = new StringBuilder()
                    .AppendLine();

                if (!string.IsNullOrEmpty(heading))
                     sb
                        .AppendLine($"## {heading}")
                        .AppendLine();
                sb.AppendLine(StartTag(tagName));
                sb.AppendLine(EndTag(tagName));

                File.AppendAllText(
                    path: fullTargetFile,
                    contents: sb.ToString(),
                    encoding: Encoding.UTF8);
                return true;
            }
            return false;
        }

        public string FullPathTargetFile(
            string targetfile) =>

            FullFileName(targetfile);

        public bool TargetFileExists(
            string targetfile) =>

            File.Exists(
                FullFileName(targetfile));

        public string ReadPropertyText(string targetfile)
        {
            var fullTargetFile = FullFileName(targetfile);
            if (File.Exists(fullTargetFile))
            {
                var text = File.ReadAllText(fullTargetFile);
                if (text.Trim().StartsWith("---"))
                {
                    var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var propertyLines = new List<string>();
                    var inProperties = false;
                    foreach (var line in lines)
                    {
                        if (line.Trim() == "---")
                        {
                            inProperties = !inProperties;
                            continue;
                        }
                        if (inProperties)
                        {
                            propertyLines.Add(line);
                        }
                    }
                    return string.Join("\n", propertyLines);
                }
            }
            return string.Empty;
        }

        public bool HasProperty(
            string propertyName, 
            string targetFile)
        {
            var fullTargetFile = FullFileName(targetFile);
            if (File.Exists(fullTargetFile))
            {
                var text = ReadPropertyText(targetFile);
                var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                return lines.Any(line => line.StartsWith($"{propertyName}:"));
            }
            return false;
        }

        public string PropertyValue(
            string propertyName,
            string targetFile)
        {
            var fullTargetFile = FullFileName(targetFile);
            if (File.Exists(fullTargetFile))
            {
                var text = ReadPropertyText(targetFile);
                var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var propertyLine = lines.FirstOrDefault(line => line.StartsWith($"{propertyName}:"));
                if (propertyLine != null)
                {
                    var parts = propertyLine.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        return parts[1].Trim();
                    }
                }
            }
            return string.Empty;
        }

        public string UpdateProperty(
            string propertyName, 
            string newValue, 
            string targetFile)
        {
            var fullTargetFile = FullFileName(targetFile);
            if (File.Exists(fullTargetFile))
            {
                if (HasProperty(propertyName, targetFile))
                {
                    var oldValue = PropertyValue(
                        propertyName,
                        targetFile);
                    var text = File.ReadAllText(fullTargetFile);
                    text = text.Replace(
                        $"{propertyName}: {oldValue}",
                        $"{propertyName}: {newValue}");
                    File.WriteAllText(fullTargetFile, text);
                    return text ?? string.Empty;
                }
                AddProperty(
                    propertyName, 
                    newValue, 
                    targetFile,
                    fullTargetFile);
            }
            return string.Empty;
        }

        private void AddProperty(
            string propertyName,
            string newValue,
            string targetFile,
            string fullTargetFile)
        {
            if (HasProperties(targetFile))
            {
                var ptext = ReadPropertyText(targetFile);
                var bodyText = File
                    .ReadAllText(fullTargetFile)
                    .Replace(Wrap(ptext), string.Empty);
                ptext = ptext.Replace("---\n", string.Empty);
                ptext = Wrap($"{ptext}\n{propertyName}: {newValue}");
                var fullText = $"{ptext}{bodyText}";
                File.WriteAllText(fullTargetFile, fullText);
            }
            else
            {
                var text = File.ReadAllText(fullTargetFile);
                var newProps = Wrap($"{propertyName}: {newValue}");
                File.WriteAllText(fullTargetFile, $"{newProps}{text}");
            }
        }

        private static string Wrap(string ptext) =>
            $"---\n{ptext}\n---\n";

        public bool HasProperties(string targetFile)
        {
            var ptext = ReadPropertyText(targetFile);
            var result = !String.IsNullOrEmpty(ptext);
            return result;
        }
        
    }
}