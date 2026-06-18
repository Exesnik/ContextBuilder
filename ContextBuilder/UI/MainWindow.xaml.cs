using ContextBuilder.Core;
using ContextBuilder.Models;
using ContextBuilder.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ContextBuilder;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly FolderPicker _folderPicker = new();
    private readonly ProjectTreeBuilder _treeBuilder = new();

    private AppSettings _settings;
    public MainWindow()
    {
        InitializeComponent();

        LoadSettings();
        UpdateRemoveButtonState();
        RebuildTree();

        Log("Application started");
    }

    // -------------------------
    // SETTINGS
    // -------------------------

    private void LoadSettings()
    {
        _settings = _settingsService.Load();

        FoldersList.Items.Clear();

        foreach (var folder in _settings.LastFolders)
        {
            FoldersList.Items.Add(new SourceFolderModel
            {
                Path = folder,
                IsSelected = false
            });
        }

        OutputBox.Text = _settings.LastOutputPath ?? string.Empty;

        CsCheck.IsChecked = _settings.Cs;
        JsonCheck.IsChecked = _settings.Json;
        MdCheck.IsChecked = _settings.Md;
        TxtCheck.IsChecked = _settings.Txt;

        TreeCheck.IsChecked = _settings.IncludeTree;

        foreach (ComboBoxItem item in FormatBox.Items)
        {
            if ((string)item.Content == _settings.Format)
            {
                FormatBox.SelectedItem = item;
                break;
            }
        }

        UpdateRemoveButtonState();
    }

    private void SaveSettings()
    {
         _settings.LastFolders =
           FoldersList.Items
            .Cast<SourceFolderModel>()
            .Select(x => x.Path)
            .ToList();

        _settings.LastOutputPath = OutputBox.Text;

        _settings.Cs = CsCheck.IsChecked == true;
        _settings.Json = JsonCheck.IsChecked == true;
        _settings.Md = MdCheck.IsChecked == true;
        _settings.Txt = TxtCheck.IsChecked == true;

        _settings.IncludeTree = TreeCheck.IsChecked == true;

        _settings.Format =
            (FormatBox.SelectedItem as ComboBoxItem)?.Content?.ToString()
            ?? "txt";

        _settingsService.Save(_settings);
    }

    // -------------------------
    // FOLDERS
    // -------------------------

    private void AddFolder_Click(object sender, RoutedEventArgs e)
    {
        var path = _folderPicker.PickFolder();

        if (string.IsNullOrWhiteSpace(path))
            return;

        if (FoldersList.Items.Cast<SourceFolderModel>().Any(x => x.Path == path))
            return;

        FoldersList.Items.Add(new SourceFolderModel
        {
            Path = path,
            IsSelected = false
        });

        Log($"Folder added: {path}");

        RebuildTree();
        SaveSettings();
    }

    private void RemoveFolder_Click(object sender, RoutedEventArgs e)
    {
        var toRemove = FoldersList.Items
            .Cast<SourceFolderModel>()
            .Where(x => x.IsSelected)
            .ToList();

        if (!toRemove.Any())
            return;

        foreach (var item in toRemove)
        {
            FoldersList.Items.Remove(item);
            Log($"Folder removed: {item.Path}");
        }

        RebuildTree();
        SaveSettings();
        UpdateRemoveButtonState();
    }

    // -------------------------
    // OUTPUT
    // -------------------------

    private void Output_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text (*.txt)|*.txt|Markdown (*.md)|*.md",
            FileName = "context"
        };

        if (dialog.ShowDialog() != true)
            return;

        OutputBox.Text = dialog.FileName;

        SaveSettings();
        Log($"Output selected: {dialog.FileName}");
    }

    // -------------------------
    // BUILD
    // -------------------------

    private void Build_Click(object sender, RoutedEventArgs e)
    {
        var folders =
            NormalizeFolders(
                FoldersList.Items
                    .Cast<SourceFolderModel>()
                    .Where(x => x.IsSelected)
                    .Select(x => x.Path)
                    .ToList());

        var extensions = GetSelectedExtensions();

        var scanner = new FileScanner();

        var seenFiles = new HashSet<string>();

        var rawFiles = scanner.Scan(folders, extensions);

        var filteredRawFiles = rawFiles
            .Where(f =>
                ProjectTree.Items
                    .Cast<TreeViewItem>()
                    .Any(root => IsNodeSelected(root, f.FullPath)))
            .ToList();

        var files = new List<ProjectFile>();

        foreach (var file in filteredRawFiles)
        {
            var fullPath = Path.GetFullPath(file.FullPath);

            if (seenFiles.Contains(fullPath))
                continue;

            seenFiles.Add(fullPath);
            files.Add(file);
        }

        var treeExporter = new ProjectTreeExporter();

        var tree = string.Join(
            Environment.NewLine,
            folders.Select(treeExporter.BuildTree));

        var exporter = new MarkdownExporter();

        var markdown = exporter.Build(files, tree);

        File.WriteAllText(OutputBox.Text, markdown);

        Log($"Exported {files.Count} files");
    }

    // -------------------------
    // DROP SUPPORT
    // -------------------------

    private void Window_DragEnter(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop)
            ? DragDropEffects.Copy
            : DragDropEffects.None;

        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            return;

        var dropped =
            (string[])e.Data.GetData(DataFormats.FileDrop);

        foreach (var path in dropped)
        {
            if (!Directory.Exists(path))
                continue;

            var normalized = Path.GetFullPath(path);

            if (FoldersList.Items.Cast<SourceFolderModel>()
                .Any(x => Path.GetFullPath(x.Path) == normalized))
                return;

            FoldersList.Items.Add(new SourceFolderModel
            {
                Path = path,
                IsSelected = false
            });

            Log($"Folder dropped: {path}");
        }

        RebuildTree();
        SaveSettings();
    }
    private bool IsNodeSelected(ItemsControl node, string filePath)
    {
        foreach (var item in node.Items)
        {
            if (item is TreeViewItem tvi)
            {
                if (tvi.Header is CheckBox cb)
                {
                    if (cb.IsChecked == true &&
                        filePath.Contains(cb.Content.ToString()))
                    {
                        return true;
                    }
                }

                if (IsNodeSelected(tvi, filePath))
                    return true;
            }
        }

        return false;
    }
    // -------------------------
    // TREE
    // -------------------------

    private void RebuildTree()
    {
        ProjectTree.Items.Clear();

        foreach (var folder in FoldersList.Items.Cast<SourceFolderModel>().Select(x => x.Path))
        {
            if (!Directory.Exists(folder))
                continue;

            try
            {
                var rootNode = _treeBuilder.Build(folder);

                ProjectTree.Items.Add(CreateTreeItem(rootNode));
            }
            catch (Exception ex)
            {
                Log($"Tree build error: {ex.Message}");
            }
        }
    }
    private void UpdateRemoveButtonState()
    {
        RemoveFolderButton.IsEnabled =
            FoldersList.Items
                .Cast<SourceFolderModel>()
                .Any(x => x.IsSelected);
    }
    private TreeViewItem CreateTreeItem(TreeNodeModel node)
    {
        var check = new CheckBox
        {
            Content = node.Name,
            IsChecked = true
        };

        var item = new TreeViewItem
        {
            Header = check
        };

        foreach (var child in node.Children)
        {
            item.Items.Add(CreateTreeItem(child));
        }

        return item;
    }

    // -------------------------
    // UTILS
    // -------------------------

    private List<string> NormalizeFolders(List<string> folders)
    {
        var fullPaths = folders
            .Select(Path.GetFullPath)
            .Distinct()
            .ToList();

        var result = new List<string>();

        foreach (var folder in fullPaths)
        {
            bool isChild = fullPaths.Any(other =>
                folder != other &&
                folder.StartsWith(other + Path.DirectorySeparatorChar));

            if (!isChild)
                result.Add(folder);
        }

        return result;
    }

    private List<string> GetSelectedExtensions()
    {
        var result = new List<string>();

        if (CsCheck.IsChecked == true)
            result.Add(".cs");

        if (JsonCheck.IsChecked == true)
            result.Add(".json");

        if (ManifestCheck.IsChecked == true)
            result.Add(".manifest");

        if (MdCheck.IsChecked == true)
            result.Add(".md");

        if (TxtCheck.IsChecked == true)
            result.Add(".txt");

        return result;
    }
    private void FolderSelectionChanged(object sender, RoutedEventArgs e)
    {
        UpdateRemoveButtonState();
    }
    private void Log(string message)
    {
        LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        LogBox.ScrollToEnd();
    }

    protected override void OnClosed(EventArgs e)
    {
        SaveSettings();
        base.OnClosed(e);
    }
}