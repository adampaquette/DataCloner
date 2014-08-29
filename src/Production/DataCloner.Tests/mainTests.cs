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
    public class mainTests
    {
        [Fact()]
        public static void CachedTableSerialization()
        {
            var ct = new CachedTables();
            var table1 = new TableDef();

            table1.Name = "table1";
            table1.IsStatic = false;
            table1.SelectCommand = "SELECT * FROM TABLE1";
            table1.InsertCommand = "INSERT INTO TABLE1 VALUES(@COL1, @COL2)";

            table1.SchemaColumns = table1.SchemaColumns.Add(new SchemaColumn()
            {
                Name = "COL1",
                Type = "INT",
                IsPrimary = true,
                IsForeignKey = false,
                IsAutoIncrement = true,
                BuilderName = ""
            });
            table1.SchemaColumns = table1.SchemaColumns.Add(new SchemaColumn()
            {
                Name = "COL2",
                Type = "INT",
                IsPrimary = false,
                IsForeignKey = false,
                IsAutoIncrement = false,
                BuilderName = "Builder.NASBuilder"
            });

            table1.DerivativeTables = table1.DerivativeTables.Add(new DerivativeTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Forced,
                Cascade = true
            });
            table1.DerivativeTables = table1.DerivativeTables.Add(new DerivativeTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table3",
                Access = DerivativeTableAccess.Denied,
                Cascade = false
            });

            table1.ForeignKeys = table1.ForeignKeys.Add(new ForeignKey()
            {
                ServerIdTo = 2,
                DatabaseTo = "db",
                SchemaTo = "dbo",
                TableTo = "TABLE2",
                Columns = new ForeignKeyColumn[] { new ForeignKeyColumn() { NameFrom = "COL1", NameTo = "COL1" } }
            });

            ct.Add(1, "db", "dbo", table1);

            MemoryStream ms1 = new MemoryStream();
            MemoryStream ms2 = new MemoryStream();

            //Test TableDef
            table1.Serialize(ms1);
            ms1.Position = 0;
            var output = TableDef.Deserialize(ms1);
            output.Serialize(ms2);

            Assert.True(ms1.ToArray().SequenceEqual(ms2.ToArray()));
                

            //Test cachedtables
            ms1 = new MemoryStream();
            ms2 = new MemoryStream();

            ct.Serialize(ms1);
            ms1.Position = 0;
            var outputCT = CachedTables.Deserialize(ms1);
            outputCT.Serialize(ms2);

            Assert.True(ms1.ToArray().SequenceEqual(ms2.ToArray()));               
        }
    }
}
