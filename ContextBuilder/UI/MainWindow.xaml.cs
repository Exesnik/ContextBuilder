using ContextBuilder.Core;
using ContextBuilder.Models;
using ContextBuilder.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;

namespace ContextBuilder;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private readonly FolderPicker _folderPicker = new();
    private readonly ProjectTreeBuilder _treeBuilder = new();

    private readonly SelectedFilesProvider _filesProvider = new();

    private AppSettings _settings; 
    private readonly ObservableCollection<TreeNodeModel> _rootNodes = new();
    private readonly ObservableCollection<SourceFolderModel> _folders = new();
    public MainWindow()
    {
        InitializeComponent();
        FoldersList.ItemsSource = _folders;
        ProjectTree.ItemsSource = _rootNodes;
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
        _folders.Clear();

        foreach (var folder in _settings.LastFolders)
        {
            var model = new SourceFolderModel
            {
                Path = folder,
                IsSelected = false
            };

            model.PropertyChanged += (_, _) =>
            {
                UpdateRemoveButtonState();
            };

            _folders.Add(model);
        }

        OutputBox.Text = _settings.LastOutputPath ?? string.Empty;

        CsCheck.IsChecked = _settings.Cs;
        JsonCheck.IsChecked = _settings.Json;
        MdCheck.IsChecked = _settings.Md;
        TxtCheck.IsChecked = _settings.Txt;

        TreeCheck.IsChecked = _settings.IncludeTree;
        OnlyCheckedTreeRadio.IsChecked = _settings.OnlyCheckedTree;
        FullTreeRadio.IsChecked = !_settings.OnlyCheckedTree;

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
   _folders
       .Select(x => x.Path)
       .ToList();

        _settings.LastOutputPath = OutputBox.Text;

        _settings.Cs = CsCheck.IsChecked == true;
        _settings.Json = JsonCheck.IsChecked == true;
        _settings.Md = MdCheck.IsChecked == true;
        _settings.Txt = TxtCheck.IsChecked == true; 
        _settings.OnlyCheckedTree = OnlyCheckedTreeRadio.IsChecked == true;
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

        if (_folders.Any(x => x.Path == path))
            return;

        var model = new SourceFolderModel
        {
            Path = path,
            IsSelected = false
        };

        model.PropertyChanged += (_, _) =>
        {
            UpdateRemoveButtonState();
        };

        _folders.Add(model);

        Log($"Folder added: {path}");

        RebuildTree();
        SaveSettings();
    }

    private void RemoveFolder_Click(
    object sender,
    RoutedEventArgs e)
    {
        var selected =
            _folders
            .Where(x => x.IsSelected)
            .ToList();

        foreach (var item in selected)
        {
            _folders.Remove(item);

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

        var extensions =
            GetSelectedExtensions();



        var files =
            _filesProvider.GetFiles(
                _rootNodes,
                extensions);



        string tree = string.Empty;



        if (TreeCheck.IsChecked == true)
        {

            if (OnlyCheckedTreeRadio.IsChecked == true)
            {

                var exporter =
                    new ProjectTreeExporterFromNodes();


                tree = string.Join(
                    Environment.NewLine,
                    _rootNodes.Select(exporter.BuildTree));

            }
            else
            {

                var exporter =
                    new ProjectTreeExporter();


                tree = string.Join(
                    Environment.NewLine,
                    _folders.Select(x => x.Path)
                            .Select(exporter.BuildTree));

            }

        }



        var markdownExporter =
            new MarkdownExporter();



        var markdown =
            markdownExporter.Build(
                files,
                tree);



        File.WriteAllText(
            OutputBox.Text,
            markdown);



        Log(
            $"Exported {files.Count} files");

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

            if (_folders.Any(x =>
    Path.GetFullPath(x.Path) == normalized))
            {
                continue;
            }

            var model = new SourceFolderModel
            {
                Path = path,
                IsSelected = false
            };

            model.PropertyChanged += (_, _) =>
            {
                UpdateRemoveButtonState();
            };

            _folders.Add(model);

            Log($"Folder dropped: {path}");
        }

        RebuildTree();
        SaveSettings();
    }

    // -------------------------
    // TREE
    // -------------------------

    private void RebuildTree()
    {
        _rootNodes.Clear();

        foreach (var folder in _folders.Select(x => x.Path))
        {
            if (!Directory.Exists(folder))
                continue;

            try
            {
                var rootNode = _treeBuilder.Build(folder);

                _rootNodes.Add(rootNode);
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
    _folders.Any(x => x.IsSelected);
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