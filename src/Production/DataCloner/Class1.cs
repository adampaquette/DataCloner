using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Class
{
   public class main
   {
      static int Main(string[] args)
      {
         var ti = new DataCloner.DataClasse.TableIdentifier();
         ti.DatabaseName = "botnet";
         ti.SchemaName = "botnet";
         ti.TableName = "link";

         var ri = new DataCloner.DataClasse.RowIdentifier();
         ri.TableIdentifier = ti;
         ri.Columns.Add("fromPageHostId", 6);
         ri.Columns.Add("fromPageId", 4);

         //var m = new DataCloner.DataAccess.QueryDatabaseMySQL("server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
         //var dt = m.GetFK(ti);
         //m.Select(ri);


         var conn = new System.Data.SqlClient.SqlConnection("Data Source=une_sql_pgis;Initial Catalog=PGISCBL;Integrated Security=SSPI;");
         conn.Open();
         if (conn.Database != "PGISCBL")
            conn.ChangeDatabase("PGISCBL");
         var dt = conn.GetSchema("Columns");

         conn.Close();

         return 0;
      }
   }
}