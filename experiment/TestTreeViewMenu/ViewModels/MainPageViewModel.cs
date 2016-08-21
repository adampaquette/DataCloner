using System.Collections.ObjectModel;

namespace TestTreeViewMenu.ViewModels
{
    public class MainPageViewModel
    {
        public ObservableCollection<MenuItem> MenuItems { get; set; }

        public MainPageViewModel()
        {
            MenuItems = new ObservableCollection<MenuItem>()
            {
                new MenuItem()
                {
                    Text = "MY ACCOUNT",
                    Children = new ObservableCollection<MenuItem>
                    {
                        new MenuItem{ Text = "Overview============================================" },
                        new MenuItem{ Text = "History" },
                        new MenuItem{ Text = "Profile" }
                    }
                },
                new MenuItem()
                {
                    Text = "SOLUTION EXPLORER",
                    Children = new ObservableCollection<MenuItem>
                    {
                        new MenuItem
                        {
                            Text = "Project",
                            Children = new ObservableCollection<MenuItem>
                            {
                                new MenuItem { Text = "ViewModels", Children = new ObservableCollection<MenuItem>() { new MenuItem { Text = "MainPageViewModel.cs" } } },
                                new MenuItem { Text = "App.xaml", Children = new ObservableCollection<MenuItem>() { new MenuItem { Text = "App.xaml.cs" } } }
                            }
                        }
                    }
                }
            };
        }

    }
}
