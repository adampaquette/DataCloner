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

using DataCloner.DataClasse;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;

namespace DataCloner.GUI
{

    public partial class MainWindow : Window
    {
        private ServersMaps _serversMaps;

        public MainWindow()
        {
            InitializeComponent();

            _serversMaps = Extensions.LoadXml<ServersMaps>("serversMaps.config");
            cbServerSource.ItemsSource = _serversMaps.Maps.Select(m => m.nameFrom).Distinct().ToList(); ;
        }

        private void cbServerSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbServerDestination.ItemsSource = _serversMaps.Maps.Where(m=>m.nameFrom == cbServerSource.SelectedValue.ToString())
                                                          .Select(m => m.nameTo).Distinct().ToList();
        }

        private void btnExec_Click(object sender, RoutedEventArgs e)
        {
            var cloner = new DataCloner.Cloner();
            var selectedMap = _serversMaps.Maps.FirstOrDefault(m => m.nameFrom == cbServerSource.SelectedValue.ToString() &&
                                                                    m.nameTo == cbServerDestination.SelectedValue.ToString());
            //Map serveur source / destination
            foreach (var road in selectedMap.Roads)
            {
                cloner.ServerMap.Add(
                    new ServerIdentifier { ServerId = road.ServerSrc, Database = road.DatabaseSrc, Schema = road.SchemaSrc },
                    new ServerIdentifier { ServerId = road.ServerDst, Database = road.DatabaseDst, Schema = road.SchemaDst });               
            }

            cloner.Initialize();

            ////Basic test : 1 row
            var source = new RowIdentifier();
            source.ServerId = 1;
            source.Database = "sakila";
            source.Schema = "";
            source.Table = "actor";
            source.Columns.Add("actor_id", 1);

            cloner.SqlTraveler(source, true, false);
        }
    }
}
