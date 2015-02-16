using System.Data;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using NSubstitute;

namespace DataCloner.Tests
{
    internal class FakeBasicDatabase
    {
        internal static Cache CreateCache()
        {
            var city = new TableSchema
            {
                Name = "city",
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, Type = DbType.Int32},
                    new ColumnDefinition {Name = "name", Type = DbType.String, Size = "20"}
                }
            };

            var state = new TableSchema
            {
                Name = "state",
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, Type = DbType.Int32},
                    new ColumnDefinition {Name = "name", Type = DbType.String, Size = "20"},
                    new ColumnDefinition {Name = "cityId", IsForeignKey = true, Type = DbType.Int32}
                },
                ForeignKeys = new[]
                {
                    new ForeignKey
                    {
                        ServerIdTo = 0,
                        DatabaseTo = "",
                        SchemaTo = "",
                        TableTo = "city",
                        Columns = new []{new ForeignKeyColumn {NameFrom = "cityId", NameTo = "id"} }
                    }
                },
                DerivativeTables = new[] { new DerivativeTable { ServerId = 0, Schema = "", Database = "", Table = "city" } }
            };

            var country = new TableSchema
            {
                Name = "country",
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, Type = DbType.Int32},
                    new ColumnDefinition {Name = "name", Type = DbType.String, Size = "20"},
                    new ColumnDefinition {Name = "stateId", IsForeignKey = true, Type = DbType.Int32}
                },
                ForeignKeys = new[]
                {
                    new ForeignKey
                    {
                        ServerIdTo = 0,
                        DatabaseTo = "",
                        SchemaTo = "",
                        TableTo = "state",
                        Columns = new []{new ForeignKeyColumn {NameFrom = "stateId", NameTo = "id"} }
                    }
                },
                DerivativeTables = new[] { new DerivativeTable { ServerId = 0, Schema = "", Database = "", Table = "contry" } }
            };

            var cache = new Cache();
            cache.ServerMap.Add(new ServerIdentifier { Database = "", Schema = "" },
                new ServerIdentifier { Database = "", Schema = "" });
            cache.DatabasesSchema.Add(0, "", "", city);
            cache.DatabasesSchema.Add(0, "", "", state);
            cache.DatabasesSchema.Add(0, "", "", country);

            return cache;
        }

        internal static IQueryHelper CreateData()
        {
            var queryHelper = Substitute.For<IQueryHelper>();
            queryHelper.Select(Tools.NewRi(0, "", "", "customer", new ColumnsWithValue { { "id", 1 } }))
                .Returns(new[] { new object[] { 1, 2, 3 } });

            return queryHelper;
        }

        internal static IQueryDispatcher CreateServer(IQueryHelper queryHelper)
        {
            var queryDispatcher = Substitute.For<IQueryDispatcher>();
            queryDispatcher.GetQueryHelper(0).Returns(queryHelper);
            queryDispatcher.GetQueryHelper(Arg.Is<IRowIdentifier>(r => r.ServerId == 0)).Returns(queryHelper);
            return queryDispatcher;
        }
    }
}