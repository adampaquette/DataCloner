using DataCloner.Universal.Facedes;
using DataCloner.Universal.ViewModels;
using System;
using Windows.UI.Xaml.Media.Imaging;

namespace DataCloner.Universal.Menu.Left
{
    public class ModelMenuItem : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public ModelMenuItem(INavigationFacade navigation): base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/web.png"));
            Children.Add(new SqlServersMenuItem1(Navigation));
        }

        public override string Label => "Modèles";
    }

    public class SqlServersMenuItem1 : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public SqlServersMenuItem1(INavigationFacade navigation) : base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/cube.png"));
            Children.Add(new SqlServersMenuItem2(Navigation));
        }

        public override string Label => "Individus";
    }
    public class SqlServersMenuItem2 : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public SqlServersMenuItem2(INavigationFacade navigation) : base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/table-1.png"));
            Children.Add(new SqlServersMenuItem3(null));
            Children.Add(new SqlServersMenuItem4(null));
            Children.Add(new SqlServersMenuItem5(null));
        }

        public override string Label => "Personne";
    }
    public class SqlServersMenuItem3 : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public SqlServersMenuItem3(INavigationFacade navigation) : base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/columns.png"));
        }

        public override string Label => "IdPersonne";
    }
    public class SqlServersMenuItem4 : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public SqlServersMenuItem4(INavigationFacade navigation) : base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/columns.png"));
        }

        public override string Label => "Nom";
    }
    public class SqlServersMenuItem5 : TreeViewMenuItemBase
    {
        private INavigationFacade _navigation;

        public SqlServersMenuItem5(INavigationFacade navigation) : base(navigation)
        {
            _navigation = navigation;
            Image = new BitmapImage(new Uri("ms-appx:///Assets/MenuIcons/columns.png"));
        }

        public override string Label => "Prenom";
    }
}
