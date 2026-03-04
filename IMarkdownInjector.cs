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

        bool AppendTag(
            string targetfile,
            string tagName,
            string heading = "");
    }
}