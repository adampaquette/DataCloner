using System.Data;
using System.IO;
using System.Linq;
using DataCloner.DataClasse.Cache;
using DataCloner.Framework;
using Xunit;

namespace DataCloner.Tests
{
    public class CachedTableTests
    {
        private readonly DatabasesSchema _cache;
        private readonly TableSchema _table;

        public CachedTableTests()
        { 
            _cache = new DatabasesSchema();
            _table = new TableSchema
            {
                Name = "table1",
                IsStatic = false,
                SelectCommand = "SELECT * FROM TABLE1",
                InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)"
            };

            _table.ColumnsDefinition = _table.ColumnsDefinition.Add(new ColumnDefinition
            {
                Name = "COL1",
                Type = DbType.Int32,
                IsPrimary = true,
                IsForeignKey = false,
                IsAutoIncrement = true,
                BuilderName = ""
            });

            _table.ColumnsDefinition = _table.ColumnsDefinition.Add(new ColumnDefinition
            {
                Name = "COL2",
                Type = DbType.Int32,
                IsPrimary = false,
                IsForeignKey = false,
                IsAutoIncrement = false,
                BuilderName = "Builder.NASBuilder"
            });

            _table.DerivativeTables = _table.DerivativeTables.Add(new DerivativeTable
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Forced,
                Cascade = true
            });

            _table.DerivativeTables = _table.DerivativeTables.Add(new DerivativeTable
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table3",
                Access = DerivativeTableAccess.Denied,
                Cascade = false
            });

            _table.ForeignKeys = _table.ForeignKeys.Add(new ForeignKey
            {
                ServerIdTo = 2,
                DatabaseTo = "db",
                SchemaTo = "dbo",
                TableTo = "TABLE2",
                Columns = new[] { new ForeignKeyColumn { NameFrom = "COL1", NameTo = "COL1" } }
            });

            _cache.Add(1, "db1", "dbo", _table);
            _cache.Add(1, "db2", "dbo", _table);
        }

        [Fact]
        public void TableDefBinarySerialization()
        {
            var ms1 = new MemoryStream();
            var ms2 = new MemoryStream();

            _table.Serialize(ms1);
            ms1.Position = 0;
            var output = TableSchema.Deserialize(ms1);
            output.Serialize(ms2);

            Assert.True(ms1.ToArray().SequenceEqual(ms2.ToArray()));           
        }

        [Fact]
        public void CachedTablesBinarySerialization()
        {
            var ms1 = new MemoryStream();
            var ms2 = new MemoryStream();

            _cache.Serialize(ms1);
            ms1.Position = 0;
            var output = DatabasesSchema.Deserialize(ms1);
            output.Serialize(ms2);

            Assert.True(ms1.ToArray().SequenceEqual(ms2.ToArray()));
        }
    }
}
