using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;
using System.Security.Cryptography;
using System.Diagnostics;

using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using DataCloner.Enum;

using Murmur;

namespace Class
{
    public class main
    {
        static int Main(string[] args)
        {
            //ActivatorTest();

            /*CachedTableTest();
            GeneralDBTest();
            ConfigTest();*/
            CacheTest();
            //ExtensionsTest();

            Console.ReadKey();
            return 0;
        }

        public static void ActivatorTest()
        {
            string strType = "DataCloner.DataAccess.QueryProviderMySql";
            string strConn = "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false";
            Int16 server = 1;
            Type t = Type.GetType(strType);
            Stopwatch sw = new Stopwatch();
            int nbLoop = 100000;


            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            sw.Reset();
            sw.Start();
            for (int i = 0; i < nbLoop; i++)
            {
                var providerFA = FastActivator<string, Int16>.GetConstructor(t, new Type[] { typeof(string), typeof(Int16) })(strConn, server);
            }
            sw.Stop();
            Console.WriteLine("FastActivator.GetConstructor : " + sw.ElapsedMilliseconds.ToString());


            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            sw.Reset();
            sw.Start();
            for (int i = 0; i < nbLoop; i++)
            {
                var providerFA2 = FastActivator<string, Int16>.CreateInstance(t, strConn, server);
            }
            sw.Stop();
            Console.WriteLine("FastActivator.CreateInstance : " + sw.ElapsedMilliseconds.ToString());


            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            sw.Start();
            for (int i = 0; i < nbLoop; i++)
            {
                var provider = Activator.CreateInstance(t, new object[] { strConn, server }) as IQueryProvider;
            }
            sw.Stop();
            Console.WriteLine("Activator.CreateInstance : " + sw.ElapsedMilliseconds.ToString());
        }

