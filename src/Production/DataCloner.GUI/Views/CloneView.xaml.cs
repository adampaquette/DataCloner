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
using System.ComponentModel;
using DataCloner.GUI.Properties;
using DataCloner.DataClasse.Configuration;
using System.Windows.Threading;
using System.Threading;

namespace DataCloner.GUI.Views
{
    /// <summary>
    /// Interaction logic for CloneView.xaml
    /// </summary>
    public partial class CloneView : UserControl
    {
        private const string FileElement = "File";
        private const string FileExtension = ".dca";
        private const string Filter = "Datacloner archive (.dca)|*.dca";

        private Cloner _cloner = new Cloner();
        private BackgroundWorker _cloneWorker;
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

        public CloneView()
        {
            InitCloner();
            InitClonerWorker();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbApp.ItemsSource = _config.Applications;

            //Tente de charger la préférence utilisateur
            var app = _config.Applications.FirstOrDefault(a => a.Id == Settings.Default.ApplicationId);
            if (app != null)
                cbApp.SelectedItem = app;
            else
            {
                if (_config.Applications.Count == 1)
                    cbApp.SelectedIndex = 0;
            }
        }

        private void CbApp_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedApp = _config?.Applications?.FirstOrDefault(a => a.Id == (Int16)cbApp.SelectedValue);
            if (_selectedApp != null)
            {
                Settings.Default.ApplicationId = _selectedApp.Id;
                Settings.Default.Save();

                cbDatabaseConfig.ItemsSource = _selectedApp.ClonerConfigurations;

                //Tente de charger la préférence utilisateur
                var config = _selectedApp.ClonerConfigurations.FirstOrDefault(c => c.Id == Settings.Default.DatabaseConfigId);
                if (config != null)
                    cbDatabaseConfig.SelectedItem = config;
                else
                {
                    if (_selectedApp.ClonerConfigurations.Count == 1)
                        cbDatabaseConfig.SelectedIndex = 0;
                }
            }
        }

