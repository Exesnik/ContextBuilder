namespace ContextBuilder.Models;

public class AppSettings
{
    public List<string> LastFolders { get; set; } = new();
    public string LastOutputPath { get; set; }

    public bool Cs { get; set; } = true;
    public bool Json { get; set; } = true;
    public bool Md { get; set; } = false;
    public bool Txt { get; set; } = false;

    public bool IncludeTree { get; set; } = true;

    public string Format { get; set; } = "txt";
}