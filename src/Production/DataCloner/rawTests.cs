﻿using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.Archive;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using Connection = DataCloner.DataClasse.Cache.Connection;
using ForeignKeyColumn = DataCloner.DataClasse.Cache.ForeignKeyColumn;

namespace DataCloner
{
    public class EntryPoint
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
            const string outputFile = "compressed.dca";
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
                From = "PROD",
                To = "UNE",
                Roads = new List<Road> { r1, r2 }
            };

            //Cache
            var ct = new DatabasesSchema();
            var table = new TableSchema
            {
                Name = "table1",
                IsStatic = false,
                SelectCommand = "SELECT * FROM TABLE1",
                InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)"
            };

            table.ColumnsDefinition = table.ColumnsDefinition.Add(new ColumnDefinition
            {
                Name = "COL1",
                Type = DbType.Int32,
                IsPrimary = true,
                IsForeignKey = false,
                IsAutoIncrement = true,
                BuilderName = ""
            });

            table.ColumnsDefinition = table.ColumnsDefinition.Add(new ColumnDefinition
            {
                Name = "COL2",
                Type = DbType.Int32,
                IsPrimary = false,
                IsForeignKey = false,
                IsAutoIncrement = false,
                BuilderName = "Builder.NASBuilder"
            });

            table.DerivativeTables = table.DerivativeTables.Add(new DerivativeTable
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Forced,
                Cascade = true
            });

            table.DerivativeTables = table.DerivativeTables.Add(new DerivativeTable
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table3",
                Access = DerivativeTableAccess.Denied,
                Cascade = false
            });

            table.ForeignKeys = table.ForeignKeys.Add(new ForeignKey
            {
                ServerIdTo = 2,
                DatabaseTo = "db",
                SchemaTo = "dbo",
                TableTo = "TABLE2",
                Columns = new[] { new ForeignKeyColumn { NameFrom = "COL1", NameTo = "COL1" } }
            });

            ct.Add(1, "db1", "dbo", table);
            ct.Add(1, "db2", "dbo", table);

            var config = new Cache
            {
                ConnectionStrings =
                    new List<Connection> {new Connection {Id = 1, ConnectionString = "", ProviderName = ""}},
                ConfigFileHash = "",
                DatabasesSchema = ct
            };

            //Créaton de l'archive
            var ar = new DataArchive
            {
                Description = "aloll 1 un deux test test",
                Cache = config,
                OriginalQueries = new List<RowIdentifier>(),
                Databases = new List<string> {"System.Data.SQLite.xml", "dc.cache"}
            };

            ar.Save(outputFile);

            var strsm = map.SerializeXml();

            var archive = DataArchive.Load(outputFile, "decompressedArchive");
        }

        public static void DataclonerTest1()
        {
            var source = new RowIdentifier();

            var dc = new Cloner
            {
                Config = Configuration.Load(Configuration.ConfigFileName), 
                EnforceIntegrity = false
            };
            dc.Logger += Console.WriteLine;

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
            dc.Clone("TestApp", "testMySQL", "testMySQL", null, source, true);

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
                From = "PROD",
                To = "UNE",
                Roads = new List<Road> { r1, r2 }
            };

            var strSm = map.SerializeXml();
            var destrSm = strSm.DeserializeXml<Map>();

            map.SaveXml("serversmaps.config");
            var smloaded = Extensions.LoadXml<Map>("serversmaps.config");
        }

#endif
    }
}