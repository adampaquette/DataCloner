using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;

using DataCloner.Framework;
using DataCloner.DataAccess;

namespace DataCloner.DataClasse.Cache
{
    internal sealed class TableSchema : ITableSchema
    {
        public string Name { get; set; }
        public bool IsStatic { get; set; }
        public string SelectCommand { get; set; }
        public string InsertCommand { get; set; }
        public IDerivativeTable[] DerivativeTables { get; set; }
        public IForeignKey[] ForeignKeys { get; set; }
        public IUniqueKey[] UniqueKeys { get; set; }
        public IColumnDefinition[] ColumnsDefinition { get; set; }

        public TableSchema()
        {
            DerivativeTables = new DerivativeTable[] { };
            ForeignKeys = new ForeignKey[] { };
            UniqueKeys = new UniqueKey[] { };
            ColumnsDefinition = new ColumnDefinition[] { };
        }

        public object[] BuildRawFKFromDataRow(IForeignKey fk, object[] row)
        {
            var pk = new List<object>();
            for (int j = 0; j < fk.Columns.Length; j++)
            {
                int posTblSource = ColumnsDefinition.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                pk.Add(row[posTblSource]);
            }
            return pk.ToArray();
        }

        public ColumnsWithValue BuildKeyFromDerivativeDataRow(IForeignKey fk, object[] row)
        {
            var fkValues = new ColumnsWithValue();
            for (int j = 0; j < fk.Columns.Length; j++)
            {
                int posTblSource = ColumnsDefinition.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                fkValues.Add(fk.Columns[j].NameTo, row[posTblSource]);
            }
            return fkValues;
        }

        public ColumnsWithValue BuildKeyFromFkDataRow(IForeignKey fk, object[] row)
        {
            var fkValues = new ColumnsWithValue();
            foreach (var col in fk.Columns)
            {
                var posFkTable = ColumnsDefinition.IndexOf(c => c.Name == col.NameTo);
                fkValues.Add(col.NameFrom, row[posFkTable]);
            }
            return fkValues;
        }

        public object[] BuildRawPKFromDataRow(object[] row)
        {
            var pk = new List<object>();
            for (int i = 0; i < ColumnsDefinition.Length; i++)
            {
                if (ColumnsDefinition[i].IsPrimary)
                    pk.Add(row[i]);
            }
            return pk.ToArray();
        }

        public ColumnsWithValue BuildPKFromDataRow(object[] row)
        {
            var pk = new ColumnsWithValue();
            for (int i = 0; i < ColumnsDefinition.Length; i++)
            {
                if (ColumnsDefinition[i].IsPrimary)
                    pk.Add(ColumnsDefinition[i].Name, row[i]);
            }
            return pk;
        }

        public ColumnsWithValue BuildPKFromRawKey(object[] key)
        {
            var pkColumns = ColumnsDefinition.Where(c => c.IsPrimary).ToArray();

            if (key.Length != pkColumns.Length)
                throw new Exception("The key doesn't correspond to table defenition.");

            var pk = new ColumnsWithValue();
            for (int i = 0; i < pkColumns.Count(); i++)
                pk.Add(pkColumns[i].Name, key[i]);
            return pk;
        }

        /// <summary>
        /// //On trouve la position de chaque colonne pour affecter la valeur de destination.
        /// </summary>
        public void SetFKInDatarow(IForeignKey fkDefinition, object[] fkData, object[] destinationRow)
        {
            for (int j = 0; j < fkDefinition.Columns.Length; j++)
            {
                for (int k = 0; k < ColumnsDefinition.Length; k++)
                {
                    if (fkDefinition.Columns[j].NameFrom == ColumnsDefinition[k].Name)
                        destinationRow[k] = fkData[j];
                }
            }
        }

        public void SetFKFromDatarowInDatarow(TableSchema fkTable, IForeignKey fk, object[][] sourceRow, object[] destinationRow)
        {
            for (int j = 0; j < fk.Columns.Length; j++)
            {
                int posTblSourceFK = ColumnsDefinition.IndexOf(c => c.Name == fk.Columns[j].NameFrom);
                int posTblDestinationPK = fkTable.ColumnsDefinition.IndexOf(c => c.Name == fk.Columns[j].NameTo);

                destinationRow[posTblSourceFK] = sourceRow[0][posTblDestinationPK];
            }
        }

