using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.IO;

using DataCloner.Framework;
using DataCloner.Framework.GeneralExtensionHelper;
using DataCloner.Enum;
using DataCloner.DataAccess;

namespace DataCloner.DataClasse.Cache
{
    internal sealed class TableDef : ITableDef
    {
        public string Name { get; set; }
        public bool IsStatic { get; set; }
        public string SelectCommand { get; set; }
        public string InsertCommand { get; set; }
        public IDerivativeTable[] DerivativeTables { get; set; }
        public IForeignKey[] ForeignKeys { get; set; }
        public ISchemaColumn[] SchemaColumns { get; set; }

        public TableDef()
        {
            DerivativeTables = new DerivativeTable[] { };
            ForeignKeys = new ForeignKey[] { };
            SchemaColumns = new SchemaColumn[] { };
        }

        public object[] BuildRawFKFromDataRow(IForeignKey fk, object[] row)
        {
            var pk = new List<object>();
            for (int j = 0; j < fk.Columns.Length; j++)
            {
                int posTblSource = SchemaColumns.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                pk.Add(row[posTblSource]);
            }
            return pk.ToArray();
        }

        public Dictionary<string, object> BuildFKFromDataRow(IForeignKey fk, object[] row)
        {
            var colFK = new Dictionary<string, object>();
            for (int j = 0; j < fk.Columns.Length; j++)
            {
                int posTblSource = SchemaColumns.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                colFK.Add(fk.Columns[j].NameTo, row[posTblSource]);
            }
            return colFK;
        }

        public object[] BuildRawPKFromDataRow(object[] row)
        {
            var pk = new List<object>();
            for (int i = 0; i < SchemaColumns.Length; i++)
            {
                if (SchemaColumns[i].IsPrimary)
                    pk.Add(row[i]);
            }
            return pk.ToArray();
        }

        public Dictionary<string, object> BuildPKFromDataRow(object[] row)
        {
            var pk = new Dictionary<string, object>();
            for (int i = 0; i < SchemaColumns.Length; i++)
            {
                if (SchemaColumns[i].IsPrimary)
                    pk.Add(SchemaColumns[i].Name, row[i]);
            }
            return pk;
        }

        public Dictionary<string, object> BuildPKFromKey(object[] key)
        {
            var pkColumns = SchemaColumns.Where(c => c.IsPrimary).ToArray();

            if (key.Length != pkColumns.Length)
                throw new Exception("The key doesn't correspond to table defenition.");

            var pk = new Dictionary<string, object>();
            for (int i = 0; i < pkColumns.Count(); i++)
                pk.Add(pkColumns[i].Name, key[i]);
            return pk;
        }

        public void SetPKFromKey(ref object[] row, object[] key)
        {
            var nbPKColumns = SchemaColumns.Where(c => c.IsPrimary).Count();
            var nbCols = SchemaColumns.Count();

            if (key.Length != nbPKColumns)
                throw new Exception("The key doesn't correspond to table defenition.");
            if (row.Length != nbCols)
                throw new Exception("The row doesn't correspond to table defenition.");

            var pkIndex = 0;
            for (int i = 0; i < nbCols; i++)
            {
                if (SchemaColumns[i].IsPrimary)
                {
                    row[i] = key[pkIndex];
                    pkIndex++;
                    if (pkIndex == nbPKColumns)
                        break;
                }
            }
        }

        public Dictionary<string, object> BuildDerivativePK(ITableDef derivativeTable, object[] sourceRow)
        {
            var colPKSrc = new Dictionary<string, object>();
            var colPKDst = new Dictionary<string, object>();

            for (int j = 0; j < SchemaColumns.Length; j++)
            {
                if (SchemaColumns[j].IsPrimary)
                    colPKSrc.Add(SchemaColumns[j].Name, sourceRow[j]);
            }

            foreach (var fk in derivativeTable.ForeignKeys)
            {
                bool isGoodFK = true;
                foreach (var col in fk.Columns)
                {
                    if (!colPKSrc.ContainsKey(col.NameTo))
                        isGoodFK = false;
                }

                if (isGoodFK)
                {
                    foreach (var col in fk.Columns)
                        colPKDst.Add(col.NameFrom, colPKSrc[col.NameTo]);
                    break;
                }
            }
            return colPKDst;
        }

        public override bool Equals(object obj)
        {
            TableDef t = obj as TableDef;
            if (t == null)
                return false;
            return t.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static TableDef Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            Int32 nbRows = DerivativeTables.Length;
            stream.Write(Name ?? "");
            stream.Write(IsStatic);
            stream.Write(SelectCommand ?? "");
            stream.Write(InsertCommand ?? "");

            stream.Write(nbRows);
            for (int i = 0; i < nbRows; i++)
            {
                stream.Write(DerivativeTables[i].ServerId);
                stream.Write(DerivativeTables[i].Database);
                stream.Write(DerivativeTables[i].Schema);
                stream.Write(DerivativeTables[i].Table);
                stream.Write((int)DerivativeTables[i].Access);
                stream.Write(DerivativeTables[i].Cascade);
            }

            nbRows = ForeignKeys.Length;
            stream.Write(nbRows);
            for (int i = 0; i < nbRows; i++)
            {
                stream.Write(ForeignKeys[i].ServerIdTo);
                stream.Write(ForeignKeys[i].DatabaseTo);
                stream.Write(ForeignKeys[i].SchemaTo);
                stream.Write(ForeignKeys[i].TableTo);

                Int32 nbCol = ForeignKeys[i].Columns.Length;
                stream.Write(nbCol);
                for (int j = 0; j < nbCol; j++)
                {
                    stream.Write(ForeignKeys[i].Columns[j].NameFrom);
                    stream.Write(ForeignKeys[i].Columns[j].NameTo);
                }
            }

            nbRows = SchemaColumns.Length;
            stream.Write(nbRows);
            for (int i = 0; i < nbRows; i++)
            {
                stream.Write(SchemaColumns[i].Name);
                stream.Write((Int32)SchemaColumns[i].Type);
                stream.Write(SchemaColumns[i].IsPrimary);
                stream.Write(SchemaColumns[i].IsForeignKey);
                stream.Write(SchemaColumns[i].IsAutoIncrement);
                stream.Write(SchemaColumns[i].BuilderName ?? "");
            }
        }

