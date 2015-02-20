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
using System.Windows.Shapes;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Configuration;

namespace DataCloner.GUI
{
    /// <summary>
    /// Interaction logic for Debug.xaml
    /// </summary>
    public partial class Debug : Window
    {
        private Cloner dc = new Cloner();

        public Debug()
        {
            InitializeComponent();

            dc.EnforceIntegrity = false;
            dc.StatusChanged += (s, arg) =>
            {
                var sb = new StringBuilder();
                //sb.Append(e.Status.ToString()).Append(" : ").Append(Environment.NewLine);
                sb.Append(new string(' ', 3 * arg.Level));
                sb.Append(arg.SourceRow.Database).Append(".").Append(arg.SourceRow.Schema).Append(".").Append(arg.SourceRow.Table).Append(" : (");
                foreach (var col in arg.SourceRow.Columns)
                    sb.Append(col.Key).Append("=").Append(col.Value).Append(", ");
                sb.Remove(sb.Length - 2, 2);
                sb.Append(")");
                TxtStatus.Text += sb.ToString();
            };
        }

        private void BtnBuildAllCache_Click(object sender, RoutedEventArgs e)
        {
            TxtStatus.Text += "Building cache..." + Environment.NewLine;
            var config = Configuration.Load(Configuration.ConfigFileName);
            dc.Setup(config.Applications[0], 1, null);
            TxtStatus.Text += "Building cache completed." + Environment.NewLine;
        }

        private void BtnClone_Click(object sender, RoutedEventArgs e)
        {
            BtnBuildAllCache_Click(null, null);

            var source = new RowIdentifier();

            source.Columns.Clear();
            source.ServerId = Int16.Parse(TxtServer.Text);
            source.Database = TxtDatabase.Text;
            source.Schema = TxtSchema.Text;
            source.Table = TxtTable.Text;
            source.Columns.Add("employeeNumber", 1188);
            dc.Clone(source, true);
        }
    }
}
