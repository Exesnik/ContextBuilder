using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContextBuilder.Models;

public class TreeNodeModel : INotifyPropertyChanged
{
    private bool _isChecked = true;


    public string Name { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public bool IsFolder { get; set; }


    public TreeNodeModel? Parent { get; set; }


    public ObservableCollection<TreeNodeModel> Children { get; }
        = new();


    public bool IsChecked
    {
        get => _isChecked;

        set
        {
            if (_isChecked == value)
                return;


            _isChecked = value;

            OnPropertyChanged();


            foreach (var child in Children)
            {
                child.IsChecked = value;
            }
        }
    }



    public event PropertyChangedEventHandler? PropertyChanged;



    private void OnPropertyChanged(
        [CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(name));
    }
}