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
            var r1 = new Road
            {
                ServerSrc = 3,
                DatabaseSrc = "db1",
                SchemaSrc = "",
                ServerDst = 1,
                DatabaseDst = "cloned",
                SchemaDst = ""
            };

            var r2 = new Road
            {
                ServerSrc = 2,
                DatabaseSrc = "db1",
                SchemaSrc = "",
                ServerDst = 1,
                DatabaseDst = "cloned",
                SchemaDst = ""
            };

            var map = new Map
            {
                nameFrom = "PROD",
                nameTo = "UNE",
                Roads = new List<Road> { r1, r2 }
            };

            //Cache
            DatabasesSchema ct = new DatabasesSchema();
            TableSchema table = new TableSchema();

            table.Name = "table1";
            table.IsStatic = false;
            table.SelectCommand = "SELECT * FROM TABLE1";
            table.InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)";

            table.ColumnsDefinition = table.ColumnsDefinition.Add(new ColumnDefinition()
            {
                Name = "COL1",
                Type = DbType.Int32,
                IsPrimary = true,
                IsForeignKey = false,
                IsAutoIncrement = true,
                BuilderName = ""
            });
            table.ColumnsDefinition = table.ColumnsDefinition.Add(new ColumnDefinition()
            {
                Name = "COL2",
                Type = DbType.Int32,
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
                Columns = new DataCloner.DataClasse.Cache.ForeignKeyColumn[] { new DataCloner.DataClasse.Cache.ForeignKeyColumn() { NameFrom = "COL1", NameTo = "COL1" } }
            });

            ct.Add(1, "db1", "dbo", table);
            ct.Add(1, "db2", "dbo", table);

            Cache config = new Cache();
            config.ConnectionStrings = new List<DataCloner.DataClasse.Cache.Connection> { new DataCloner.DataClasse.Cache.Connection { Id = 1, ConnectionString = "", ProviderName = ""} };
            config.ConfigFileHash = "";
            config.DatabasesSchema = ct;

            //Créaton de l'archive
            var ar = new DataArchive();
            ar.Description = "aloll 1 un deux test test";
            ar.Cache = config;
            ar.OriginalQueries = new List<RowIdentifier>();
            ar.Databases = new List<string> { "System.Data.SQLite.xml", "dc.cache" };

            ar.Save(outputFile);

            string strsm = map.SerializeXml();

            var archive = DataArchive.Load(outputFile, "decompressedArchive");
        }

        public static void DataclonerTest1()
        {
            var dc = new Cloner();
            RowIdentifier source = new RowIdentifier();

            //Map serveur source / destination
            dc.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "dataclonertestdatabase", Schema = "" },
                             new ServerIdentifier { ServerId = 1, Database = "dataclonertestdatabase", Schema = "" });
            dc.ServerMap.Add(new ServerIdentifier { ServerId = 1, Database = "sakila", Schema = "" },
                             new ServerIdentifier { ServerId = 1, Database = "sakila", Schema = "" });

            dc.Initialize();
            dc.Logger += msg => Console.WriteLine(msg);
            dc.EnforceIntegrity = false;

            //source.Columns.Clear();
            //source.ServerId = 1;
            //source.Database = "dataclonertestdatabase";
            //source.Schema = "";
            //source.Table = "employees";
            //source.Columns.Add("employeeNumber", 1188);
            //dc.SqlTraveler(source, true);

            //Console.WriteLine("==============");

            source.Columns.Clear();
            source.ServerId = 1;
            source.Database = "sakila";
            source.Schema = "";
            source.Table = "customer";
            source.Columns.Add("customer_id", 1);
            dc.SqlTraveler(source, true);

            //Console.WriteLine("==============");

            //source.Columns.Clear();
            //source.ServerId = 1;
            //source.Database = "dataclonertestdatabase";
            //source.Schema = "";
            //source.Table = "employees";
            //source.Columns.Add("employeeNumber", 1370);
            //dc.SqlTraveler(source, true);
        }

        public static void ServerMapTest()
        {
            var r1 = new Road
            {
                ServerSrc = 3,
                DatabaseSrc = "db1",
                SchemaSrc = "",
                ServerDst = 1,
                DatabaseDst = "cloned",
                SchemaDst = ""
            };

            var r2 = new Road
            {
                ServerSrc = 2,
                DatabaseSrc = "db1",
                SchemaSrc = "",
                ServerDst = 1,
                DatabaseDst = "cloned",
                SchemaDst = ""
            };

            var map = new Map
            {
                nameFrom = "PROD",
                nameTo = "UNE",
                Roads = new List<Road> { r1, r2 }
            };

            string strSM = map.SerializeXml();
            var destrSM = strSM.DeserializeXml<Map>();

            map.SaveXml("serversmaps.config");
            var smloaded = Extensions.LoadXml<Map>("serversmaps.config");
        }

#endif
    }
}