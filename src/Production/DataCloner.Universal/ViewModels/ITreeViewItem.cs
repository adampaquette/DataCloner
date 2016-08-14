using System.Collections.ObjectModel;

namespace DataCloner.Universal.ViewModels
{
    /// <summary>
    /// Define fonctionalities for a tree view item.
    /// </summary>
    public interface ITreeViewItem
    {
        ITreeViewItem Parent { get; }
        ObservableCollection<ITreeViewItem> Children { get; }

        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }
}
