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
using DataCloner.Archive;

using System.Data.SQLite;

using Murmur;

namespace Class
{
    public class main
    {
        static int Main(string[] args)
        {
#if DEBUG
            //ArchiveTest();
            //ServerMapTest();
            DataclonerTest1();
#endif
            return 0;
        }
#if DEBUG

        public static void ArchiveTest()
        {
            string outputFile = "compressed.dca";
            //var inputFiles = new List<string>{ "dc.cache","System.Data.SQLite.xml", "MySql.Data.dll"};
            //var archiveDescription = "Individu de type 1A ayant deux certificats.";

            //var input = new List<byte>(System.Text.Encoding.UTF8.GetBytes(archiveDescription));

            //foreach(var file in inputFiles)
            //{
            //    if(!File.Exists(file)) continue;
            //    input.AddRange(File.ReadAllBytes(file));
            //}

            //Server maps
            var sm = new ServersMapsXml();

            var r1 = new ServersMapsXml.RoadXml
            {
                ServerSrc = 3,
                DatabaseSrc = "db1",
                SchemaSrc = "",
                ServerDst = 1,
                DatabaseDst = "cloned",
                SchemaDst = ""
            };

            var r2 = new ServersMapsXml.RoadXml
            {
                ServerSrc = 2,
                DatabaseSrc = "db1",
                SchemaSrc = "",
                ServerDst = 1,
                DatabaseDst = "cloned",
                SchemaDst = ""
            };

            var map = new ServersMapsXml.MapXml
            {
                nameFrom = "PROD",
                nameTo = "UNE",
                Roads = new List<ServersMapsXml.RoadXml> { r1, r2 }
            };

            sm.Maps.Add(map);

            //Cache
            CachedTables ct = new CachedTables();
            TableDef table = new TableDef();

            table.Name = "table1";
            table.IsStatic = false;
            table.SelectCommand = "SELECT * FROM TABLE1";
            table.InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)";

            table.SchemaColumns = table.SchemaColumns.Add(new SchemaColumn()
            {
                Name = "COL1",
                Type = "INT",
                IsPrimary = true,
                IsForeignKey = false,
                IsAutoIncrement = true,
                BuilderName = ""
            });
            table.SchemaColumns = table.SchemaColumns.Add(new SchemaColumn()
            {
                Name = "COL2",
                Type = "INT",
                IsPrimary = false,
                IsForeignKey = false,
                IsAutoIncrement = false,
                BuilderName = "Builder.NASBuilder"
            });

            table.DerivativeTables = table.DerivativeTables.Add(new DerivativeTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Forced,
                Cascade = true
            });
            table.DerivativeTables = table.DerivativeTables.Add(new DerivativeTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table3",
                Access = DerivativeTableAccess.Denied,
                Cascade = false
            });

            table.ForeignKeys = table.ForeignKeys.Add(new ForeignKey()
            {
                ServerIdTo = 2,
                DatabaseTo = "db",
                SchemaTo = "dbo",
                TableTo = "TABLE2",
                Columns = new ForeignKeyColumn[] { new ForeignKeyColumn() { NameFrom = "COL1", NameTo = "COL1" } }
            });

            ct.Add(1, "db1", "dbo", table);
            ct.Add(1, "db2", "dbo", table);

            Configuration config = new Configuration();
            config.ConnectionStrings = new List<Connection> { new Connection { Id = 1, ConnectionString = "", ProviderName = "", SameConfigAsId = 0 } };
            config.ConfigFileHash = "";
            config.CachedTables = ct;

            //Créaton de l'archive
            var ar = new DataArchive();
            ar.Description = "aloll 1 un deux test test";
            ar.Cache = config;
            ar.OriginalQueries = new List<RowIdentifier>();
            ar.Databases = new List<string> { "System.Data.SQLite.xml", "dc.cache" };

            ar.Save(outputFile);

            string strsm = sm.SerializeXml();

            var archive = DataArchive.Load(outputFile, "decompressedArchive");
        }

        public static void DataclonerTest1()
        {
            var dc = new DataCloner.Cloner();
            RowIdentifier source = new RowIdentifier();

            //Map serveur source / destination
            dc.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "sakila", Schema = "" },
                             new ServerIdentifier { ServerId = 1, Database = "sakila", Schema = "" });
            dc.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "employees", Schema = "" },
                             new ServerIdentifier { ServerId = 1, Database = "employees", Schema = "" });

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

        public static void ServerMapTest()
        {
            var sm = new ServersMapsXml();

            var r1 = new ServersMapsXml.RoadXml
            {
                ServerSrc = 3,
                DatabaseSrc = "db1",
                SchemaSrc = "",
                ServerDst = 1,
                DatabaseDst = "cloned",
                SchemaDst = ""
            };

            var r2 = new ServersMapsXml.RoadXml
            {
                ServerSrc = 2,
                DatabaseSrc = "db1",
                SchemaSrc = "",
                ServerDst = 1,
                DatabaseDst = "cloned",
                SchemaDst = ""
            };

            var map = new ServersMapsXml.MapXml
            {
                nameFrom = "PROD",
                nameTo = "UNE",
                Roads = new List<ServersMapsXml.RoadXml> { r1, r2 }
            };

            sm.Maps.Add(map);

            string strSM = sm.SerializeXml();
            var destrSM = strSM.DeserializeXml<ServersMapsXml>();

            sm.SaveXml("serversmaps.config");
            var smloaded = Extensions.LoadXml<ServersMapsXml>("serversmaps.config");
        }


#endif
    }
}