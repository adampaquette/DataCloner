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

         ci.ServerName = "127.0.0.1";
         ci.DatabaseName = "db";
         ci.SchemaName = "dbo";
         ci.TableName = "table";
         //ci.ColumnName = "tbl";

         ti.ServerName = "127.0.0.1";
         ti.DatabaseName = "db";
         ti.SchemaName = "dbo";
         ti.TableName = "table";

         ti2.ServerName = "localhost";
         ti2.DatabaseName = "myDB";
         ti2.SchemaName = "dbo";
         ti2.TableName = "profil";

         //ti = ci;

         Interface.IStaticTableDictionnary std = new Class.StaticTableDictionnary();
         std.Add(ti, true);
         //std.Add(ti2, true);
         std.Add(ci, true);

         var tables = std.Select(kv => new StaticTable()
         {
            ServerName = kv.Key.ServerName,
            DatabaseName = kv.Key.DatabaseName,
            SchemaName = kv.Key.SchemaName,
            TableName = kv.Key.TableName,
            Active = kv.Value
         }).ToList();

         var staticTables = new Config();
         staticTables.StaticTables = tables;
         Console.WriteLine(SerizlizeXML(staticTables));
         Console.WriteLine();


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
