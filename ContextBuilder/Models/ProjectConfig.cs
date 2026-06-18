namespace ContextBuilder.Models;

public class ProjectConfig
{
    public string RootPath { get; set; }
    public string OutputPath { get; set; }

    public string[] Extensions { get; set; } = [".cs", ".json"];
    public string[] ExcludedFolders { get; set; } =
    [
        "Library",
        "Temp",
        "obj",
        ".git",
        "Build"
    ];
}