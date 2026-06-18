using ContextBuilder.Models;
using System.IO;

namespace ContextBuilder.Core;

public class FileScanner
{
    public List<ProjectFile> Scan(ProjectConfig config)
    {
        var result = new List<ProjectFile>();

        var files = Directory.GetFiles(
            config.RootPath,
            "*.*",
            SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var ext = Path.GetExtension(file);

            if (!config.Extensions.Contains(ext))
                continue;

            if (config.ExcludedFolders.Any(f => file.Contains($"\\{f}\\")))
                continue;

            var content = File.ReadAllText(file);

            result.Add(new ProjectFile
            {
                FullPath = file,
                RelativePath = Path.GetRelativePath(config.RootPath, file),
                Content = content,
                Extension = ext
            });
        }

        return result;
    }
}