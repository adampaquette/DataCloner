using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Configuration;

namespace DataCloner.GUI
{
    /// <summary>
    /// Interaction logic for Debug.xaml
    /// </summary>
    public partial class Debug : Window
    {
        private Cloner _dc;
        private BackgroundWorker _cloneWorker;
        private Configuration _config = Configuration.Load(Configuration.ConfigFileName);

        public Debug()
        {
            InitializeComponent();
            InitCloner();
            InitClonerWorker();
        }

        private void InitCloner()
        {
            _dc = new Cloner();
            _dc.EnforceIntegrity = false;
            _dc.StatusChanged += ClonerWorkerStatusChanged_event;
        }

        private void InitClonerWorker()
        {
            _cloneWorker = new BackgroundWorker();
            _cloneWorker.WorkerReportsProgress = true;
            _cloneWorker.RunWorkerCompleted += (s, e) =>
            {
                var sbLog = new StringBuilder();
                var paramsOut = e.Result as ClonerWorkerOutputArgs;

                sbLog.Append(Environment.NewLine)
                    .Append("Cloning completed in : ")
                    .Append(DateTime.Now.Subtract(paramsOut.StartDate).ToString("hh':'mm':'ss'.'fff"))
                    .Append(Environment.NewLine);

                foreach (var row in paramsOut.ClonedRow)
                {
                    sbLog.Append("New clone : ")
                         .Append(row.Database).Append(".").Append(row.Schema)
                         .Append(".").Append(row.Table).Append(" : (");

                    foreach (var col in row.Columns)
                        sbLog.Append(col.Key).Append("=").Append(col.Value).Append(", ");

                    sbLog.Remove(sbLog.Length - 2, 2);
                    sbLog.Append(")").Append(Environment.NewLine);
                }
               
                sbLog.Append(Environment.NewLine);
                TxtStatus.Text += sbLog.ToString();
                TxtStatus.ScrollToEnd();

                BtnClone.IsEnabled = true;
                BtnForceClone.IsEnabled = true;
            };
            _cloneWorker.ProgressChanged += (s, e) =>
            {
                var args = e.UserState as StatusChangedEventArgs;
                StatusChanged_event(s, args);
            };
            _cloneWorker.DoWork += (s, arg) =>
            {
                var paramsIn = arg.Argument as ClonerWorkerInputArgs;
                var paramsOut = new ClonerWorkerOutputArgs { StartDate = DateTime.Now };

                var source = new RowIdentifier();
                source.Columns.Clear();
                source.ServerId = Int16.Parse(paramsIn.Server);
                source.Database = paramsIn.Database;
                source.Schema = paramsIn.Schema;
                source.Table = paramsIn.Table;
                source.Columns = paramsIn.Columns;

                if(paramsIn.ForceClone)
                    _dc.Clear();
                _dc.Setup(_config.Applications[0], 1, null);
                paramsOut.ClonedRow = _dc.Clone(source, true);

                arg.Result = paramsOut;
            };
        }

        public void ClonerWorkerStatusChanged_event(object sender, StatusChangedEventArgs e)
        {
            _cloneWorker.ReportProgress(0, e);
        }

        public void StatusChanged_event(object sender, StatusChangedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.Append(new string(' ', 3 * e.Level));
            sb.Append(e.SourceRow.Database).Append(".").Append(e.SourceRow.Schema).Append(".").Append(e.SourceRow.Table).Append(" : (");
            foreach (var col in e.SourceRow.Columns)
                sb.Append(col.Key).Append("=").Append(col.Value).Append(", ");
            sb.Remove(sb.Length - 2, 2);
            sb.Append(")").Append(Environment.NewLine);

            TxtStatus.Text += sb.ToString();
            TxtStatus.ScrollToEnd();
        }

        private void BtnReloadConfigBuildAllCache_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text += "Building cache..." + Environment.NewLine;
            _config = Configuration.Load(Configuration.ConfigFileName);
            _dc.Setup(_config.Applications[0], 1, null);
            TxtStatus.Text += "Building cache completed." + Environment.NewLine;
            TxtStatus.ScrollToEnd();
        }

        private void BtnClone_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text += "Cloning started" + Environment.NewLine;
            _cloneWorker.RunWorkerAsync(new ClonerWorkerInputArgs
            {
                Server = TxtServer.Text,
                Database = TxtDatabase.Text,
                Schema = TxtSchema.Text,
                Table = TxtTable.Text,
                Columns = new ColumnsWithValue { { TxtColumn.Text, TxtValue.Text } }
            });
            BtnClone.IsEnabled = false;
            BtnForceClone.IsEnabled = false;
        }

        private void BtnForceClone_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text += "Cloning started" + Environment.NewLine;
            _cloneWorker.RunWorkerAsync(new ClonerWorkerInputArgs
            {
                Server = TxtServer.Text,
                Database = TxtDatabase.Text,
                Schema = TxtSchema.Text,
                Table = TxtTable.Text,
                Columns = new ColumnsWithValue { { TxtColumn.Text, TxtValue.Text } },
                ForceClone = true
            });
            BtnClone.IsEnabled = false;
            BtnForceClone.IsEnabled = false;
        }

        public class ClonerWorkerInputArgs
        {
            public string Server { get; set; }
            public String Database { get; set; }
            public String Schema { get; set; }
            public String Table { get; set; }
            public ColumnsWithValue Columns { get; set; }
            public bool ForceClone { get; set; }
        }

        public class ClonerWorkerOutputArgs
        {
            public DateTime StartDate { get; set; }
            public List<IRowIdentifier> ClonedRow { get; set; }
        }
    }
}
