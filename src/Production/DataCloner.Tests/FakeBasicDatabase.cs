using System.Data;
using DataCloner.Data;
using DataCloner.Internal;
using DataCloner.Metadata;
using NSubstitute;

namespace DataCloner.Tests
{
    internal class FakeBasicDatabase
    {
        internal static MetadataContainer CreateDatabaseSchema()
        {
            var house = new TableMetadata("house")
            {
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, DbType = DbType.Int32},
                    new ColumnDefinition {Name = "name", DbType = DbType.String, SqlType = new SqlType{ Precision = 20 } },
                    new ColumnDefinition {Name = "cityId", IsForeignKey = true, DbType = DbType.Int32}
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
                }
            };

            var city = new TableMetadata("city")
            {
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, DbType = DbType.Int32},
                    new ColumnDefinition {Name = "name", DbType = DbType.String,  SqlType = new SqlType{ Precision = 20 }},
                    new ColumnDefinition {Name = "stateId", IsForeignKey = true, DbType = DbType.Int32}
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
                DerivativeTables = new[] { new DerivativeTable { ServerId = 0, Schema = "", Database = "", Table = "house" } }
            };

            var state = new TableMetadata("state")
            {
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, DbType = DbType.Int32},
                    new ColumnDefinition {Name = "name", DbType = DbType.String,  SqlType = new SqlType{ Precision = 20 }},
                    new ColumnDefinition {Name = "countryId", IsForeignKey = true, DbType = DbType.Int32}
                },
                ForeignKeys = new[]
                {
                    new ForeignKey
                    {
                        ServerIdTo = 0,
                        DatabaseTo = "",
                        SchemaTo = "",
                        TableTo = "country",
                        Columns = new []{new ForeignKeyColumn {NameFrom = "countryId", NameTo = "id"} }
                    }
                },
                DerivativeTables = new[] { new DerivativeTable { ServerId = 0, Schema = "", Database = "", Table = "city" } }
            };

            var country = new TableMetadata("country")
            {
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, DbType = DbType.Int32},
                    new ColumnDefinition {Name = "name", DbType = DbType.String, SqlType = new SqlType{ Precision = 20 }}
                },
                DerivativeTables = new[]
                {
                    new DerivativeTable { ServerId = 0, Schema = "", Database = "", Table = "state" },
                    new DerivativeTable { ServerId = 0, Schema = "", Database = "", Table = "person" }
                }
            };

            var person = new TableMetadata("person")
            {
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, DbType = DbType.Int32},
                    new ColumnDefinition {Name = "name", DbType = DbType.String,  SqlType = new SqlType{ Precision = 20 }},
                    new ColumnDefinition {Name = "fatherId", IsForeignKey = true, IsUniqueKey = true, DbType = DbType.Int32},
                    new ColumnDefinition {Name = "favoriteColorId", IsForeignKey = true, DbType = DbType.Int32}
                },
                ForeignKeys = new[]
                {
                    new ForeignKey
                    {
                        ServerIdTo = 0,
                        DatabaseTo = "",
                        SchemaTo = "",
                        TableTo = "color",
                        Columns = new []{new ForeignKeyColumn {NameFrom = "favoriteColorId", NameTo = "id"} }
                    },
                    new ForeignKey
                    {
                        ServerIdTo = 0,
                        DatabaseTo = "",
                        SchemaTo = "",
                        TableTo = "person",
                        Columns = new []{new ForeignKeyColumn {NameFrom = "fatherId", NameTo = "id"} }
                    }
                }
            };

            var color = new TableMetadata("color")
            {
                ColumnsDefinition = new[]
                {
                    new ColumnDefinition {Name = "id", IsPrimary = true, DbType = DbType.Int32},
                    new ColumnDefinition {Name = "name", DbType = DbType.String,  SqlType = new SqlType{ Precision = 20 }}
                },
                IsStatic = true
            };

            var cache = new MetadataContainer();
            cache.ServerMap.Add(new ServerIdentifier { Database = "", Schema = "" },
                                new ServerIdentifier { Database = "", Schema = "" });
            cache.Metadatas.Add(0, "", "", house);
            cache.Metadatas.Add(0, "", "", city);
            cache.Metadatas.Add(0, "", "", state);
            cache.Metadatas.Add(0, "", "", country);
            cache.Metadatas.Add(0, "", "", person);
            cache.Metadatas.Add(0, "", "", color);

            return cache;
        }

        internal static IQueryHelper CreateData()
        {
            var db = Substitute.For<IQueryHelper>();
            db.Select(Make.Ri0("color", new ColumnsWithValue { { "id", 1 } })).Returns(Make.Obj(1, "orange"));
            db.Select(Make.Ri0("person", new ColumnsWithValue { { "id", 1 } })).Returns(Make.Obj(1, "joe", 1, 1));
            db.Select(Make.Ri0("country", new ColumnsWithValue { { "id", 1 } })).Returns(Make.Obj(1));
            db.Select(Make.Ri0("state", new ColumnsWithValue { { "id", 1 } })).Returns(Make.Obj(1));
            db.Select(Make.Ri0("city", new ColumnsWithValue { { "id", 1 } })).Returns(Make.Obj(1));
            db.Select(Make.Ri0("house", new ColumnsWithValue { { "id", 1 } })).Returns(Make.Obj(1));

            return db;
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