using DataCloner.Core.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DataCloner.Core.Configuration;

namespace DataCloner.Core.IntegrationTests
{
    public static class DatabaseInitializer
    {
        private const int NB_STATEMENTS_PER_QUERY = 1000;

        public static IEnumerable<object[]> Connections { get; set; }

        static DatabaseInitializer()
        {
            Connections = new List<object[]>
            {
                new object[] { CreateMsSqlDatabase() },
                new object[] { CreateMsSqlDatabaseAutoIncrement() },
                new object[] { CreatePostgreSqlDatabase() },
                new object[] { CreateMySqlDatabase() },
                new object[] { CreateMySqlDatabaseAutoIncrement() }
            };
        }

        private static Connection CreateMsSqlDatabase()
        {
            var conn = new Connection()
            {
                Id = 1,
                ProviderName = "System.Data.SqlClient",
                ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;"
            };

            //Create DB
            //var provider = DbProviderFactories.GetFactory(conn.ProviderName);
            //var c = provider.CreateConnection();
            //c.ConnectionString = conn.ConnectionString;

            //using (var cmd = c.CreateCommand())
            //{
            //    var sql = File.ReadAllText(@"..\..\Chinook1.4\Chinook_SqlServer.sql");
            //    var batchs = Regex.Split(sql, "^GO", RegexOptions.Multiline);

            //    c.Open();
            //    foreach (var script in batchs)
            //    {
            //        cmd.CommandText = script;
            //        cmd.ExecuteNonQuery();
            //    }
            //    c.Close();
            //}

            conn.ConnectionString += "Initial Catalog=Chinook;";

            return conn;
        }

        private static Connection CreateMsSqlDatabaseAutoIncrement()
        {
            var conn = new Connection()
            {
                Id = 2,
                ProviderName = "System.Data.SqlClient",
                ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Integrated Security=True;Initial Catalog=ChinookAI;"
            };

            return conn;
        }

        private static Connection CreateMySqlDatabase()
        {
            var conn = new Connection()
            {
                Id = 1,
                ProviderName = "MySql.Data.MySqlClient",
                ConnectionString = @"Server=localhost;Uid=root;Pwd=toor;default command timeout=120;Allow User Variables=True"
            };

            //Create DB
            //var provider = DbProviderFactories.GetFactory(conn.ProviderName);
            //var c = provider.CreateConnection();
            //c.ConnectionString = conn.ConnectionString;

            //using (var cmd = c.CreateCommand())
            //{
            //    var sql = File.ReadAllText(@"..\..\Chinook1.4\Chinook_MySql.sql");
            //    var statements = Regex.Split(sql, @"\;$", RegexOptions.Multiline);

            //    c.Open();

            //    cmd.CommandText = sql;
            //    cmd.ExecuteNonQuery();

            //    c.Close();
            //}

            return conn;
        }

        private static Connection CreateMySqlDatabaseAutoIncrement()
        {
            var conn = new Connection()
            {
                Id = 2,
                ProviderName = "MySql.Data.MySqlClient",
                ConnectionString = @"Server=localhost;Uid=root;Pwd=toor;Allow User Variables=True"
            };

            return conn;
        }

        private static Connection CreatePostgreSqlDatabase()
        {
            var conn = new Connection()
            {
                Id = 1,
                ProviderName = "Npgsql",
                ConnectionString = @"Host=localhost;Username=postgres;Password=toor;Database=Chinook"
            };

            return conn;
        }
    }
}