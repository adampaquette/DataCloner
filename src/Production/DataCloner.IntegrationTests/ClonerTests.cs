using DataCloner.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Xunit;
using Xunit.Extensions;

namespace DataCloner.IntegrationTests
{
    public class DatabaseFixture
    {
        public List<SqlConnection> Connections { get; set; }

        public DatabaseFixture()
        {
            Connections = new List<SqlConnection>
            {
                CreateSqlServer()
            };
        }

        private SqlConnection CreateSqlServer()
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

            return conn;
        }
    }


    public class ClonerTests : IClassFixture<DatabaseFixture>
    {
        private DatabaseFixture _fixture;

        public ClonerTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public static IEnumerable<object[]> SgbdToTest
        {
            get
            {
                return new[]
                {
                    new object[] {1},
                    new object[] {2}
                };
            }
        }


        [Theory, MemberData("SgbdToTest")]
        public void asd(int sqlConnectionId)
        {
            Assert.Equal("1", _fixture.Connections.Count.ToString());
        }
    }
}
