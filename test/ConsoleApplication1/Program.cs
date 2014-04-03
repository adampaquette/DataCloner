using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interface;
using Class;
using System.Xml.Serialization;
using Serialisation;

using System.Data.SQLite;


namespace ConsoleApplication1
{
   public class Program
   {
      static void Main(string[] args)
      {
         var conn = new SQLiteConnection(@"Data Source=C:\datacloner.db;Version=3;");
         conn.Open();


         using (SQLiteTransaction mytransaction = conn.BeginTransaction())
         {
            using (SQLiteCommand mycommand = new SQLiteCommand(conn))
            {
               SQLiteParameter myparam = new SQLiteParameter();
               int n;

               mycommand.CommandText = "DROP TABLE MyTable";
               mycommand.ExecuteNonQuery();
               mycommand.CommandText = "CREATE TABLE MyTable (Id INTEGER PRIMARY KEY ASC)";
               mycommand.ExecuteNonQuery();

               mycommand.CommandText = "INSERT INTO [MyTable] ([Id]) VALUES(?)";
               mycommand.Parameters.Add(myparam);

               for (n = 0; n < 100; n++)
               {
                  myparam.Value = n + 1;
                  mycommand.ExecuteNonQuery();
               }
            }
            mytransaction.Commit();
         }
         conn.Close();
/*
#define SQLITE_OK           0   // Successful result 
#define SQLITE_ERROR        1   // SQL error or missing database 
#define SQLITE_INTERNAL     2   // Internal logic error in SQLite 
#define SQLITE_PERM         3   // Access permission denied 
#define SQLITE_ABORT        4   // Callback routine requested an abort 
#define SQLITE_BUSY         5   // The database file is locked 
#define SQLITE_LOCKED       6   // A table in the database is locked 
#define SQLITE_NOMEM        7   // A malloc() failed 
#define SQLITE_READONLY     8   // Attempt to write a readonly database 
#define SQLITE_INTERRUPT    9   // Operation terminated by sqlite3_interrupt()
#define SQLITE_IOERR       10   // Some kind of disk I/O error occurred 
#define SQLITE_CORRUPT     11   // The database disk image is malformed 
#define SQLITE_NOTFOUND    12   // Unknown opcode in sqlite3_file_control() 
#define SQLITE_FULL        13   // Insertion failed because database is full 
#define SQLITE_CANTOPEN    14   // Unable to open the database file 
#define SQLITE_PROTOCOL    15   // Database lock protocol error 
#define SQLITE_EMPTY       16   // Database is empty 
#define SQLITE_SCHEMA      17   // The database schema changed 
#define SQLITE_TOOBIG      18   // String or BLOB exceeds size limit 
#define SQLITE_CONSTRAINT  19   // Abort due to constraint violation 
#define SQLITE_MISMATCH    20   // Data type mismatch 
#define SQLITE_MISUSE      21   // Library used incorrectly 
#define SQLITE_NOLFS       22   // Uses OS features not supported on host 
#define SQLITE_AUTH        23   // Authorization denied 
#define SQLITE_FORMAT      24   // Auxiliary database format error 
#define SQLITE_RANGE       25   // 2nd parameter to sqlite3_bind out of range 
#define SQLITE_NOTADB      26   // File opened that is not a database file 
#define SQLITE_NOTICE      27   // Notifications from sqlite3_log() 
#define SQLITE_WARNING     28   // Warnings from sqlite3_log() 
#define SQLITE_ROW         100  // sqlite3_step() has another row ready 
#define SQLITE_DONE        101  // sqlite3_step() has finished executing 
*/


         Console.ReadKey();
      }


      public void serial()
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
