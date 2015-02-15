using System.Windows;
using System.Windows.Controls;
using DataCloner.DataClasse;

namespace DataCloner.GUI
{
    public partial class MainWindow : Window
    {
        private const string FileElement = "File";
        private const string FileExtension = ".dca";
        private const string Filter = "Datacloner archive (.dca)|*.dca";

        
        private Cloner _cloner = new Cloner();

        private void cbServerSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void cbServerDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //_cloner.SaveToFile = false;
            
            //if (CbServerDestination.SelectedValue.ToString() == FileElement)
            //{
            //    var dlg = new SaveFileDialog();
            //    dlg.DefaultExt = FileExtension;
            //    dlg.Filter = Filter ;
            //    if (dlg.ShowDialog() == true)
            //    {
            //        _cloner.SaveToFile = true;
            //        _cloner.SavePath = dlg.FileName;
            //    }
            //}
        }

        private void btnExec_Click(object sender, RoutedEventArgs e)
        {
            //var selectedMap = _serversMaps.Maps.FirstOrDefault(m => m.nameFrom == cbServerSource.SelectedValue.ToString() &&
            //                                                        m.nameTo == cbServerDestination.SelectedValue.ToString());
            //cloner.ServerMap = selectedMap;
            //cloner.Initialize();

            //////Basic test : 1 row
            //var source = new RowIdentifier();
            //source.ServerId = 1;
            //source.Database = "sakila";
            //source.Schema = "";
            //source.Table = "actor";
            //source.Columns.Add("actor_id", 1);

            ////cloner.SqlTraveler(source, true, false);
        }
    }
}
