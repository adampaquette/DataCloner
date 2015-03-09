using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DataCloner.DataClasse.Configuration;
using System.Linq;

namespace DataCloner.GUI
{
    public partial class MainWindow : Window
    {
        private const string FileElement = "File";
        private const string FileExtension = ".dca";
        private const string Filter = "Datacloner archive (.dca)|*.dca";

        private Cloner _cloner = new Cloner();
		private Configuration _config = Configuration.Load(Configuration.ConfigFileName);

		public MainWindow()
	    {
			

		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			cbApp.ItemsSource = _config.Applications;

			/*****************************************************************/
			var conn1 = new Connection {ConnectionString = "", Id = 1, ProviderName = "", Name = "UNI"};
			var lstConn = new List<Connection>{conn1};
			var lstDatabases = new List<string>
			{
				"Sakila",
				"DataClonerTestDatabases"
			};
			var lstSchemas = new List<string> {""};
			var lstTables = new List<string> {"table1", "table2"};

			var lstsourceCloneIdentifier = new List<sourceCloneIdentifier>
			{
				new sourceCloneIdentifier {ServerId = 1, Database = "Sakila", Schema = "", Table = "table1"}
			};

			dgcServer.ItemsSource = lstConn;
			dgcDatabase.ItemsSource = lstDatabases;
			dgcSchema.ItemsSource = lstSchemas;
			dgcTable.ItemsSource = lstTables;
			dataGrid.ItemsSource = lstsourceCloneIdentifier;


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

	    private void CbApp_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	    {
		    var app = _config?.Applications.FirstOrDefault(a => a.Id == (Int16)cbApp.SelectedValue);
		    if (app != null)
			    cbExtractionType.ItemsSource = app.ClonerConfigurations;
	    }

	    private void CbExtractionType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	    {
			var app = _config?.Applications.FirstOrDefault(a => a.Id == (Int16)cbApp.SelectedValue);
		    if (app != null)
		    {
			    var maps = app.Maps?.Where(m => m.UsableConfigs.Split(',').Contains(cbExtractionType.SelectedValue.ToString()));
				if(maps!=null)
					cbServerSource.ItemsSource = maps;
		    }
	    }

		private void cbServerSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void cbServerDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
		}
	}

	public class sourceCloneIdentifier
	{
		public Int16 ServerId { get; set; }
		public string Database { get; set; }
		public string Schema { get; set; }
		public string Table { get; set; }
	}
}
