using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DataCloner.DataClasse.Configuration;
using System.Windows.Media;
using System.Linq;
using System.Text;
using DataCloner.DataClasse;

namespace DataCloner.GUI
{
	public partial class MainWindow : Window
	{
		private const string FileElement = "File";
		private const string FileExtension = ".dca";
		private const string Filter = "Datacloner archive (.dca)|*.dca";

		private Cloner _cloner = new Cloner();
		private Configuration _config = Configuration.Load(Configuration.ConfigFileName);
		private DataCloner.DataClasse.Configuration.Application _selectedApp;
		private IEnumerable<Map> _maps;
		private Map _fromMaps;

		private Int16 _selectedServer;
		private string _selectedDatabase;
		private string _selectedSchema;
		private string _selectedTable;
		private string _selectedColumn;

		private List<Int16> Servers = null;

		public MainWindow()
		{
			_cloner.EnforceIntegrity = false;
			_cloner.StatusChanged += OnStatusChanged;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			cbApp.ItemsSource = _config.Applications;
			if (_config.Applications.Count == 1)
				cbApp.SelectedIndex = 0;
		}

		private static void OnStatusChanged(object s, StatusChangedEventArgs e)
		{
			if (e.Status == Status.Cloning)
			{
				var sb = new StringBuilder();
				sb.Append(new string(' ', 3 * e.Level));
				sb.Append(e.SourceRow.Database).Append(".").Append(e.SourceRow.Schema).Append(".").Append(e.SourceRow.Table).Append(" : (");
				foreach (var col in e.SourceRow.Columns)
					sb.Append(col.Key).Append("=").Append(col.Value).Append(", ");
				sb.Remove(sb.Length - 2, 2);
				sb.Append(")");
				Console.WriteLine(sb.ToString());
			}
			else if (e.Status == Status.FetchingDerivatives)
			{
				Console.WriteLine(new string(' ', 3 * e.Level) + "=================================");
			}
		}

		private void CbApp_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedApp = _config?.Applications?.FirstOrDefault(a => a.Id == (Int16)cbApp.SelectedValue);
			if (_selectedApp != null)
			{
				cbExtractionType.ItemsSource = _selectedApp.ClonerConfigurations;
				if (_selectedApp.ClonerConfigurations.Count == 1)
					cbExtractionType.SelectedIndex = 0;
			}
		}

		private void CbExtractionType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_maps = _selectedApp?.Maps?.Where(m => m.UsableConfigs.Split(',').Contains(cbExtractionType.SelectedValue.ToString()));
			if (_maps != null)
			{
				cbSource.ItemsSource = _maps;
				if (_maps.Count() == 1)
					cbSource.SelectedIndex = 0;
			}
		}

		private void cbSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_fromMaps = _maps?.FirstOrDefault(m => m.From == cbSource.SelectedValue.ToString());
			if (_fromMaps != null)
			{
				var _mapsTo = _maps.Where(m =>
						m.From == _fromMaps.From &&
						m.UsableConfigs.Split(',').Contains(cbExtractionType.SelectedValue.ToString()));
				cbDestination.ItemsSource = _mapsTo;

				if (_mapsTo.Count() == 1)
					cbDestination.SelectedIndex = 0;
			}
		}

		private void cbDestination_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var map = _maps?.FirstOrDefault(m => m.From == cbSource.SelectedValue.ToString() &&
											   m.To == cbDestination.SelectedValue.ToString() &&
											   m.UsableConfigs.Split(',').Contains(cbExtractionType.SelectedValue.ToString()));
			if (map != null)
			{
				var configId = (Int16)cbExtractionType.SelectedValue;
				_cloner.Setup(_selectedApp, map.Id, configId);
				Servers = _cloner._cache.DatabasesSchema._dic.Keys.ToArray().ToList();

				cbServer.ItemsSource = Servers;
				if (Servers.Count == 1)
					cbServer.SelectedIndex = 0;
			}
		}

		private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedServer = (Int16)cbServer.SelectedValue;
			var databases = _cloner._cache.DatabasesSchema._dic[_selectedServer].Keys.ToArray().ToList();

			cbDatabase.ItemsSource = databases;
			if (databases.Count == 1)
				cbDatabase.SelectedIndex = 0;
		}

		private void cbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedDatabase = cbDatabase.SelectedValue.ToString();
			var schemas = _cloner._cache.DatabasesSchema._dic[_selectedServer][_selectedDatabase].Keys.ToArray().ToList();

			cbSchema.ItemsSource = schemas;
			if (schemas.Count == 1)
				cbSchema.SelectedIndex = 0;
		}

		private void cbSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedSchema = cbSchema.SelectedValue.ToString();
			var tables = _cloner._cache.DatabasesSchema._dic[_selectedServer][_selectedDatabase][_selectedSchema].Select(t => t.Name).ToList();
				
			cbTable.ItemsSource = tables;
			if (tables.Count == 1)
				cbTable.SelectedIndex = 0;
		}

		private void cbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedTable = cbTable.SelectedValue.ToString();
			var columns = _cloner._cache.DatabasesSchema._dic[_selectedServer][_selectedDatabase][_selectedSchema].FirstOrDefault(t => t.Name == _selectedTable)?.ColumnsDefinition.Select(c => c.Name).ToList();

			cbColonne.ItemsSource = columns;
			if (columns.Count == 1)
				cbColonne.SelectedIndex = 0;
		}

		private void cbColonne_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_selectedColumn = cbColonne.SelectedValue.ToString();
		}

		private void btnExec_Click(object sender, RoutedEventArgs e)
		{
			if (_selectedColumn != null &&
				txtValeur.Text != null)
			{

				var source = new RowIdentifier();
				source.ServerId = _selectedServer;
				source.Database = _selectedDatabase;
				source.Schema = _selectedSchema;
				source.Table = _selectedTable;
				source.Columns.Add(_selectedColumn, txtValeur.Text);

				_cloner.Clone(source, true);
			}
		}
	}
}
