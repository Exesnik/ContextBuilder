using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ContextBuilder.Models;

public class SourceFolderModel : INotifyPropertyChanged
{
    private bool _isSelected;

    public string Path { get; set; } = string.Empty;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}