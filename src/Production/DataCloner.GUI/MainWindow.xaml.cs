using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using DataCloner.DataClasse.Configuration;

namespace DataCloner.GUI
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            cbServerSource.ItemsSource = GetServers();
            cbServerDestination.ItemsSource = GetServers();
        }

        public List<ConnectionXml> GetServers()
        {
            return ConfigurationXml.Load("dc.config").ConnectionStrings;
        }
    }
}
