using System;
using System.Collections;
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
            //KeyRelationshipTest();
            //ConfigTest();
            DataclonerTest1();
            //ActivatorTest();
            //CachedTableObjTest();
            //GeneralDBTest();
            //LoadingCacheTest();
            //ExtensionsTest();
            //Console.ReadKey();
            return 0;
        }

        public static void KeyRelationshipTest()
        {
            KeyRelationship kr = new KeyRelationship();
            kr.SetKey(1, "db", "dbo", "table1", new object[] { 1, 1 }, new object[] { 1, 2 });
            kr.SetKey(1, "db", "dbo", "table1", new object[] { 1, 2 }, new object[] { 1 });

            object[] key = kr.GetKey(1, "db", "dbo", "table1", new object[] { 1, 1 });
            object[] key2 = kr.GetKey(1, "db", "dbo", "table1", new object[] { 1, 2 });

            if (!StructuralComparisons.StructuralEqualityComparer.Equals(new object[] { 1, 2 }, key))
                throw new Exception("");
            if (!StructuralComparisons.StructuralEqualityComparer.Equals(new object[] { 1 }, key2))
                throw new Exception("");
        }

        public static void DataclonerTest1()
        {
            var dc = new DataCloner.DataCloner();
            RowIdentifier source = new RowIdentifier();

            //Map serveur source / destination
            dc.ServerMap.Add(new Tuple<short, string>(1, "sakila"), new Tuple<short, string>(1, "sakila"));
            
            dc.Initialize();          

            //Basic test 1 row
            source.ServerId = 1;
            source.Database = "sakila";
            source.Schema = "";
            source.Table = "actor";
            source.Columns.Add("actor_id", 1);
            dc.SqlTraveler(source, true, false);

            //Medium test 1 rows with dependencies
            source.Columns.Clear();
            source.Table = "city";
            source.Columns.Add("city_id", 9);
            dc.SqlTraveler(source, true, false);

            //Medium test 15 rows with dependencies
            source.Columns.Clear();
            source.Table = "customer";
            source.Columns.Add("active", 0);
            dc.SqlTraveler(source, true, false);
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

        public static void CachedTableObjTest()
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

        public static void LoadingCacheTest()
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
            var cs2 = new ConnectionXml(2, "DataCloner.DataAccess.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false", 1);
            config.ConnectionStrings.Add(cs);
            config.ConnectionStrings.Add(cs2);

            var dataColumnBuilder1 = new TableModifiersXml.DataColumnBuilderXml()
            {
                BuilderName = "Client.Builder.CreatePK",
                Name = "col1"
            };

            var dataColumnBuilder2 = new TableModifiersXml.DataColumnBuilderXml()
            {
                BuilderName = "Client.Builder.CreateNAS",
                Name = "col4"
            };

            var derivativeTable1 = new TableModifiersXml.DerivativeSubTableXml()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Denied
            };

            var fkAdd1 = new TableModifiersXml.ForeignKeyAddXml()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table55",
                Columns = new List<TableModifiersXml.ForeignKeyColumnXml>()
                { 
                    new TableModifiersXml.ForeignKeyColumnXml()
                    {
                        NameFrom = "col1", 
                        NameTo = "col1"
                    },
                    new TableModifiersXml.ForeignKeyColumnXml()
                    {
                        NameFrom = "col2", 
                        NameTo = "col2"
                    }
                }
            };

            var fkRemove = new TableModifiersXml.ForeignKeyRemoveXml()
            {
                Name = "col3"
            };

            var table1 = new TableModifiersXml.TableModifierXml();
            table1.Name = "table1";
            table1.IsStatic = false;
            table1.DataBuilders.Add(dataColumnBuilder1);
            table1.DerativeTables.GloabalAccess = DerivativeTableAccess.Forced;
            table1.DerativeTables.Cascade = true;
            table1.DerativeTables.DerativeSubTables.Add(derivativeTable1);
            table1.ForeignKeys.ForeignKeyAdd.Add(fkAdd1);
            table1.ForeignKeys.ForeignKeyRemove.Add(fkRemove);

            var schema1 = new TableModifiersXml.SchemaXml()
            {
                Name = "dbo",
                Tables = new List<TableModifiersXml.TableModifierXml>() { table1 }
            };

            var database1 = new TableModifiersXml.DatabaseXml()
            {
                Name = "db",
                Schemas = new List<TableModifiersXml.SchemaXml>() { schema1 }
            };

            var server1 = new TableModifiersXml.ServerXml()
            {
                Id = 1,
                Databases = new List<TableModifiersXml.DatabaseXml>() { database1 }
            };

            config.TableModifiers.Servers.Add(server1);

            //Save / load from file
            //=====================
            var serialized = config.SerializeXml();
            config.Save("dc.config");

            ConfigurationXml configLoaded;
            configLoaded = ConfigurationXml.Load("dc.config");
        }
    }
}