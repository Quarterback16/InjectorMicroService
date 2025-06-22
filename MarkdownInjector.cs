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
        }

        public string EndTag(string name)
        {
            return $"{StartDelimiter}/{name}{EndDelimiter}";
        }

        public string StartTag(string name)
        {
            return $"{StartDelimiter}{name}{EndDelimiter}";
        }

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
            string targetfile)
        {
            return $"{_stemFolder}{targetfile}";
        }
    }
}