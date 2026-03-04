using System;
using System.IO;
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
    }
}