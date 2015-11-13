using DataCloner.Configuration;
using DataCloner.Data;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DataCloner.IntegrationTests
{
    public class ClonerTests
    {
        private const string TestDatabase = "chinook";
        private const string TestSchema = "dbo";
        public static IEnumerable<object[]> SgbdToTest => DatabaseInitializer.Connections;

        [Theory(Skip = "Generation of the cache files"), MemberData("SgbdToTest")]
        public void Should_NotFail_When_Settuping(SqlConnection conn)
        {
            var cloner = new Cloner();
            cloner.Setup(Utils.MakeDefaultSettings(conn));
        }

        [Theory, MemberData("SgbdToTest")]
        public void Should_ReturnExpectedRows_When_CloningBasicData(SqlConnection conn)
        {
            //Arrange
            var cloner = new Cloner();
            cloner.Setup(Utils.MakeDefaultSettings(conn));

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "customer",
                Columns = new ColumnsWithValue
                {
                    { "customerid", 1 }
                }
            };

            var clonedData = new List<IRowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if(e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Clone(source, false);

            //Assert
            var expectedData = new List<IRowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "customer",
                     Columns = new ColumnsWithValue
                     {
                         { "customerid", 1 }
                     }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "employee",
                     Columns = new ColumnsWithValue
                     {
                         { "employeeid", 3 }
                     }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "employee",
                     Columns = new ColumnsWithValue
                     {
                         { "employeeid", 2 }
                     }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "employee",
                     Columns = new ColumnsWithValue
                     {
                         { "employeeid", 1 }
                     }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));
        }
    }
}