        public DataTable Read1(string query)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Connection.Open();
                var table = new DataTable();
                using (var r = cmd.ExecuteReader())
                    table.Load(r);
                return table;
            }
        }

        public List<S> Read5<S>(string query, Func<IDataRecord, S> selector)
        {
            System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;
                cmd.Connection.Open();
                using (var r = cmd.ExecuteReader())
                {
                    var items = new List<S>();
                    while (r.Read())
                        items.Add(selector(r));
                    return items;
                }
            }
        }

        public static void GeneralDBTest()
        {
            var conn = new MySql.Data.MySqlClient.MySqlConnection("server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM botnet.page limit 5;";
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        Console.WriteLine(r["id"].ToString());
                }
            }

            var dt = new DataTable();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT * FROM botnet.page limit 5;";
                using (var r = cmd.ExecuteReader())
                    dt.Load(r);
            }

            //var ti = new TableIdentifier { DatabaseName = "botnet", SchemaName = "botnet", TableName = "link" };
            //var ri = new RowIdentifier { TableIdentifier = ti };
            //ri.Columns.Add("fromPageHostId", 6);
            //ri.Columns.Add("fromPageId", 4);

            //var m = new DataCloner.DataAccess.QueryProviderMySql("server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            //var dt = m.GetFK(ti);
            //m.Select(ri);

            //var conn = new System.Data.SqlClient.SqlConnection("Data Source=une_sql_pgis;Initial Catalog=PGISCBL;Integrated Security=SSPI;");
            //conn.Open();
            //if (conn.Database != "PGISCBL")
            //    conn.ChangeDatabase("PGISCBL");
            ////var dt = conn.GetSchema("Columns");
            //conn.Close();

            //var a = new DataCloner.DataCloner();
            //a.SQLTraveler(null, true, true);
        }

        public static void CachedTableTest()
        {
            var ct = new CachedTables();
            var table1 = new TableDef();

            table1.Name = "table1";
            table1.IsStatic = false;
            table1.SelectCommand = "SELECT * FROM TABLE1";
            table1.InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)";

            table1.SchemaColumns = table1.SchemaColumns.Add(new SchemaColumn()
            {
                Name = "COL1",
                Type = "INT",
                IsPrimary = true,
                IsForeignKey = false,
                IsAutoIncrement = true,
                BuilderName = ""
            });
            table1.SchemaColumns = table1.SchemaColumns.Add(new SchemaColumn()
            {
                Name = "COL2",
                Type = "INT",
                IsPrimary = false,
                IsForeignKey = false,
                IsAutoIncrement = false,
                BuilderName = "Builder.NASBuilder"
            });

            table1.DerivativeTables = table1.DerivativeTables.Add(new DerivativeTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Forced,
                Cascade = true
            });
            table1.DerivativeTables = table1.DerivativeTables.Add(new DerivativeTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table3",
                Access = DerivativeTableAccess.Denied,
                Cascade = false
            });

            table1.ForeignKeys = table1.ForeignKeys.Add(new ForeignKey()
            {
                ServerIdTo = 2,
                DatabaseTo = "db",
                SchemaTo = "dbo",
                TableTo = "TABLE2",
                Columns = new ForeignKeyColumn[] { new ForeignKeyColumn() { NameFrom = "COL1", NameTo = "COL1" } }
            });

            ct.Add(1, "db", "dbo", table1);

            MemoryStream ms1 = new MemoryStream();
            MemoryStream ms2 = new MemoryStream();

            //Test TableDef
            table1.Serialize(ms1);
            ms1.Position = 0;
            var output = TableDef.Deserialize(ms1);
            output.Serialize(ms2);

            if (!ms1.ToArray().SequenceEqual(ms2.ToArray()))
                throw new Exception("");

            //Test cachedtables
            ms1 = new MemoryStream();
            ms2 = new MemoryStream();

            ct.Serialize(ms1);
            ms1.Position = 0;
            var outputCT = CachedTables.Deserialize(ms1);
            outputCT.Serialize(ms2);

            if (!ms1.ToArray().SequenceEqual(ms2.ToArray()))
                throw new Exception("");
        }

        public static void CacheTest()
        {
            string configFileName = "dc.config";
            string cacheFileName = "dc.cache";

            ////Hash config file
            //HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            //byte[] configFile = File.ReadAllBytes(configFileName);
            //string hashConfigFile = Encoding.Default.GetString(murmur.ComputeHash(configFile));

            ////Build new cache file
            //var fsOutputConfig = new FileStream(cacheFileName, FileMode.Create);
            //var config = new Configuration();
            //config.ConfigFileHash = hashConfigFile;
            //config.Serialize(fsOutputConfig);

            //fsOutputConfig.Close();

            //Test reload of cache
            var dispatcher = new QueryDispatcher();
            dispatcher.Initialize();
        }

        public static void ExtensionsTest()
        {
            var t = new int[] { 1, 2, 3, 4 };

            //RemoveAt
            var m = t.RemoveAt(0);
            if (!m.SequenceEqual(new int[] { 2, 3, 4 }))
                throw new Exception("");

            m = m.RemoveAt(2);
            if (!m.SequenceEqual(new int[] { 2, 3 }))
                throw new Exception("");

            //Remove
            var n = t.Remove(4);
            if (!n.SequenceEqual(new int[] { 1, 2, 3 }))
                throw new Exception("");

            //Add
            var l = t.Add(5).Add(1);
            if (!l.SequenceEqual(new int[] { 1, 2, 3, 4, 5, 1 }))
                throw new Exception("");
        }

        public static void ConfigTest()
        {
            var config = new ConfigurationXml();

            //ConnectionXML
            //=============
            var cs = new ConnectionXml(1, "DataCloner.DataAccess.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false", 1);
            config.ConnectionStrings.Add(cs);

            //StaticTableXml
            //==============            
            var schema1 = new StaticTableXml.SchemaXml { Name = "dbo" };
            schema1.Tables.Add(new StaticTableXml.TableXml("table1", true));
            schema1.Tables.Add(new StaticTableXml.TableXml("table2", true));

            var schema2 = new StaticTableXml.SchemaXml { Name = "master" };
            schema2.Tables.Add(new StaticTableXml.TableXml("person", true));
            schema2.Tables.Add(new StaticTableXml.TableXml("house", true));

            var listSchema = new List<StaticTableXml.SchemaXml> { schema1, schema2 };

            var database = new StaticTableXml.DatabaseXml(listSchema, "db");
            var server = new StaticTableXml.ServerXml(new List<StaticTableXml.DatabaseXml> { database }, 1);
            var server2 = new StaticTableXml.ServerXml(new List<StaticTableXml.DatabaseXml> { database }, 2);
            var staticTable = new StaticTableXml(new List<StaticTableXml.ServerXml> { server, server2 });
            config.StaticTables = staticTable;

            //DataBuilderXml 
            //==============  
            var col1DB = new DataBuilderXml.ColumnXml("ID", "DataCloner.Builder.Generic", "CreateID", true);
            var col2DB = new DataBuilderXml.ColumnXml("NAS", "Client.Builder.Builder1", "CreateNAS", true);

            var schemaDB1 = new DataBuilderXml.SchemaXml { Name = "master" };
            schemaDB1.Tables.Add(new DataBuilderXml.TableXml("person", new List<DataBuilderXml.ColumnXml>() { col1DB, col2DB }));

            var listSchemaDB = new List<DataBuilderXml.SchemaXml> { schemaDB1 };

            var databaseDB = new DataBuilderXml.DatabaseXml(listSchemaDB, "db");
            var serverDB = new DataBuilderXml.ServerXml(new List<DataBuilderXml.DatabaseXml> { databaseDB }, 1);
            var dataBuilders = new DataBuilderXml(new List<DataBuilderXml.ServerXml> { serverDB });
            config.DataBuilders = dataBuilders;

            //DerivativeTableAccessXml
            //========================
            var toDTA = new DerivativeTableAccessXml.TableToXml("table1", DerivativeTableAccess.Forced, true, true);
            var lstToDTA = new List<DerivativeTableAccessXml.TableToXml>() { toDTA };
            var schemaDerivativeTableAccess = new DerivativeTableAccessXml.SchemaXml { Name = "dbo" };
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXml.TableFromXml("table1", DerivativeTableAccess.Denied, true, true, null));
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXml.TableFromXml("table2", DerivativeTableAccess.Forced, true, false, null));
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXml.TableFromXml("table3", DerivativeTableAccess.NotSet, true, false, lstToDTA));

            var listSchemaDerivativeTableAccess = new List<DerivativeTableAccessXml.SchemaXml>
            {
                schemaDerivativeTableAccess
            };

            var databaseDerivativeTableAccess = new DerivativeTableAccessXml.DatabaseXml(listSchemaDerivativeTableAccess, "db");
            var serverDerivativeTableAccess = new DerivativeTableAccessXml.ServerXml(new List<DerivativeTableAccessXml.DatabaseXml> { databaseDerivativeTableAccess }, 1);
            var derivativeTableAccess = new DerivativeTableAccessXml(new List<DerivativeTableAccessXml.ServerXml> { serverDerivativeTableAccess });
            config.DerivativeTableAccess = derivativeTableAccess;

            //ForeignKeysXml
            //==============
            var serverfk1 = new ForeignKeysXml.ServerXml();
            var dbfk1 = new ForeignKeysXml.DatabaseXml();
            var schemafk1 = new ForeignKeysXml.SchemaXml();
            var tablefk1 = new ForeignKeysXml.TableXml();
            var addForeignKeyXmLfk1 = new ForeignKeysXml.AddForeignKeyXml();
            var removeForeignKeyXmLfk1 = new ForeignKeysXml.RemoveForeignKeyXml();
            var removeForeignKeyXmLfk2 = new ForeignKeysXml.RemoveForeignKeyXml();
            var collumnNameXmLfk1 = new ForeignKeysXml.CollumnNameXml();
            var collumnXmLfk1 = new ForeignKeysXml.CollumnXml();
            var collumnXmLfk2 = new ForeignKeysXml.CollumnXml();

            collumnXmLfk1.ColNameDest = "col1";
            collumnXmLfk1.Name = "col1";

            collumnXmLfk2.ColNameDest = "col2";
            collumnXmLfk2.Name = "col2";

            collumnNameXmLfk1.Name = "col3";

            addForeignKeyXmLfk1.ServerIdDest = 1;
            addForeignKeyXmLfk1.DatabaseDest = "db1";
            addForeignKeyXmLfk1.SchemaDest = "dbo";
            addForeignKeyXmLfk1.TableDest = "table1";
            addForeignKeyXmLfk1.Collumns.Add(collumnXmLfk1);
            addForeignKeyXmLfk1.Collumns.Add(collumnXmLfk2);

            removeForeignKeyXmLfk1.Name = "fk1";

            removeForeignKeyXmLfk2.Collumns.Add(collumnNameXmLfk1);

            tablefk1.Name = "table1";
            tablefk1.AddForeignKeys.Add(addForeignKeyXmLfk1);
            tablefk1.RemoveForeignKeys.Add(removeForeignKeyXmLfk1);
            tablefk1.RemoveForeignKeys.Add(removeForeignKeyXmLfk2);

            schemafk1.Name = "dbo";
            schemafk1.Tables.Add(tablefk1);

            dbfk1.Name = "db1";
            dbfk1.Schemas.Add(schemafk1);

            serverfk1.Id = 1;
            serverfk1.Databases.Add(dbfk1);

            config.ForeignKeys.Servers.Add(serverfk1);

            //Save / load from file
            //=====================
            var serialized = config.SerializeXml();
            config.Save("dc.config");

            ConfigurationXml configLoaded;
            configLoaded = ConfigurationXml.Load("dc.config");
        }
    }
}