        private void CbDatabaseConfig_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _maps = _selectedApp?.Maps?.Where(m => m.UsableConfigs.Split(',').Contains(cbDatabaseConfig.SelectedValue.ToString()));
            if (_maps != null)
            {
                Settings.Default.DatabaseConfigId = (Int16)cbDatabaseConfig.SelectedValue;
                Settings.Default.Save();

                cbSourceEnvir.ItemsSource = _maps;

                //Tente de charger la préférence utilisateur
                _fromMaps = _maps.FirstOrDefault(m => m.From == Settings.Default.SourceEnvir);
                if (_fromMaps != null)
                    cbSourceEnvir.SelectedItem = _fromMaps;
                else
                {
                    if (_maps.Count() == 1)
                        cbSourceEnvir.SelectedIndex = 0;
                }
            }
        }

        private void cbSourceEnvir_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _fromMaps = _maps?.FirstOrDefault(m => m.From == cbSourceEnvir.SelectedValue.ToString());
            if (_fromMaps != null)
            {
                Settings.Default.SourceEnvir = cbSourceEnvir.SelectedValue.ToString();
                Settings.Default.Save();

                var _mapsTo = _maps.Where(m =>
                        m.From == _fromMaps.From &&
                        m.UsableConfigs.Split(',').Contains(cbDatabaseConfig.SelectedValue.ToString()));
                cbDestinationEnvir.ItemsSource = _mapsTo;

                //Tente de charger la préférence utilisateur
                var map = _maps?.FirstOrDefault(m => m.From == cbSourceEnvir.SelectedValue.ToString() &&
                                               m.To == Settings.Default.DestinationEnvir &&
                                               m.UsableConfigs.Split(',').Contains(cbDatabaseConfig.SelectedValue.ToString()));
                if (map != null)
                    cbDestinationEnvir.SelectedItem = map;
                else
                {
                    if (_mapsTo.Count() == 1)
                        cbDestinationEnvir.SelectedIndex = 0;
                }
            }
        }

        private void cbDestinationEnvir_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var map = _maps?.FirstOrDefault(m => m.From == cbSourceEnvir.SelectedValue.ToString() &&
                                               m.To == cbDestinationEnvir.SelectedValue.ToString() &&
                                               m.UsableConfigs.Split(',').Contains(cbDatabaseConfig.SelectedValue.ToString()));
            if (map != null)
            {
                Settings.Default.DestinationEnvir = cbDestinationEnvir.SelectedValue.ToString();
                Settings.Default.Save();

                var configId = (Int16)cbDatabaseConfig.SelectedValue;
                _cloner.Setup(_selectedApp, map.Id, configId);
                Servers = _cloner._cache.DatabasesSchema.Keys.ToArray().ToList();
                cbServer.ItemsSource = Servers;

                //Tente de charger la préférence utilisateur
                if (Servers.Contains(Settings.Default.ServerSource))
                    cbServer.SelectedItem = Settings.Default.ServerSource;
                else
                {
                    if (Servers.Count == 1)
                        cbServer.SelectedIndex = 0;
                }
            }
        }

        private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbServer.SelectedIndex != -1)
            {
                _selectedServer = (Int16)cbServer.SelectedValue;

                Settings.Default.ServerSource = _selectedServer;
                Settings.Default.Save();

                var databases = _cloner._cache.DatabasesSchema[_selectedServer].Keys.ToArray().ToList();
                cbDatabase.ItemsSource = databases;

                //Tente de charger la préférence utilisateur
                if (databases.Contains(Settings.Default.DatabaseSource))
                    cbDatabase.SelectedItem = Settings.Default.DatabaseSource;
                else
                {
                    if (databases.Count == 1)
                        cbDatabase.SelectedIndex = 0;
                }
            }
            else
                cbDatabase.SelectedIndex = -1;

            cbDatabase_SelectionChanged(null, null);
        }

        private void cbDatabase_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDatabase.SelectedIndex != -1)
            {
                _selectedDatabase = cbDatabase.SelectedValue.ToString();

                Settings.Default.DatabaseSource = _selectedDatabase;
                Settings.Default.Save();

                var schemas = _cloner._cache.DatabasesSchema[_selectedServer][_selectedDatabase].Keys.ToArray().ToList();
                cbSchema.ItemsSource = schemas;

                //Tente de charger la préférence utilisateur
                if (schemas.Contains(Settings.Default.SchemaSource))
                    cbSchema.SelectedItem = Settings.Default.SchemaSource;
                else
                {
                    if (schemas.Count == 1)
                        cbSchema.SelectedIndex = 0;
                }
            }
            else
                cbSchema.SelectedIndex = -1;

            cbSchema_SelectionChanged(null, null);
        }

        private void cbSchema_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbSchema.SelectedIndex != -1)
            {
                _selectedSchema = cbSchema.SelectedValue.ToString();

                Settings.Default.SchemaSource = _selectedSchema;
                Settings.Default.Save();

                var tables = _cloner._cache.DatabasesSchema[_selectedServer][_selectedDatabase][_selectedSchema].Select(t => t.Name).ToList();
                cbTable.ItemsSource = tables;

                //Tente de charger la préférence utilisateur
                if (tables.Contains(Settings.Default.TableSource))
                    cbTable.SelectedItem = Settings.Default.TableSource;
                else
                {
                    if (tables.Count == 1)
                        cbTable.SelectedIndex = 0;
                }
            }
            else
                cbTable.SelectedIndex = -1;

            cbTable_SelectionChanged(null, null);
        }

        private void cbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTable.SelectedIndex != -1)
            {
                _selectedTable = cbTable.SelectedValue.ToString();

                Settings.Default.TableSource = _selectedTable;
                Settings.Default.Save();

                var columns = _cloner._cache.DatabasesSchema[_selectedServer][_selectedDatabase][_selectedSchema].FirstOrDefault(t => t.Name == _selectedTable)?.ColumnsDefinition.Select(c => c.Name).ToList();
                cbColonne.ItemsSource = columns;

                //Tente de charger la préférence utilisateur
                if (columns.Contains(Settings.Default.ColumnSource))
                    cbColonne.SelectedItem = Settings.Default.ColumnSource;
                else
                {
                    if (columns.Count == 1)
                        cbColonne.SelectedIndex = 0;
                }
            }
            else
                cbColonne.SelectedIndex = -1;

            cbColonne_SelectionChanged(null, null);
        }

        private void cbColonne_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbColonne.SelectedIndex != -1)
            {
                _selectedColumn = cbColonne.SelectedValue.ToString();

                Settings.Default.ColumnSource = _selectedColumn;
                Settings.Default.Save();
            }
            else
                _selectedColumn = null;
        }

        private void btnExec_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedColumn != null &&
                txtValeur.Text != null)
            {
                txtStatus.Text += "Cloning started" + Environment.NewLine;
                _cloneWorker.RunWorkerAsync(new ClonerWorkerInputArgs
                {
                    Server = _selectedServer,
                    Database = _selectedDatabase,
                    Schema = _selectedSchema,
                    Table = _selectedTable,
                    Columns = new ColumnsWithValue { { _selectedColumn, txtValeur.Text } },
                    NbCopies = sliderCopy.Value
                });
                BtnExec.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Vous devez sélectionner la source des données à copier.", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitCloner()
        {
            _cloner = new Cloner();
            _cloner.EnforceIntegrity = false;
            _cloner.StatusChanged += ClonerWorkerStatusChanged_event;
            _cloner.QueryCommiting += ClonerWorkerQueryCommiting_event;
        }

        private void InitClonerWorker()
        {
            _cloneWorker = new BackgroundWorker();
            _cloneWorker.WorkerReportsProgress = true;
            _cloneWorker.RunWorkerCompleted += (s, e) =>
            {
                var sbLog = new StringBuilder();
                var paramsOut = e.Result as ClonerWorkerOutputArgs;

                sbLog.Append("Cloning completed in : ")
                    .Append(DateTime.Now.Subtract(paramsOut.StartDate).ToString("hh':'mm':'ss'.'fff"))
                    .Append(Environment.NewLine);

                if (chkSimulation.IsChecked.GetValueOrDefault())
                {
                    sbLog.AppendLine("Simulation mode : No clone appended to the database.");
                }
                else
                {
                    foreach (var row in paramsOut.ClonedRow)
                    {
                        sbLog.Append("New clone : ")
                             .Append(row.Database).Append(".").Append(row.Schema)
                             .Append(".").Append(row.Table).Append(" : (");

                        foreach (var col in row.Columns)
                        {
                            var sqlVar = col.Value as SqlVariable;
                            sbLog.Append(col.Key).Append("=").Append(sqlVar ?? col.Value).Append(", ");
                        }

                        sbLog.Remove(sbLog.Length - 2, 2);
                        sbLog.Append(")").Append(Environment.NewLine);
                    }
                }

                sbLog.Append(Environment.NewLine);
                txtStatus.Text += sbLog.ToString();
                txtStatus.ScrollToEnd();

                BtnExec.IsEnabled = true;
            };
            _cloneWorker.ProgressChanged += (s, e) =>
            {
                var statusArgs = e.UserState as StatusChangedEventArgs;
                if (statusArgs != null)
                    StatusChanged_event(s, statusArgs);

                var queryArgs = e.UserState as QueryCommitingEventArgs;
                if (queryArgs != null)
                    QueryCommiting_event(s, queryArgs);
            };
            _cloneWorker.DoWork += (s, arg) =>
            {
                var paramsIn = arg.Argument as ClonerWorkerInputArgs;
                var paramsOut = new ClonerWorkerOutputArgs
                {
                    StartDate = DateTime.Now,
                    ClonedRow = new List<IRowIdentifier>()
                };

                var source = new RowIdentifier();
                source.Columns.Clear();
                source.ServerId = paramsIn.Server;
                source.Database = paramsIn.Database;
                source.Schema = paramsIn.Schema;
                source.Table = paramsIn.Table;
                source.Columns = paramsIn.Columns;

                if (paramsIn.ForceClone)
                    _cloner.Clear();

                //Clone
                for (int i = 0; i < paramsIn.NbCopies; i++)
                    paramsOut.ClonedRow.AddRange(_cloner.Clone(source, true));

                arg.Result = paramsOut;
            };
        }

        private void ClonerWorkerQueryCommiting_event(object sender, QueryCommitingEventArgs e)
        {
            //On doit savoir dans le thread 2 la valeur du thread 1
            //car ReportProgress call le thread 1 en asyn.
            System.Windows.Application.Current.Dispatcher.Invoke(
            DispatcherPriority.Normal,
            (ThreadStart)delegate { e.Cancel = chkSimulation.IsChecked.GetValueOrDefault(); });

            _cloneWorker.ReportProgress(0, e);
        }

        public void QueryCommiting_event(object sender, QueryCommitingEventArgs e)
        {
            scintilla.IsReadOnly = false;
            scintilla.Text = e.Query;
            scintilla.IsReadOnly = true;
        }

        public void ClonerWorkerStatusChanged_event(object sender, StatusChangedEventArgs e)
        {
            _cloneWorker.ReportProgress(0, e);
        }

        public void StatusChanged_event(object sender, StatusChangedEventArgs e)
        {
            if (e.Status == Status.Cloning)
            {
                var sb = new StringBuilder();
                sb.Append(new string(' ', 3 * e.Level));
                sb.Append(e.SourceRow.Database)
                    .Append(".")
                    .Append(e.SourceRow.Schema)
                    .Append(".")
                    .Append(e.SourceRow.Table)
                    .Append(" : (");
                foreach (var col in e.SourceRow.Columns)
                    sb.Append(col.Key).Append("=").Append(col.Value).Append(", ");
                sb.Remove(sb.Length - 2, 2);
                sb.Append(")").Append(Environment.NewLine);

                txtStatus.Text += sb.ToString();
                txtStatus.ScrollToEnd();
            }
            else if (e.Status == Status.FetchingDerivatives)
            {
                Console.WriteLine(new string(' ', 3 * e.Level) + "=================================");
            }
        }

        public class ClonerWorkerInputArgs
        {
            public Int16 Server { get; set; }
            public String Database { get; set; }
            public String Schema { get; set; }
            public String Table { get; set; }
            public ColumnsWithValue Columns { get; set; }
            public bool ForceClone { get; set; }
            public double NbCopies { get; set; }
        }

        public class ClonerWorkerOutputArgs
        {
            public DateTime StartDate { get; set; }
            public List<IRowIdentifier> ClonedRow { get; set; }
        }

        private class ClonedRows
        {
            public int RowId { get; set; }
            public string Result { get; set; }
        }
    }
}
