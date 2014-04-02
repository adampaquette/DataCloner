using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interface;
using Class;
using System.Xml.Serialization;
using Serialisation;


namespace ConsoleApplication1
{
   public class Program
   {
      static void Main(string[] args)
      {
         Interface.IColumnIdentifier ci = new Class.ColumnIdentifier();
         Interface.ITableIdentifier ti = new Class.TableIdentifier();
         Interface.ITableIdentifier ti2 = new Class.TableIdentifier();

         ci.ConnStringID = 0;
         ci.DatabaseName = "db";
         ci.SchemaName = "dbo";
         ci.TableName = "table";
         ci.ColumnName = "tbl";

         ti.ConnStringID = 2;
         ti.DatabaseName = "db";
         ti.SchemaName = "dbo";
         ti.TableName = "table";

         ti2.ConnStringID = 1;
         ti2.DatabaseName = "db";
         ti2.SchemaName = "dbo";
         ti2.TableName = "table";

         Interface.IStaticTableDictionnary std = new Class.StaticTableDictionnary();
         std.Add(ti, true);
         std.Add(ti2, true);
         std.Add(ci, false);

         var tables = std.Select(kv => new StaticTable()
         {
            ConnStringID = kv.Key.ConnStringID,
            Database = kv.Key.DatabaseName,
            Schema = kv.Key.SchemaName,
            Table = kv.Key.TableName,
            Active = kv.Value
         }).ToList();

         var staticTables = new Config();
         staticTables.StaticTables = tables;
         Console.WriteLine(SerizlizeXML(staticTables));
         Console.WriteLine();


         var c = new System.Data.SqlClient.SqlConnection();




         Console.ReadKey();
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
