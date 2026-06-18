using System.IO;
using System.Text;
using ContextBuilder.Models;

namespace ContextBuilder.Core;

public class ContextExporter
{
    public string Build(List<ProjectFile> files)
    {
        var sb = new StringBuilder();

        sb.AppendLine("===== CONTEXT EXPORT =====");
        sb.AppendLine();

        foreach (var file in files)
        {
            sb.AppendLine("====================================");
            sb.AppendLine($"FILE: {file.RelativePath}");
            sb.AppendLine("====================================");
            sb.AppendLine(file.Content);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public void Save(string outputPath, string content)
    {
        File.WriteAllText(outputPath, content, Encoding.UTF8);
    }
}