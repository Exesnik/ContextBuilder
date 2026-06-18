using System.ComponentModel;

namespace ContextBuilder.Models;

public class SourceFolderModel : INotifyPropertyChanged
{
    public string Path { get; set; }

    private bool _isSelected;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}