using ContextBuilder.Models;
using ContextBuilder.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ContextBuilder;

public partial class MainWindow : Window
{
    private readonly SettingsService _settingsService = new();
    private AppSettings _settings;

    public MainWindow()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        _settings = _settingsService.Load();

        foreach (var f in _settings.LastFolders)
            FoldersList.Items.Add(f);

        OutputBox.Text = _settings.LastOutputPath;

        CsCheck.IsChecked = _settings.Cs;
        JsonCheck.IsChecked = _settings.Json;
        MdCheck.IsChecked = _settings.Md;
        TxtCheck.IsChecked = _settings.Txt;

        TreeCheck.IsChecked = _settings.IncludeTree;

        Log("Settings loaded");
    }

    private void SaveSettings()
    {
        _settings.LastFolders = FoldersList.Items.Cast<string>().ToList();
        _settings.LastOutputPath = OutputBox.Text;

        _settings.Cs = CsCheck.IsChecked == true;
        _settings.Json = JsonCheck.IsChecked == true;
        _settings.Md = MdCheck.IsChecked == true;
        _settings.Txt = TxtCheck.IsChecked == true;

        _settings.IncludeTree = TreeCheck.IsChecked == true;

        _settings.Format = (FormatBox.SelectedItem as ComboBoxItem)?.Content.ToString();

        _settingsService.Save(_settings);
    }

    private void AddFolder_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            FoldersList.Items.Add(dialog.SelectedPath);
        }
    }

    private void Output_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Text|*.txt|Markdown|*.md",
            FileName = "context"
        };

        if (dialog.ShowDialog() == true)
        {
            OutputBox.Text = dialog.FileName;
        }
    }
    private void Log(string message)
    {
        LogBox.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
        LogBox.ScrollToEnd();
    }
}