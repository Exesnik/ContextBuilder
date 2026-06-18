using System.IO;
using System.Text;

namespace ContextBuilder.Services;

public class ProjectTreeExporter
{
    public string BuildTree(string root)
    {
        var sb = new StringBuilder();

        var dir = new DirectoryInfo(root);

        WriteDirectory(sb, dir, "");

        return sb.ToString();
    }

    private void WriteDirectory(
        StringBuilder sb,
        DirectoryInfo dir,
        string indent)
    {
        sb.AppendLine($"{indent}{dir.Name}");

        foreach (var subDir in dir.GetDirectories())
        {
            WriteDirectory(sb, subDir, indent + "│   ");
        }

        foreach (var file in dir.GetFiles())
        {
            sb.AppendLine($"{indent}├── {file.Name}");
        }
    }
}