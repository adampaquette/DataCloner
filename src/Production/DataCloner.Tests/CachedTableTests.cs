using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DataCloner;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using DataCloner.Enum;

using Class;
using Xunit;

namespace DataCloner.Tests
{
    public class CachedTableTests
    {
        private CachedTables _cache;
        private TableDef _table;

        public CachedTableTests()
        { 
            _cache = new CachedTables();
            _table = new TableDef();

            _table.Name = "table1";
            _table.IsStatic = false;
            _table.SelectCommand = "SELECT * FROM TABLE1";
            _table.InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)";

            _table.SchemaColumns = _table.SchemaColumns.Add(new SchemaColumn()
            {
                Name = "COL1",
                Type = "INT",
                IsPrimary = true,
                IsForeignKey = false,
                IsAutoIncrement = true,
                BuilderName = ""
            });
            _table.SchemaColumns = _table.SchemaColumns.Add(new SchemaColumn()
            {
                Name = "COL2",
                Type = "INT",
                IsPrimary = false,
                IsForeignKey = false,
                IsAutoIncrement = false,
                BuilderName = "Builder.NASBuilder"
            });

            _table.DerivativeTables = _table.DerivativeTables.Add(new DerivativeTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Forced,
                Cascade = true
            });
            _table.DerivativeTables = _table.DerivativeTables.Add(new DerivativeTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table3",
                Access = DerivativeTableAccess.Denied,
                Cascade = false
            });

            _table.ForeignKeys = _table.ForeignKeys.Add(new ForeignKey()
            {
                ServerIdTo = 2,
                DatabaseTo = "db",
                SchemaTo = "dbo",
                TableTo = "TABLE2",
                Columns = new ForeignKeyColumn[] { new ForeignKeyColumn() { NameFrom = "COL1", NameTo = "COL1" } }
            });

            _cache.Add(1, "db1", "dbo", _table);
            _cache.Add(1, "db2", "dbo", _table);
        }

        [Fact()]
        public void TableDefBinarySerialization()
        {
            MemoryStream ms1 = new MemoryStream();
            MemoryStream ms2 = new MemoryStream();

            _table.Serialize(ms1);
            ms1.Position = 0;
            var output = TableDef.Deserialize(ms1);
            output.Serialize(ms2);

            Assert.True(ms1.ToArray().SequenceEqual(ms2.ToArray()));           
        }

        [Fact()]
        public void CachedTablesBinarySerialization()
        {
            MemoryStream ms1 = new MemoryStream();
            MemoryStream ms2 = new MemoryStream();

            _cache.Serialize(ms1);
            ms1.Position = 0;
            var output = CachedTables.Deserialize(ms1);
            output.Serialize(ms2);

            Assert.True(ms1.ToArray().SequenceEqual(ms2.ToArray()));
        }
    }
}
