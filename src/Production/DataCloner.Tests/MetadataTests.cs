using System.Data;
using System.IO;
using System.Linq;
using DataCloner.Metadata;
using DataCloner.Framework;
using Xunit;

namespace DataCloner.Tests
{
    public class MetadataTests
    {
        private readonly AppMetadata _appMD;
        private readonly TableMetadata _tableMD;

        public MetadataTests()
        {
            _appMD = new AppMetadata();
            _tableMD = new TableMetadata("table1")
            {
                IsStatic = false,
                SelectCommand = "SELECT * FROM TABLE1",
                InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)"
            };

            _tableMD.ColumnsDefinition = _tableMD.ColumnsDefinition.Add(new ColumnDefinition
            {
                Name = "COL1",
                DbType = DbType.Int32,
                IsPrimary = true,
                IsForeignKey = false,
                IsAutoIncrement = true,
                BuilderName = "",
            });

            _tableMD.ColumnsDefinition = _tableMD.ColumnsDefinition.Add(new ColumnDefinition
            {
                Name = "COL2",
                DbType = DbType.Int32,
                IsPrimary = false,
                IsForeignKey = false,
                IsAutoIncrement = false,
                BuilderName = "Builder.NASBuilder",
                IsUniqueKey = true,
                SqlType = new Data.SqlType
                {
                    DataType = "String",
                    IsUnsigned = true,
                    Precision = 3,
                    Scale = 5
                }
            });

            _tableMD.DerivativeTables = _tableMD.DerivativeTables.Add(new DerivativeTable
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Forced,
                Cascade = true
            });

            _tableMD.DerivativeTables = _tableMD.DerivativeTables.Add(new DerivativeTable
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table3",
                Access = DerivativeTableAccess.Denied,
                Cascade = false
            });

            _tableMD.ForeignKeys = _tableMD.ForeignKeys.Add(new ForeignKey
            {
                ServerIdTo = 2,
                DatabaseTo = "db",
                SchemaTo = "dbo",
                TableTo = "TABLE2",
                Columns = new[] { new ForeignKeyColumn { NameFrom = "COL1", NameTo = "COL1" } }
            });

            _appMD.Add(1, "db1", "dbo", _tableMD);
            _appMD.Add(1, "db2", "dbo", _tableMD);
        }

        [Fact]
        public void Should_Equal_When_SerializingTheSameTableMetaData()
        {
            var ms1 = new MemoryStream();
            var ms2 = new MemoryStream();

            _tableMD.Serialize(ms1);
            ms1.Position = 0;
            var output = TableMetadata.Deserialize(ms1);
            output.Serialize(ms2);

            Assert.True(ms1.ToArray().SequenceEqual(ms2.ToArray()));
        }

        [Fact]
        public void Should_Equal_When_SerializingTheSameAppMetadata()
        {
            var ms1 = new MemoryStream();
            var ms2 = new MemoryStream();

            _appMD.Serialize(ms1);
            ms1.Position = 0;
            var output = AppMetadata.Deserialize(ms1);
            output.Serialize(ms2);

            Assert.True(ms1.ToArray().SequenceEqual(ms2.ToArray()));
        }
    }
}
