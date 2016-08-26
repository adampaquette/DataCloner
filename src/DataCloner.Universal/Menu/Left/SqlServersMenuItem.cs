using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Left
{
    public class SqlServersMenuItem : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public SqlServersMenuItem(INavigationFacade navigation): base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/database.png"));
            Children.Add(new SqlServersMenuItem1(Navigation));
        }

        public override string Label => "Tableau de board";
    }

    public class SqlServersMenuItem1 : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public SqlServersMenuItem1(INavigationFacade navigation) : base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/database.png"));
            Children.Add(new SqlServersMenuItem2(Navigation));
        }

        public override string Label => "Tableau de board";
    }
    public class SqlServersMenuItem2 : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public SqlServersMenuItem2(INavigationFacade navigation) : base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/database.png"));
        }

        public override string Label => "Tableau de board";
    }
}
