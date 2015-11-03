using DataCloner.GUI.Framework;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace DataCloner.GUI.UserControls
{
    public class TreeViewLazyItemViewModel : ModelBase
    {
        static readonly TreeViewLazyItemViewModel DummyChild = new TreeViewLazyItemViewModel();

        private bool _isSelected;
        private bool _isExpanded;

        protected TreeViewLazyItemViewModel(TreeViewLazyItemViewModel parent, bool lazyLoadChildren)
        {
            Parent = parent;
            Children = new ObservableCollection<TreeViewLazyItemViewModel>();

            if (lazyLoadChildren && !HasParentDummy)
            {
                Children.Add(DummyChild);
            }
        }

        /// <summary>
        /// For dummy child
        /// </summary>
        private TreeViewLazyItemViewModel() { }

        public TreeViewLazyItemViewModel Parent { get; private set; }
        public ObservableCollection<TreeViewLazyItemViewModel> Children { get; private set; }
        public virtual ImageSource Image { get; protected set; }
        public virtual string Text { get; set; }

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
        protected virtual void LoadChildren()
        {
        }
    }
}
