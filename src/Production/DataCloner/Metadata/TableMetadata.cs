using DataCloner.Data;
using DataCloner.Framework;
using DataCloner.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DataCloner.Metadata
{
    [DebuggerDisplay("{Name}")]
    public sealed class TableMetadata : ITableMetadata
    {
        private string _name;

        public string Name { get {return _name;} }
        public bool IsStatic { get; set; }
        public string SelectCommand { get; set; }
        public string InsertCommand { get; set; }
        public IDerivativeTable[] DerivativeTables { get; set; }
        public IForeignKey[] ForeignKeys { get; set; }
        public IUniqueKey[] UniqueKeys { get; set; }
        public IColumnDefinition[] ColumnsDefinition { get; set; }

        public TableMetadata(string name)
        {
            _name = name;
            DerivativeTables = new IDerivativeTable[] { };
            ForeignKeys = new IForeignKey[] { };
            UniqueKeys = new IUniqueKey[] { };
            ColumnsDefinition = new IColumnDefinition[] { };
        }

        public object[] BuildRawFkFromDataRow(IForeignKey fk, object[] row)
        {
            var pk = new List<object>();
            foreach (var t in fk.Columns)
            {
                var posTblSource = ColumnsDefinition.IndexOf(c => c.Name == t.NameFrom);
                pk.Add(row[posTblSource]);
            }
            return pk.ToArray();
        }

        public ColumnsWithValue BuildKeyFromDerivativeDataRow(IForeignKey fk, object[] row)
        {
            var fkValues = new ColumnsWithValue();
            foreach (var t in fk.Columns)
            {
                var posTblSource = ColumnsDefinition.IndexOf(c => c.Name == t.NameFrom);
                fkValues.Add(t.NameTo, row[posTblSource]);
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

        public object[] BuildRawPkFromDataRow(object[] row)
        {
            var pk = new List<object>();
            for (var i = 0; i < ColumnsDefinition.Length; i++)
            {
                if (ColumnsDefinition[i].IsPrimary)
                    pk.Add(row[i]);
            }
            return pk.ToArray();
        }

        public ColumnsWithValue BuildPkFromDataRow(object[] row)
        {
            var pk = new ColumnsWithValue();
            for (var i = 0; i < ColumnsDefinition.Length; i++)
            {
                if (ColumnsDefinition[i].IsPrimary)
                    pk.Add(ColumnsDefinition[i].Name, row[i]);
            }
            return pk;
        }

        public ColumnsWithValue BuildPkFromRawKey(object[] key)
        {
            var pkColumns = ColumnsDefinition.Where(c => c.IsPrimary).ToArray();

            if (key.Length != pkColumns.Length)
                throw new Exception("The key doesn't correspond to table defenition.");

            var pk = new ColumnsWithValue();
            for (var i = 0; i < pkColumns.Count(); i++)
                pk.Add(pkColumns[i].Name, key[i]);
            return pk;
        }

        /// <summary>
        /// //On trouve la position de chaque colonne pour affecter la valeur de destination.
        /// </summary>
        public void SetFkInDatarow(IForeignKey fkDefinition, object[] fkData, object[] destinationRow)
        {
            for (var j = 0; j < fkDefinition.Columns.Length; j++)
            {
                for (var k = 0; k < ColumnsDefinition.Length; k++)
                {
                    if (fkDefinition.Columns[j].NameFrom == ColumnsDefinition[k].Name)
                        destinationRow[k] = fkData[j];
                }
            }
        }

        public void SetFkFromDatarowInDatarow(TableMetadata fkTable, IForeignKey fk, object[] sourceRow, object[] destinationRow)
        {
            foreach (var col in fk.Columns)
            {
                var posTblSourceFk = ColumnsDefinition.IndexOf(c => c.Name == col.NameFrom);
                var posTblDestinationPk = fkTable.ColumnsDefinition.IndexOf(c => c.Name == col.NameTo);

                destinationRow[posTblSourceFk] = sourceRow[posTblDestinationPk];
            }
        }

        public void SetPkFromKey(ref object[] row, object[] key)
        {
            var nbPkColumns = ColumnsDefinition.Count(c => c.IsPrimary);
            var nbCols = ColumnsDefinition.Count();

            if (key.Length != nbPkColumns)
                throw new Exception("The key doesn't correspond to table defenition.");
            if (row.Length != nbCols)
                throw new Exception("The row doesn't correspond to table defenition.");

            var pkIndex = 0;
            for (var i = 0; i < nbCols; i++)
            {
                if (ColumnsDefinition[i].IsPrimary)
                {
                    row[i] = key[pkIndex];
                    pkIndex++;
                    if (pkIndex == nbPkColumns)
                        break;
                }
            }
        }

        public  ColumnsWithValue BuildDerivativePk(ITableMetadata derivativeTable, object[] sourceRow)
        {
            var colPkSrc = new ColumnsWithValue();
            var colPkDst = new ColumnsWithValue();

            for (var j = 0; j < ColumnsDefinition.Length; j++)
            {
                if (ColumnsDefinition[j].IsPrimary)
                    colPkSrc.Add(ColumnsDefinition[j].Name, sourceRow[j]);
            }

            //FK qui pointent vers la table courante
            foreach (var fk in derivativeTable.ForeignKeys.Where(k=>k.TableTo == Name))
            {
                //Toutes les colonnes doivent correspondre
                if (!fk.Columns.Any(c => !colPkSrc.ContainsKey(c.NameTo)))
                {
                    foreach (var col in fk.Columns)
                        colPkDst.Add(col.NameFrom, colPkSrc[col.NameTo]);
                    break;
                }
            }
            if(!colPkDst.Any())
                throw new Exception(
                    string.Format("A problem append with the metadata. The derivative table '{0}' dosen't have a foreign key to the table '{1}'.", derivativeTable.Name, Name));
            return colPkDst;
        }

        public override bool Equals(object obj)
        {
            var t = obj as TableMetadata;
            if (t == null)
                return false;
            return t.Name.Equals(Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static TableMetadata Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            var nbRows = DerivativeTables.Length;
            stream.Write(Name ?? "");
            stream.Write(IsStatic);
            stream.Write(SelectCommand ?? "");
            stream.Write(InsertCommand ?? "");

            stream.Write(nbRows);
            for (var i = 0; i < nbRows; i++)
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
            for (var i = 0; i < nbRows; i++)
            {
                stream.Write(ForeignKeys[i].ServerIdTo);
                stream.Write(ForeignKeys[i].DatabaseTo);
                stream.Write(ForeignKeys[i].SchemaTo);
                stream.Write(ForeignKeys[i].TableTo);

                var nbCol = ForeignKeys[i].Columns.Length;
                stream.Write(nbCol);
                for (var j = 0; j < nbCol; j++)
                {
                    stream.Write(ForeignKeys[i].Columns[j].NameFrom);
                    stream.Write(ForeignKeys[i].Columns[j].NameTo);
                }
            }

            nbRows = ColumnsDefinition.Length;
            stream.Write(nbRows);
            for (var i = 0; i < nbRows; i++)
            {
                stream.Write(ColumnsDefinition[i].Name);
                stream.Write((Int32)ColumnsDefinition[i].DbType);
                stream.Write(ColumnsDefinition[i].IsPrimary);
                stream.Write(ColumnsDefinition[i].IsForeignKey);
                stream.Write(ColumnsDefinition[i].IsAutoIncrement);
                stream.Write(ColumnsDefinition[i].BuilderName ?? "");
                ColumnsDefinition[i].SqlType.Serialize(stream);
            }
        }

        public static TableMetadata Deserialize(BinaryReader stream)
        {
            
            var dtList = new List<DerivativeTable>();
            var fkList = new List<ForeignKey>();
            var schemaColList = new List<ColumnDefinition>();

            var t = new TableMetadata(stream.ReadString())
            {
                IsStatic = stream.ReadBoolean(),
                SelectCommand = stream.ReadString(),
                InsertCommand = stream.ReadString()
            };

            var nbRows = stream.ReadInt32();
            for (var i = 0; i < nbRows; i++)
            {
                dtList.Add(new DerivativeTable
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
            for (var i = 0; i < nbRows; i++)
            {
                var fkColList = new List<ForeignKeyColumn>();

                var fk = new ForeignKey
                {
                    ServerIdTo = stream.ReadInt16(),
                    DatabaseTo = stream.ReadString(),
                    SchemaTo = stream.ReadString(),
                    TableTo = stream.ReadString()
                };

                var nbRows2 = stream.ReadInt32();
                for (var j = 0; j < nbRows2; j++)
                {
                    fkColList.Add(new ForeignKeyColumn
                    {
                        NameFrom = stream.ReadString(),
                        NameTo = stream.ReadString()
                    });
                }

                fk.Columns = fkColList.ToArray();
                fkList.Add(fk);
            }

            nbRows = stream.ReadInt32();
            for (var i = 0; i < nbRows; i++)
            {
                schemaColList.Add(new ColumnDefinition
                {
                    Name = stream.ReadString(),
                    DbType = (DbType)stream.ReadInt32(),                    
                    IsPrimary = stream.ReadBoolean(),
                    IsForeignKey = stream.ReadBoolean(),
                    IsAutoIncrement = stream.ReadBoolean(),
                    BuilderName = stream.ReadString(),
                    SqlType = SqlType.Deserialize(stream)
            });
            }

            t.DerivativeTables = dtList.ToArray();
            t.ForeignKeys = fkList.ToArray();
            t.ColumnsDefinition = schemaColList.ToArray();

            return t;
        }
    }

    public sealed class ForeignKey : IForeignKey
    {
        public Int16 ServerIdTo { get; set; }
        public string DatabaseTo { get; set; }
        public string SchemaTo { get; set; }
        public string TableTo { get; set; }
        public IForeignKeyColumn[] Columns { get; set; }
    }

    public sealed class ForeignKeyColumn : IForeignKeyColumn
    {
        public string NameFrom { get; set; }
        public string NameTo { get; set; }
    }

    public sealed class UniqueKey : IUniqueKey
    {
        public string[] Columns { get; set; }
    }

    public sealed class ColumnDefinition : IColumnDefinition
    {
        public string Name { get; set; }
        public DbType DbType { get; set; }
        public SqlType SqlType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsUniqueKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string BuilderName { get; set; }
    }

    public sealed class DerivativeTable : IDerivativeTable
    {
        public Int16 ServerId { get; set; }
        public string Database { get; set; }
        public string Schema { get; set; }
        public string Table { get; set; }
        public DerivativeTableAccess Access { get; set; }
        public bool Cascade { get; set; }

        public override bool Equals(object obj)
        {
            var tableToObj = obj as DerivativeTable;
            if (tableToObj == null)
                return false;
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
}
