namespace ContextBuilder.Models;

public class TreeNodeModel
{
    public string Name { get; set; }
    public bool IsSelected { get; set; } = true;
    public List<TreeNodeModel> Children { get; set; } = new();
}