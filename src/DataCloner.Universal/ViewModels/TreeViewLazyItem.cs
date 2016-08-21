using DataCloner.Universal.ViewModels;
using System.Collections.ObjectModel;

namespace DataCloner.Universal.ViewModels
{
    public class TreeViewLazyItem : ViewModelBase, ITreeViewItem
    {
        static readonly ITreeViewItem DummyChild = new TreeViewLazyItem();

        private bool _isSelected;
        private bool _isExpanded;

        protected TreeViewLazyItem(ITreeViewItem parent, bool lazyLoadChildren)
        {
            Parent = parent;
            Children = new ObservableCollection<ITreeViewItem>();

            if (lazyLoadChildren && !HasParentDummy)
            {
                Children.Add(DummyChild);
            }
        }

        /// <summary>
        /// For dummy child
        /// </summary>
        private TreeViewLazyItem() { }
        public ITreeViewItem Parent { get; private set; }
        public ObservableCollection<ITreeViewItem> Children { get; private set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                SetProperty(ref _isSelected, value);
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                SetProperty(ref _isExpanded, value);

                //Expand parents to the root
                if (_isExpanded && Parent != null)
                    Parent.IsExpanded = true;

                //Lazy load
                if (HasDummyChild)
                {
                    Children.Clear();
                    LoadChildren();
                }
            }
        }

        private bool HasDummyChild
        {
            get { return Children.Count == 1 && Children[0] == DummyChild; }
        }

        private bool HasParentDummy
        {
            get { return Parent != null && Parent == DummyChild; }
        }

        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// </summary>
        public virtual void LoadChildren()
        {
        }
    }
}
