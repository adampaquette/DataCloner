using DataCloner.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;

namespace DataCloner.IntegrationTests
{
    public static class DatabaseInitializer
    {
        public static List<object[]> Connections { get; set; }

        static DatabaseInitializer()
        {
            Connections = new List<object[]>
            {
                new object[] { CreateSqlServer()  }
            };
        }

        private static SqlConnection CreateSqlServer()
        {
            var conn = new SqlConnection
            {
                Id = 1,
                ProviderName = "System.Data.SqlClient",
                ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;"
            };

            var provider = DbProviderFactories.GetFactory(conn.ProviderName);
            var c = provider.CreateConnection();
            c.ConnectionString = conn.ConnectionString;

            using (var cmd = c.CreateCommand())
            {
                var sql = File.ReadAllText(@"..\..\Chinook1.4\Chinook_SqlServer.sql");
                cmd.CommandText = sql;

                c.Open();
                cmd.ExecuteNonQuery();
                c.Close();
            }

            conn.ConnectionString += "Initial Catalog=Chinook;";

            return conn;
        }
    }
}
