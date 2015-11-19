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


		public static void DataclonerTest1()
		{
			var source = new RowIdentifier();

		    var dc = new Cloner {EnforceIntegrity = false};
		    dc.StatusChanged += OnStatusChanged;

			var config = Configuration.ProjectContainer.Load("northWind.dcProj");
			
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