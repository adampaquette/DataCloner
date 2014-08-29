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
using DataCloner;

using Murmur;

namespace Class
{
    public class main
    {
        static int Main(string[] args)
        {
#if DEBUG
            //KeyRelationshipTest();
            ConfigTest();
            DataclonerTest1();
            //ActivatorTest();
            //CachedTableObjTest();
            //GeneralDBTest();
            //LoadingCacheTest();
            //ExtensionsTest();
            //Console.ReadKey();
#endif
            return 0;
        }
#if DEBUG

        public static void DataclonerTest1()
        {
            var dc = new DataCloner.DataCloner();
            RowIdentifier source = new RowIdentifier();

            //Map serveur source / destination
            dc.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "sakila" }, new ServerIdentifier { ServerId = 1, Database = "sakila" });
            dc.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "employees" }, new ServerIdentifier { ServerId = 1, Database = "employees" });

            dc.Initialize();

            /*******************
                 Employees
            *******************/
            //source.ServerId = 1;
            //source.Database = "employees";
            //source.Schema = "";
            //source.Table = "employees";
            //source.Columns.Add("emp_no", 10001);
            //dc.SqlTraveler(source, true, false);


            /*******************
                   SAKILA
            *******************/

            ////Basic test : 1 row
            //source.ServerId = 1;
            //source.Database = "sakila";
            //source.Schema = "";
            //source.Table = "actor";
            //source.Columns.Add("actor_id", 1);
            //dc.SqlTraveler(source, true, false);

            ////Basic test : 1 rows with dependencies
            //source.Columns.Clear();
            //source.ServerId = 1;
            //source.Database = "sakila";
            //source.Schema = "";
            //source.Table = "city";
            //source.Columns.Add("city_id", 9);
            //dc.SqlTraveler(source, true, false);

            //Medium test : 1 rows with lots of dependencies
            source.Columns.Clear();
            source.ServerId = 1;
            source.Database = "sakila";
            source.Schema = "";
            source.Table = "customer";
            source.Columns.Add("active", 0);
            source.Columns.Add("address_id", 20);
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
                Columns = new List<TableModifiersXml.ForeignKeyRemoveColumnXml>
                {
                    new TableModifiersXml.ForeignKeyRemoveColumnXml
                    {
                        Name = "col3"
                    },
                    new TableModifiersXml.ForeignKeyRemoveColumnXml
                    {
                        Name = "col4"
                    }
                }
            };

            var table1 = new TableModifiersXml.TableModifierXml();
            table1.Name = "table1";
            table1.IsStatic = false;
            table1.DataBuilders.Add(dataColumnBuilder1);
            table1.DerativeTablesConfig.GlobalAccess = DerivativeTableAccess.Forced;
            table1.DerativeTablesConfig.Cascade = true;
            table1.DerativeTablesConfig.DerativeTables.Add(derivativeTable1);
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
            config.Save("dctest.config");

            ConfigurationXml configLoaded;
            configLoaded = ConfigurationXml.Load("dctest.config");
        }
#endif
    }   
}