namespace InjectorMicroService
{
    public interface IMarkdownInjector
    {
        string StartTag(
            string name);

        string EndTag(
            string name);

        bool InjectMarkdown(
            string targetfile,
            string tagName,
            string markdown);

        bool ContainsTag(
            string targetfile,
            string tagName);

        bool ContainsProperties(
            string targetfile);

        bool AppendTag(
            string targetfile,
            string tagName,
            string heading = "");

        string FullPathTargetFile(
            string targetfile);

        bool TargetFileExists(
            string targetfile);

        string ReadPropertyText(
            string targetfile);

        bool HasProperty(
            string propertyName,
            string targetFile);

        string PropertyValue(
            string propertyName,
            string targetFile);

        string UpdateProperty(
                    string propertyName,
                    string newValue,
                    string targetFile);
    }
}