using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DataCloner.Serialization;

namespace Class
{
    public class main
    {
        static int Main(string[] args)
        {
            var ti = new DataCloner.DataClasse.TableIdentifier();
            ti.DatabaseName = "botnet";
            ti.SchemaName = "botnet";
            ti.TableName = "link";

            var ri = new DataCloner.DataClasse.RowIdentifier();
            ri.TableIdentifier = ti;
            ri.Columns.Add("fromPageHostId", 6);
            ri.Columns.Add("fromPageId", 4);

            //var m = new DataCloner.DataAccess.QueryDatabaseMySQL("server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            //var dt = m.GetFK(ti);
            //m.Select(ri);


            //var conn = new System.Data.SqlClient.SqlConnection("Data Source=une_sql_pgis;Initial Catalog=PGISCBL;Integrated Security=SSPI;");
            //conn.Open();
            //if (conn.Database != "PGISCBL")
            //    conn.ChangeDatabase("PGISCBL");
            ////var dt = conn.GetSchema("Columns");
            //conn.Close();

            configTest();

            //var a = new DataCloner.DataCloner();
            //a.SQLTraveler(null, true, true);


            return 0;
        }

        public static void configTest()
        {
            var config = new Configuration();
            var cs = new Connection(1, "sql1", "DataCloner.DataAccess.QueryProviderMySQL", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            config.ConnectionStrings.Add(cs);

            //config.StaticTables.Add(new StaticTable(1, "root", "dbo", "table1", true));
            //config.StaticTables.Add(new StaticTable(1, "root", "dbo", "table2", true));


            //StaticTable a = new StaticTable();
            //StaticTable.ServerXML.DatabaseXML.SchemaXML.TableXML()
            //a.Server.Add(new s


            //Save / load from file
            string serialized = SerizlizeXML(config);
            config.Save();

            Configuration configLoaded;
            configLoaded = Configuration.Load();
        }

        public static string SerizlizeXML<T>(T obj)
        {
            var xs = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var sw = new System.IO.StringWriter();
            var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(sw, obj, ns);
            return sw.ToString();
        }
    }
}