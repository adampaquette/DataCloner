using DataCloner.Configuration;
using DataCloner.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using DataCloner.Framework;

namespace DataCloner.GUI.View
{
    /// <summary>
    /// Interaction logic for CloneView.xaml
    /// </summary>
    public partial class CloneView : UserControl
    {
        private const string FileElement = "File";
        private const string FileExtension = ".dca";
        private const string Filter = "Datacloner archive (.dca)|*.dca";

        private Cloner _cloner;
        private BackgroundWorker _cloneWorker;
        private ProjectContainer _proj = ProjectContainer.Load("northWind.dcproj");
        private IEnumerable<Map> _maps;
        private Map _fromMaps;

        private Int16 _selectedServer;
        private string _selectedDatabase;
        private string _selectedSchema;
        private string _selectedTable;
        private string _selectedColumn;

        private int derivativeLevel = -1;

        private List<Int16> Servers = null;

        public CloneView()
        {
            InitClonerWorker();
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            chkSimulation.IsChecked = Properties.Settings.Default.IsSimulation;
            chkOptimisation.IsChecked = Properties.Settings.Default.DoOptimisation;

            cbDatabaseConfig.ItemsSource = _proj.Behaviours;

            //Tente de charger la préférence utilisateur
            var config = _proj.Behaviours.FirstOrDefault(c => c.Id == Properties.Settings.Default.DatabaseConfigId);
            if (config != null)
                cbDatabaseConfig.SelectedItem = config;
            else
            {
                if (_proj.Behaviours.Count == 1)
                    cbDatabaseConfig.SelectedIndex = 0;
            }
        }

        private void CbDatabaseConfig_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_proj == null || _proj.Maps == null || cbDatabaseConfig.SelectedValue == null)
                return;

