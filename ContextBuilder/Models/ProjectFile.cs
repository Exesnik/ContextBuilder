namespace ContextBuilder.Models;

public class ProjectFile
{
    public string FullPath { get; set; }
    public string RelativePath { get; set; }
    public string Content { get; set; }
    public string Extension { get; set; }
}