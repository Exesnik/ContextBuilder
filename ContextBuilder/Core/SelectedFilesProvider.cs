using ContextBuilder.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace ContextBuilder.Core;


public class SelectedFilesProvider
{

    public List<ProjectFile> GetFiles(
        IEnumerable<TreeNodeModel> roots,
        IEnumerable<string> extensions)
    {

        var result = new List<ProjectFile>();

        var seen = new HashSet<string>();


        foreach (var root in roots)
        {
            Collect(
                root,
                extensions,
                result,
                seen);
        }


        return result;

    }



    private void Collect(
        TreeNodeModel node,
        IEnumerable<string> extensions,
        List<ProjectFile> result,
        HashSet<string> seen)
    {


        if (!node.IsChecked)
            return;



        if (!node.IsFolder)
        {

            var extension =
                Path.GetExtension(node.FullPath);



            if (!extensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase))
            {
                return;
            }



            var fullPath =
                Path.GetFullPath(node.FullPath);



            if (!seen.Add(fullPath))
                return;



            result.Add(new ProjectFile
            {
                FullPath = fullPath,

                RelativePath =
                    Path.GetFileName(fullPath),

                Content =
                    File.ReadAllText(fullPath)
            });



            return;

        }



        foreach (var child in node.Children)
        {
            Collect(
                child,
                extensions,
                result,
                seen);
        }

    }

}