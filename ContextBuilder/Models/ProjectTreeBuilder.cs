using ContextBuilder.Models;
using System.IO;

namespace ContextBuilder.Services;

public class ProjectTreeBuilder
{
    public TreeNodeModel Build(string rootPath)
    {
        var root = new DirectoryInfo(rootPath);

        return CreateDirectoryNode(root);
    }

    private TreeNodeModel CreateDirectoryNode(DirectoryInfo dir)
    {
        var node = new TreeNodeModel
        {
            Name = dir.Name
        };

        foreach (var subDir in dir.GetDirectories())
        {
            node.Children.Add(CreateDirectoryNode(subDir));
        }

        foreach (var file in dir.GetFiles())
        {
            node.Children.Add(new TreeNodeModel
            {
                Name = file.Name
            });
        }

        return node;
    }
}