            _maps = _proj.Maps.Where(m => m.UsableBehaviours.Split(',').Contains(cbDatabaseConfig.SelectedValue.ToString()));
            if (_maps != null)
            {
                if (cbDatabaseConfig.SelectedValue != null)
                {
                    Properties.Settings.Default.DatabaseConfigId = (Int16)cbDatabaseConfig.SelectedValue;
                    Properties.Settings.Default.Save();
                }

                cbSourceEnvir.ItemsSource = _maps;

                //Tente de charger la préférence utilisateur
                _fromMaps = _maps.FirstOrDefault(m => m.From == Properties.Settings.Default.SourceEnvir);
                if (_fromMaps != null)
                    cbSourceEnvir.SelectedItem = _fromMaps;
                else
                {
                    if (_maps.Count() == 1)
                        cbSourceEnvir.SelectedIndex = 0;
                }
                //Force dans le cas oû l'environnement ne change pas
                cbSourceEnvir_SelectionChanged(null, null);
            }
        }

        private void cbSourceEnvir_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_maps == null || cbSourceEnvir.SelectedValue == null)
                return;
            _fromMaps = _maps.FirstOrDefault(m => m.From == cbSourceEnvir.SelectedValue.ToString());
            if (_fromMaps != null)
            {
                Properties.Settings.Default.SourceEnvir = cbSourceEnvir.SelectedValue.ToString();
                Properties.Settings.Default.Save();

                var _mapsTo = _maps.Where(m =>
                        m.From == _fromMaps.From &&
                        m.UsableBehaviours.Split(',').Contains(cbDatabaseConfig.SelectedValue.ToString()));
                cbDestinationEnvir.ItemsSource = _mapsTo;

                //Tente de charger la préférence utilisateur
                var map = _maps.FirstOrDefault(m => m.From == cbSourceEnvir.SelectedValue.ToString() &&
                                               m.To == Properties.Settings.Default.DestinationEnvir &&
                                               m.UsableBehaviours.Split(',').Contains(cbDatabaseConfig.SelectedValue.ToString()));
                if (map != null)
                    cbDestinationEnvir.SelectedItem = map;
                else
                {
                    if (_mapsTo.Count() == 1)
                        cbDestinationEnvir.SelectedIndex = 0;
                }
                //Force dans le cas oû l'environnement ne change pas
                cbDestinationEnvir_SelectionChanged(null, null);
            }
        }

        private void cbDestinationEnvir_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            if (_maps == null || cbDestinationEnvir.SelectedValue == null)
                return;
            var map = _maps.FirstOrDefault(m => m.From == cbSourceEnvir.SelectedValue.ToString() &&
                                               m.To == cbDestinationEnvir.SelectedValue.ToString() &&
                                               m.UsableBehaviours.Split(',').Contains(cbDatabaseConfig.SelectedValue.ToString()));
            if (map != null)
            {
                Properties.Settings.Default.DestinationEnvir = cbDestinationEnvir.SelectedValue.ToString();
                Properties.Settings.Default.Save();

                var configId = (Int16)cbDatabaseConfig.SelectedValue;
                var selectedSettings = new Settings
                {
                    Project = _proj,
                    MapId = map.Id,
                    BehaviourId = configId
                };

                _cloner = new Cloner(selectedSettings);
                _cloner.StatusChanged += ClonerWorkerStatusChanged_event;
                _cloner.QueryCommiting += ClonerWorkerQueryCommiting_event;

                Servers = _cloner.MetadataCtn.Metadatas.Keys.ToArray().ToList();
                cbServer.ItemsSource = Servers;

                //Tente de charger la préférence utilisateur
                if (Servers.Contains(Properties.Settings.Default.ServerSource))
                    cbServer.SelectedItem = Properties.Settings.Default.ServerSource;
                else
                {
                    if (Servers.Count == 1)
                        cbServer.SelectedIndex = 0;
                }
            }
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show("Erreur lors de l'initialisation de la cache.\r\n" + ex.ToString(), "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            //    throw;
            //}
        }

        private void cbServer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbServer.SelectedIndex != -1)
            {
                _selectedServer = (Int16)cbServer.SelectedValue;

                Properties.Settings.Default.ServerSource = _selectedServer;
                Properties.Settings.Default.Save();

                var databases = _cloner.MetadataCtn.Metadatas[_selectedServer].Keys.ToArray().ToList();
                cbDatabase.ItemsSource = databases;

                //Tente de charger la préférence utilisateur
                if (databases.Contains(Properties.Settings.Default.DatabaseSource))
                    cbDatabase.SelectedItem = Properties.Settings.Default.DatabaseSource;
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

                Properties.Settings.Default.DatabaseSource = _selectedDatabase;
                Properties.Settings.Default.Save();

                var schemas = _cloner.MetadataCtn.Metadatas[_selectedServer][_selectedDatabase].Keys.ToArray().ToList();
                cbSchema.ItemsSource = schemas;

                //Tente de charger la préférence utilisateur
                if (schemas.Contains(Properties.Settings.Default.SchemaSource))
                    cbSchema.SelectedItem = Properties.Settings.Default.SchemaSource;
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

                Properties.Settings.Default.SchemaSource = _selectedSchema;
                Properties.Settings.Default.Save();

                var tables = _cloner.MetadataCtn.Metadatas[_selectedServer][_selectedDatabase][_selectedSchema].Select(t => t.Name).ToList();
                cbTable.ItemsSource = tables;

                //Tente de charger la préférence utilisateur
                if (tables.Contains(Properties.Settings.Default.TableSource))
                    cbTable.SelectedItem = Properties.Settings.Default.TableSource;
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

                Properties.Settings.Default.TableSource = _selectedTable;
                Properties.Settings.Default.Save();

                var table =
                    _cloner.MetadataCtn.Metadatas[_selectedServer][_selectedDatabase][_selectedSchema].FirstOrDefault(
                        t => t.Name == _selectedTable);
                if (table == null)
                    return;
                var columns = table.ColumnsDefinition.Select(c => c.Name).ToList();
                cbColonne.ItemsSource = columns;

                //Tente de charger la préférence utilisateur
                if (columns.Contains(Properties.Settings.Default.ColumnSource))
                    cbColonne.SelectedItem = Properties.Settings.Default.ColumnSource;
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

                Properties.Settings.Default.ColumnSource = _selectedColumn;
                Properties.Settings.Default.Save();
            }
            else
                _selectedColumn = null;
        }

        private void btnExec_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedColumn != null &&
                txtValeur.Text != null)
            {
                rtbStatus.AppendText("Cloning started");
                _cloner.OptimiseExecutionPlan = (bool)chkOptimisation.IsChecked;
                _cloner.Clear();

                scintilla.IsReadOnly = false;
                scintilla.Text = string.Empty;
                scintilla.IsReadOnly = true;

                _cloneWorker.RunWorkerAsync(new ClonerWorkerInputArgs
                {
                    Server = _selectedServer,
                    Database = _selectedDatabase,
                    Schema = _selectedSchema,
                    Table = _selectedTable,
                    Columns = new ColumnsWithValue { { _selectedColumn, txtValeur.Text } },
                    NbCopies = 1
                });
                BtnExec.IsEnabled = false;
            }
            else
            {
                MessageBox.Show("Vous devez sélectionner la source des données à copier.", "Attention", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitClonerWorker()
        {
            _cloneWorker = new BackgroundWorker();
            _cloneWorker.WorkerReportsProgress = true;
            _cloneWorker.RunWorkerCompleted += (s, e) =>
            {
                var sbLog = new StringBuilder();
                var paramsOut = e.Result as ClonerWorkerOutputArgs;

                sbLog.Append("\rCloning completed in : ")
                    .Append(DateTime.Now.Subtract(paramsOut.StartDate).ToString("hh':'mm':'ss'.'fff"))
                    .Append("\r");

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
                        sbLog.Append(")").Append("\r");
                    }
                }

                sbLog.Append("\r");
                rtbStatus.AppendText(sbLog.ToString());
                rtbStatus.ScrollToEnd();

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
                    ClonedRow = new List<RowIdentifier>()
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
                    paramsOut.ClonedRow.AddRange(_cloner.AppendStep(source, true).Execute().Clones);

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
            scintilla.Text += e.Command.GetGeneratedQuery();
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
                var p = new Paragraph();
                p.Margin = new Thickness(0);
                p.Inlines.Add(e.Level.ToString());
                p.Inlines.Add(new string(' ', 4 * (e.Level + 1)));

                var sb = new StringBuilder();
                sb.Append(e.SourceRow.Database)
                    .Append(".")
                    .Append(e.SourceRow.Schema)
                    .Append(".")
                    .Append(e.SourceRow.Table)
                    .Append(" : (");
                foreach (var col in e.SourceRow.Columns)
                    sb.Append(col.Key).Append("=").Append(col.Value).Append(", ");
                sb.Remove(sb.Length - 2, 2);
                sb.Append(")");

                if (derivativeLevel == e.Level)
                {
                    var span = new Span();
                    span.Background = Brushes.PowderBlue;
                    span.Inlines.Add(sb.ToString());
                    p.Inlines.Add(span);

                    derivativeLevel = -1;
                }
                else
                    p.Inlines.Add(sb.ToString());

                rtbStatus.Document.Blocks.Add(p);
            }
            else if (e.Status == Status.FetchingDerivatives)
                derivativeLevel = e.Level;
            rtbStatus.ScrollToEnd();
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
            public List<RowIdentifier> ClonedRow { get; set; }
        }

        private class ClonedRows
        {
            public int RowId { get; set; }
            public string Result { get; set; }
        }

        private void chkSimulation_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.IsSimulation = (bool)chkSimulation.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void chkOptimisation_Checked(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.DoOptimisation = (bool)chkOptimisation.IsChecked;
            Properties.Settings.Default.Save();
        }
    }
}
