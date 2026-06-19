using ContextBuilder.Models;
using System.Text;


namespace ContextBuilder.Core;


public class ProjectTreeExporterFromNodes
{

    public string BuildTree(TreeNodeModel node)
    {

        var builder = new StringBuilder();


        BuildNode(
            node,
            builder,
            0);



        return builder.ToString();

    }



    private void BuildNode(
        TreeNodeModel node,
        StringBuilder builder,
        int depth)
    {

        if (!node.IsChecked)
            return;



        builder.AppendLine(
            $"{new string(' ', depth * 2)}{node.Name}");



        foreach (var child in node.Children)
        {

            BuildNode(
                child,
                builder,
                depth + 1);

        }

    }

}