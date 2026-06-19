using ContextBuilder.Models;
using System.IO;


namespace ContextBuilder.Core;


public class ProjectTreeBuilder
{

    public TreeNodeModel Build(string path)
    {
        var root = new TreeNodeModel
        {
            Name = Path.GetFileName(path),
            FullPath = path,
            IsFolder = true
        };


        BuildChildren(root);


        return root;
    }



    private void BuildChildren(TreeNodeModel node)
    {

        try
        {

            foreach (var directory in Directory.GetDirectories(node.FullPath))
            {

                var child = new TreeNodeModel
                {
                    Name = Path.GetFileName(directory),
                    FullPath = directory,
                    IsFolder = true,
                    Parent = node
                };


                node.Children.Add(child);


                BuildChildren(child);

            }



            foreach (var file in Directory.GetFiles(node.FullPath))
            {

                var child = new TreeNodeModel
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    IsFolder = false,
                    Parent = node
                };


                node.Children.Add(child);

            }

        }
        catch
        {

        }

    }

}