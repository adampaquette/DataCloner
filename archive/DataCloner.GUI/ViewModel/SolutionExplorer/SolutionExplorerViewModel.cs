using DataCloner.GUI.Framework;
using DataCloner.GUI.UserControls;
using System.Collections.ObjectModel;

namespace DataCloner.GUI.ViewModel.SolutionExplorer
{
    public class SolutionExplorerViewModel : ModelBase
    {
        private ObservableCollection<TreeViewLazyItemViewModel> _treeData;

        public SolutionExplorerViewModel()
        {
            var srv1 = new ServerTreeViewModel
            {
                Text = "Server NorthWind UNI",
                Children =
                {
                    new DatabaseTreeViewModel
                    {
                        Text = "Database1",
                        Children =
                        {
                            new SchemaTreeViewModel
                            {
                                Text = "Schema1",
                                Children =
                                {
                                    new TableTreeViewModel{Text= "Table 1"},
                                    new TableTreeViewModel{Text= "Table 2"},
                                    new TableTreeViewModel{Text= "Table 3"},
                                    new TableTreeViewModel{Text= "Table 4"}
                                }
                            },
                            new SchemaTreeViewModel
                            {
                                Text = "Schema2"
                            },
                            new SchemaTreeViewModel
                            {
                                Text = "Schema3",
                                Children =
                                {
                                    new TableTreeViewModel{Text= "Table 1"},
                                    new TableTreeViewModel{Text= "Table 2"},
                                    new TableTreeViewModel{Text= "Table 3"},
                                    new TableTreeViewModel{Text= "Table 4"}
                                }
                            }
                        }
                    },
                    new DatabaseTreeViewModel
                    {
                        Text = "Database2",
                        Children =
                        {
                            new SchemaTreeViewModel
                            {
                                Text = "Schema1"
                            }
                        }
                    }
                }
            };
            var srv2 = new ServerTreeViewModel { Text = "Server NorthWind FON" };
            var srv3 = new ServerTreeViewModel { Text = "Server NorthWind ACC" };
            var srv4 = new ServerTreeViewModel { Text = "Server NorthWind PROD" };

            var project = new ProjectTreeViewModel { Text = "Project Northwind" };
            project.Children.Add(srv1);
            project.Children.Add(srv2);
            project.Children.Add(srv3);
            project.Children.Add(srv4);

            _treeData = new ObservableCollection<TreeViewLazyItemViewModel>();
            _treeData.Add(project);
        }

        public ObservableCollection<TreeViewLazyItemViewModel> TreeData
        {
            get { return _treeData; }
            set { SetProperty(ref _treeData, value); }
        }
    }
}
