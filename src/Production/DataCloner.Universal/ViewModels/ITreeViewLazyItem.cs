using System.Collections.ObjectModel;

namespace DataCloner.Universal.ViewModels
{
    public interface ITreeViewLazyItem<TContent>
    {
        TContent Content { get; set; }
        ITreeViewLazyItem<TContent> Parent { get; }
        ObservableCollection<ITreeViewLazyItem<TContent>> Children { get; }

        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
    }
}
