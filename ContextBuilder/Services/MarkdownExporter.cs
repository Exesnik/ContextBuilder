using System.Text;
using ContextBuilder.Models;

namespace ContextBuilder.Services;

public class MarkdownExporter
{
    public string Build(
        IEnumerable<ProjectFile> files,
        string tree)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Project Structure");
        sb.AppendLine();

        sb.AppendLine("```");
        sb.AppendLine(tree);
        sb.AppendLine("```");

        sb.AppendLine();
        sb.AppendLine("# Files");
        sb.AppendLine();

        foreach (var file in files)
        {
            sb.AppendLine($"## {file.RelativePath}");
            sb.AppendLine();

            var lang = GetLanguage(file.Extension);

            sb.AppendLine($"```{lang}");
            sb.AppendLine(file.Content);
            sb.AppendLine("```");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GetLanguage(string ext)
    {
        return ext switch
        {
            ".cs" => "csharp",
            ".json" => "json",
            ".md" => "markdown",
            _ => ""
        };
    }
}