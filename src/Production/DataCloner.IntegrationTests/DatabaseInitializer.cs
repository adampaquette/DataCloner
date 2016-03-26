using DataCloner.Core.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text.RegularExpressions;

namespace DataCloner.Core.IntegrationTests
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
            var conn = new SqlConnection(1)
            {
                ProviderName = "System.Data.SqlClient",
                ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;"
            };

            var provider = DbProviderFactories.GetFactory(conn.ProviderName);
            var c = provider.CreateConnection();
            c.ConnectionString = conn.ConnectionString;

            using (var cmd = c.CreateCommand())
            {
                var sql = File.ReadAllText(@"..\..\Chinook1.4\Chinook_SqlServer.sql");
                var batchs = Regex.Split(sql, "^GO", RegexOptions.Multiline);

                c.Open();
                foreach (var script in batchs)
                {
                    cmd.CommandText = script;
                    cmd.ExecuteNonQuery();
                }
                c.Close();
            }

            conn.ConnectionString += "Initial Catalog=Chinook;";

            return conn;
        }
    }
}
