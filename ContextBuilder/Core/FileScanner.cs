using ContextBuilder.Models;
using System.IO;

namespace ContextBuilder.Core;

public class FileScanner
{
    public List<ProjectFile> Scan(
    IEnumerable<string> roots,
    IEnumerable<string> extensions)
    {
        var result = new List<ProjectFile>();

        foreach (var root in roots)
        {
            ScanFolder(root, extensions, result);
        }

        return result;
    }
    private void ScanFolder(
    string root,
    IEnumerable<string> extensions,
    List<ProjectFile> result)
    {
        foreach (var file in Directory.EnumerateFiles(
                     root,
                     "*",
                     SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file);

            if (!extensions.Contains(ext))
                continue;

            result.Add(new ProjectFile
            {
                FullPath = file,
                RelativePath = Path.GetRelativePath(root, file),
                Content = File.ReadAllText(file),
                Extension = ext
            });
        }
    }
}