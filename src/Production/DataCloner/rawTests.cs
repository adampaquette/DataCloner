using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using DataCloner.Archive;
using DataCloner.Internal;
using DataCloner.Metadata;
using DataCloner.Configuration;
using DataCloner.Framework;
using ForeignKeyColumn = DataCloner.Metadata.ForeignKeyColumn;
using DataCloner.Data;

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
			//Console.ReadKey();
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
				ServerSrc = "3",
				DatabaseSrc = "db1",
				SchemaSrc = "",
				ServerDst = "1",
				DatabaseDst = "cloned",
				SchemaDst = ""
			};

			var r2 = new Road
			{
				ServerSrc = "2",
				DatabaseSrc = "db1",
				SchemaSrc = "",
				ServerDst = "1",
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
			var ct = new MetadataPerServer();
			var table = new TableMetadata("table1")
			{
				IsStatic = false,
				SelectCommand = "SELECT * FROM TABLE1",
				InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)"
			};

			table.ColumnsDefinition = table.ColumnsDefinition.Add(new ColumnDefinition
			{
				Name = "COL1",
				DbType = DbType.Int32,
				IsPrimary = true,
				IsForeignKey = false,
				IsAutoIncrement = true,
				BuilderName = ""
			});

			table.ColumnsDefinition = table.ColumnsDefinition.Add(new ColumnDefinition
			{
				Name = "COL2",
				DbType = DbType.Int32,
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

			var config = new MetadataContainer
			{
				ConnectionStrings =
					new List<SqlConnection> { new SqlConnection { Id = 1, ConnectionString = "", ProviderName = "" } },
				ConfigFileHash = "",
				Metadatas = ct
			};

			//Créaton de l'archive
			var ar = new DataArchive
			{
				Description = "aloll 1 un deux test test",
				Cache = config,
				OriginalQueries = new List<RowIdentifier>(),
				Databases = new List<string> { "System.Data.SQLite.xml", "dc.cache" }
			};

			ar.Save(outputFile);

			var strsm = map.SerializeXml();

			var archive = DataArchive.Load(outputFile, "decompressedArchive");
		}

		public static void DataclonerTest1()
		{
			var source = new RowIdentifier();

		    var dc = new Cloner {EnforceIntegrity = false};
		    dc.StatusChanged += OnStatusChanged;

			var config = Configuration.ConfigurationContainer.Load(Configuration.ConfigurationContainer.ConfigFileName);
			
			var start = DateTime.Now;

			Console.WriteLine("Cloning started");

			//dc.Setup(config.Applications[0], 1, 1);
			//source.Columns.Clear();
			//source.ServerId = 1;
			//source.Database = "dataclonertestdatabase";
			//source.Schema = "";
			//source.Table = "customers";
			//source.Columns.Add("customerNumber", (int)103);
			//dc.Clone(source, true);

			//dc.Setup(config.Applications[0], 1, null);
			//source.Columns.Clear();
			//source.ServerId = 1;
			//source.Database = "dataclonertestdatabase";
			//source.Schema = "";
			//source.Table = "customers";
			//source.Columns.Add("customerNumber", (int)103);
			//dc.Clone(source, true);

			//source.Columns.Clear();
			//source.ServerId = 1;
			//source.Database = "dataclonertestdatabase";
			//source.Schema = "";
			//source.Table = "employees";
			//source.Columns.Add("employeeNumber", (int)1188);
			//dc.Clone(source, true);

			//Console.WriteLine("Done");

			source.Columns.Clear();
			source.ServerId = 1;
			source.Database = "northwind";
			source.Schema = "dbo";
			source.Table = "customers";
			source.Columns.Add("customerId", "alfki");
			dc.Clone(source, true);

			////Référence circulaire basic
			//source.Columns.Clear();
			//source.ServerId = 1;
			//source.Database = "sakila";
			//source.Schema = "";
			//source.Table = "store";
			//source.Columns.Add("store_id", (byte)1);
			//dc.Clone(source, false);

			//Référence circulaire + big data
			//source.Columns.Clear();
			//source.ServerId = 1;
			//source.Database = "sakila";
			//source.Schema = "";
			//source.Table = "store";
			//source.Columns.Add("store_id", (byte)1);
			//dc.Clone(source, true);


			//Console.WriteLine("Done");

			//source.Columns.Clear();
			//source.ServerId = 1;
			//source.Database = "dataclonertestdatabase";
			//source.Schema = "";
			//source.Table = "employees";
			//source.Columns.Add("employeeNumber", (int)1370);
			//dc.Clone(source, true);

			Console.WriteLine("Cloning completed in : " +
							  DateTime.Now.Subtract(start).ToString("hh':'mm':'ss'.'fff"));
		}

		private static void OnStatusChanged(object s, StatusChangedEventArgs e)
		{
			if (e.Status == Status.Cloning)
			{
				var sb = new StringBuilder();
				sb.Append(new string(' ', 3*e.Level));
				sb.Append(e.SourceRow.Database).Append(".").Append(e.SourceRow.Schema).Append(".").Append(e.SourceRow.Table).Append(" : (");
				foreach (var col in e.SourceRow.Columns)
					sb.Append(col.Key).Append("=").Append(col.Value).Append(", ");
				sb.Remove(sb.Length - 2, 2);
				sb.Append(")");
				Console.WriteLine(sb.ToString());
			}
			else if (e.Status == Status.FetchingDerivatives)
			{
				Console.WriteLine(new string(' ', 3*e.Level) + "=================================");
			}
		}

		public static void ServerMapTest()
		{
			var r1 = new Road
			{
				ServerSrc = "3",
				DatabaseSrc = "db1",
				SchemaSrc = "",
				ServerDst = "1",
				DatabaseDst = "cloned",
				SchemaDst = ""
			};

			var r2 = new Road
			{
				ServerSrc = "2",
				DatabaseSrc = "db1",
				SchemaSrc = "",
				ServerDst = "1",
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