        public void SetPKFromKey(ref object[] row, object[] key)
        {
            var nbPKColumns = ColumnsDefinition.Where(c => c.IsPrimary).Count();
            var nbCols = ColumnsDefinition.Count();

            if (key.Length != nbPKColumns)
                throw new Exception("The key doesn't correspond to table defenition.");
            if (row.Length != nbCols)
                throw new Exception("The row doesn't correspond to table defenition.");

            var pkIndex = 0;
            for (int i = 0; i < nbCols; i++)
            {
                if (ColumnsDefinition[i].IsPrimary)
                {
                    row[i] = key[pkIndex];
                    pkIndex++;
                    if (pkIndex == nbPKColumns)
                        break;
                }
            }
        }

        public  ColumnsWithValue BuildDerivativePK(ITableSchema derivativeTable, object[] sourceRow)
        {
            var colPKSrc = new ColumnsWithValue();
            var colPKDst = new ColumnsWithValue();

            for (int j = 0; j < ColumnsDefinition.Length; j++)
            {
                if (ColumnsDefinition[j].IsPrimary)
                    colPKSrc.Add(ColumnsDefinition[j].Name, sourceRow[j]);
            }

            //FK qui pointent vers la table courante
            foreach (var fk in derivativeTable.ForeignKeys.Where(k=>k.TableTo == Name))
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
            TableSchema t = obj as TableSchema;
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

        public static TableSchema Deserialize(Stream stream)
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

            nbRows = ColumnsDefinition.Length;
            stream.Write(nbRows);
            for (int i = 0; i < nbRows; i++)
            {
                stream.Write(ColumnsDefinition[i].Name);
                stream.Write((Int32)ColumnsDefinition[i].Type);
                stream.Write(ColumnsDefinition[i].Size ?? "");
                stream.Write(ColumnsDefinition[i].IsPrimary);
                stream.Write(ColumnsDefinition[i].IsForeignKey);
                stream.Write(ColumnsDefinition[i].IsAutoIncrement);
                stream.Write(ColumnsDefinition[i].BuilderName ?? "");
            }
        }

        public static TableSchema Deserialize(BinaryReader stream)
        {
            Int32 nbRows, nbRows2;
            var t = new TableSchema();
            var dtList = new List<DerivativeTable>();
            var fkList = new List<ForeignKey>();
            var schemaColList = new List<ColumnDefinition>();

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
                schemaColList.Add(new ColumnDefinition()
                {
                    Name = stream.ReadString(),
                    Type = (DbType)stream.ReadInt32(),
                    Size = stream.ReadString(),
                    IsPrimary = stream.ReadBoolean(),
                    IsForeignKey = stream.ReadBoolean(),
                    IsAutoIncrement = stream.ReadBoolean(),
                    BuilderName = stream.ReadString()
                });
            }

            t.DerivativeTables = dtList.ToArray();
            t.ForeignKeys = fkList.ToArray();
            t.ColumnsDefinition = schemaColList.ToArray();

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

    internal sealed class UniqueKey : IUniqueKey
    {
        public string[] Columns { get; set; }
    }

    internal sealed class ColumnDefinition : IColumnDefinition
    {
        public string Name { get; set; }
        public DbType Type { get; set; }
        public string Size { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsUniqueKey { get; set; }
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
        internal static TableSchema GetTable(this IForeignKey fk)
        {
            return Cache.Current.DatabasesSchema.GetTable(fk.ServerIdTo, fk.DatabaseTo, fk.SchemaTo, fk.TableTo);
        }

        internal static TableSchema GetTable(this IDerivativeTable dt)
        {
            return Cache.Current.DatabasesSchema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }

        internal static TableSchema GetTable(this ITableIdentifier dt)
        {
            return Cache.Current.DatabasesSchema.GetTable(dt.ServerId, dt.Database, dt.Schema, dt.Table);
        }
    }
}