        public static TableDef Deserialize(BinaryReader stream)
        {
            Int32 nbRows, nbRows2;
            var t = new TableDef();
            var dtList = new List<DerivativeTable>();
            var fkList = new List<ForeignKey>();
            var schemaColList = new List<SchemaColumn>();

            t.Name = stream.ReadString();
            t.IsStatic = stream.ReadBoolean();
            t.SelectCommand = stream.ReadString();
            t.InsertCommand = stream.ReadString();

            nbRows = stream.ReadInt32();
            for (int i = 0; i < nbRows; i++)
            {
                dtList.Add(new DerivativeTable()
                {
                    ServerId = stream.ReadInt16(),
                    Database = stream.ReadString(),
                    Schema = stream.ReadString(),
                    Table = stream.ReadString(),
                    Access = (DerivativeTableAccess)stream.ReadInt32(),
                    Cascade = stream.ReadBoolean()
                });
            }

            nbRows = stream.ReadInt32();
            for (int i = 0; i < nbRows; i++)
            {
                var fkColList = new List<ForeignKeyColumn>();

                ForeignKey fk = new ForeignKey()
                {
                    ServerIdTo = stream.ReadInt16(),
                    DatabaseTo = stream.ReadString(),
                    SchemaTo = stream.ReadString(),
                    TableTo = stream.ReadString(),
                };

                nbRows2 = stream.ReadInt32();
                for (int j = 0; j < nbRows2; j++)
                {
                    fkColList.Add(new ForeignKeyColumn()
                    {
                        NameFrom = stream.ReadString(),
                        NameTo = stream.ReadString()
                    });
                }

                fk.Columns = fkColList.ToArray();
                fkList.Add(fk);
            }

            nbRows = stream.ReadInt32();
            for (int i = 0; i < nbRows; i++)
            {
                schemaColList.Add(new SchemaColumn()
                {
                    Name = stream.ReadString(),
                    Type = (DbType)stream.ReadInt32(),
                    IsPrimary = stream.ReadBoolean(),
                    IsForeignKey = stream.ReadBoolean(),
                    IsAutoIncrement = stream.ReadBoolean(),
                    BuilderName = stream.ReadString()
                });
            }

            t.DerivativeTables = dtList.ToArray();
            t.ForeignKeys = fkList.ToArray();
            t.SchemaColumns = schemaColList.ToArray();

            return t;
        }
    }

    internal sealed class ForeignKey : IForeignKey
    {
        public Int16 ServerIdTo { get; set; }
        public string DatabaseTo { get; set; }
        public string SchemaTo { get; set; }
        public string TableTo { get; set; }
        public IForeignKeyColumn[] Columns { get; set; }
    }

    internal sealed class ForeignKeyColumn : IForeignKeyColumn
    {
        public string NameFrom { get; set; }
        public string NameTo { get; set; }
    }

    internal sealed class SchemaColumn : ISchemaColumn
    {
        public string Name { get; set; }
        public DbType Type { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string BuilderName { get; set; }
    }

    internal sealed class DerivativeTable : IDerivativeTable
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public DerivativeTableAccess Access { get; set; }
        public bool Cascade { get; set; }

        public override bool Equals(object obj)
        {
            DerivativeTable tableToObj = obj as DerivativeTable;
            if (tableToObj == null)
                return false;
            else
                return ServerId.Equals(tableToObj.ServerId) &&
                    Database.Equals(tableToObj.Database) &&
                    Schema.Equals(tableToObj.Schema) &&
                    Table.Equals(tableToObj.Table);
        }

        public override int GetHashCode()
        {
            return Table.GetHashCode();
        }
    }

    internal static class TableDefExtensions
    {
        internal static TableDef GetTable(this IForeignKey fk)
        {
            return QueryDispatcher.Cache.CachedTables.GetTable(
                Impersonate(fk.ServerIdTo), fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        internal static TableDef GetTable(this IDerivativeTable dt)
        {
            return QueryDispatcher.Cache.CachedTables.GetTable(
                Impersonate(dt.ServerId), dt.Database, dt.Schema, dt.Table);
        }

        internal static TableDef GetTable(this ITableIdentifier dt)
        {
            return QueryDispatcher.Cache.CachedTables.GetTable(
                Impersonate(dt.ServerId), dt.Database, dt.Schema, dt.Table);
        }
    }
}
