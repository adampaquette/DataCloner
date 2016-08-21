using DataCloner.Core.Data;
using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace DataCloner.Core.Metadata
{
    /// <summary>
    /// Contains all the metadatas of a SQL server's table.
    /// </summary>
    /// <example>Columns, PrimaryKeys, ForeignKeys...</example>
    [DebuggerDisplay("{Name}")]
    public sealed class TableMetadata : IEquatable<TableMetadata>
    {
        public string Name { get; }
        public bool IsStatic { get; set; }
        public string SelectCommand { get; set; }
        public string InsertCommand { get; set; }
        public List<DerivativeTable> DerivativeTables { get; set; }
        public List<ForeignKey> ForeignKeys { get; set; }
        public List<UniqueKey> UniqueKeys { get; set; }
        public List<ColumnDefinition> ColumnsDefinition { get; set; }

        public TableMetadata(string name)
        {
            Name = name;
            DerivativeTables = new List<DerivativeTable>();
            ForeignKeys = new List<ForeignKey>();
            UniqueKeys = new List<UniqueKey>();
            ColumnsDefinition = new List<ColumnDefinition>();
        }

        public object[] BuildRawFkFromDataRow(ForeignKey fk, object[] row)
        {
            var pk = new List<object>();
            foreach (var t in fk.Columns)
            {
                var posTblSource = ColumnsDefinition.IndexOf(c => c.Name == t.NameFrom);
                pk.Add(row[posTblSource]);
            }
            return pk.ToArray();
        }

        public ColumnsWithValue BuildKeyFromDerivativeDataRow(ForeignKey fk, object[] row)
        {
            var fkValues = new ColumnsWithValue();
            foreach (var t in fk.Columns)
            {
                var posTblSource = ColumnsDefinition.IndexOf(c => c.Name == t.NameFrom);
                fkValues.Add(t.NameTo, row[posTblSource]);
            }
            return fkValues;
        }

        public ColumnsWithValue BuildKeyFromFkDataRow(ForeignKey fk, object[] row)
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
            for (var i = 0; i < ColumnsDefinition.Count; i++)
            {
                if (ColumnsDefinition[i].IsPrimary)
                    pk.Add(row[i]);
            }
            return pk.ToArray();
        }

        public ColumnsWithValue BuildPkFromDataRow(object[] row)
        {
            var pk = new ColumnsWithValue();
            for (var i = 0; i < ColumnsDefinition.Count; i++)
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
        public void SetFkInDatarow(ForeignKey fkDefinition, object[] fkData, object[] destinationRow)
        {
            for (var j = 0; j < fkDefinition.Columns.Count; j++)
            {
                for (var k = 0; k < ColumnsDefinition.Count; k++)
                {
                    if (fkDefinition.Columns[j].NameFrom == ColumnsDefinition[k].Name)
                        destinationRow[k] = fkData[j];
                }
            }
        }

        public void SetFkFromDatarowInDatarow(TableMetadata fkTable, ForeignKey fk, object[] sourceRow, object[] destinationRow)
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

        public  ColumnsWithValue BuildDerivativePk(TableMetadata derivativeTable, object[] sourceRow)
        {
            var colPkSrc = new ColumnsWithValue();
            var colPkDst = new ColumnsWithValue();

            for (var j = 0; j < ColumnsDefinition.Count; j++)
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
                throw new Exception($"A problem append with the metadata. The derivative table '{derivativeTable.Name}' dosen't have a foreign key to the table '{Name}'.");
            return colPkDst;
        }

        public void Serialize(Stream output)
        {
            Serialize(new BinaryWriter(output, Encoding.UTF8, true));
        }

        public static TableMetadata Deserialize(Stream input)
        {
            return Deserialize(new BinaryReader(input, Encoding.UTF8, true));
        }

        public void Serialize(BinaryWriter output)
        {
            var nbRows = DerivativeTables.Count;
            output.Write(Name ?? "");
            output.Write(IsStatic);
            output.Write(SelectCommand ?? "");
            output.Write(InsertCommand ?? "");

            output.Write(nbRows);
            for (var i = 0; i < nbRows; i++)
            {
                output.Write(DerivativeTables[i].ServerId);
                output.Write(DerivativeTables[i].Database);
                output.Write(DerivativeTables[i].Schema);
                output.Write(DerivativeTables[i].Table);
                output.Write((int)DerivativeTables[i].Access);
                output.Write(DerivativeTables[i].Cascade);
            }

            nbRows = ForeignKeys.Count;
            output.Write(nbRows);
            for (var i = 0; i < nbRows; i++)
            {
                output.Write(ForeignKeys[i].ServerIdTo);
                output.Write(ForeignKeys[i].DatabaseTo);
                output.Write(ForeignKeys[i].SchemaTo);
                output.Write(ForeignKeys[i].TableTo);

                var nbCol = ForeignKeys[i].Columns.Count;
                output.Write(nbCol);
                for (var j = 0; j < nbCol; j++)
                {
                    output.Write(ForeignKeys[i].Columns[j].NameFrom);
                    output.Write(ForeignKeys[i].Columns[j].NameTo);
                }
            }

            nbRows = ColumnsDefinition.Count;
            output.Write(nbRows);
            for (var i = 0; i < nbRows; i++)
            {
                output.Write(ColumnsDefinition[i].Name);
                output.Write((Int32)ColumnsDefinition[i].DbType);
                output.Write(ColumnsDefinition[i].IsPrimary);
                output.Write(ColumnsDefinition[i].IsForeignKey);
                output.Write(ColumnsDefinition[i].IsAutoIncrement);
                output.Write(ColumnsDefinition[i].BuilderName ?? "");
                ColumnsDefinition[i].SqlType.Serialize(output);
            }
        }

        public static TableMetadata Deserialize(BinaryReader input)
        {
            
            var dtList = new List<DerivativeTable>();
            var fkList = new List<ForeignKey>();
            var schemaColList = new List<ColumnDefinition>();

            var t = new TableMetadata(input.ReadString())
            {
                IsStatic = input.ReadBoolean(),
                SelectCommand = input.ReadString(),
                InsertCommand = input.ReadString()
            };

            var nbRows = input.ReadInt32();
            for (var i = 0; i < nbRows; i++)
            {
                dtList.Add(new DerivativeTable
                {
                    ServerId = input.ReadInt16(),
                    Database = input.ReadString(),
                    Schema = input.ReadString(),
                    Table = input.ReadString(),
                    Access = (DerivativeTableAccess)input.ReadInt32(),
                    Cascade = input.ReadBoolean()
                });
            }

            nbRows = input.ReadInt32();
            for (var i = 0; i < nbRows; i++)
            {
                var fkColList = new List<ForeignKeyColumn>();

                var fk = new ForeignKey
                {
                    ServerIdTo = input.ReadInt16(),
                    DatabaseTo = input.ReadString(),
                    SchemaTo = input.ReadString(),
                    TableTo = input.ReadString()
                };

                var nbRows2 = input.ReadInt32();
                for (var j = 0; j < nbRows2; j++)
                {
                    fkColList.Add(new ForeignKeyColumn
                    {
                        NameFrom = input.ReadString(),
                        NameTo = input.ReadString()
                    });
                }

                fk.Columns = fkColList;
                fkList.Add(fk);
            }

            nbRows = input.ReadInt32();
            for (var i = 0; i < nbRows; i++)
            {
                schemaColList.Add(new ColumnDefinition
                {
                    Name = input.ReadString(),
                    DbType = (DbType)input.ReadInt32(),                    
                    IsPrimary = input.ReadBoolean(),
                    IsForeignKey = input.ReadBoolean(),
                    IsAutoIncrement = input.ReadBoolean(),
                    BuilderName = input.ReadString(),
                    SqlType = SqlType.Deserialize(input)
            });
            }

            t.DerivativeTables = dtList;
            t.ForeignKeys = fkList;
            t.ColumnsDefinition = schemaColList;

            return t;
        }

        public override bool Equals(object obj)
        {
            var t = obj as TableMetadata;
            return Equals(t);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public bool Equals(TableMetadata other)
        {
            return other != null && other.Name.Equals(Name, StringComparison.OrdinalIgnoreCase);
        }
    }

    public sealed class ForeignKey
    {
        public Int16 ServerIdTo { get; set; }
        public string DatabaseTo { get; set; }
        public string SchemaTo { get; set; }
        public string TableTo { get; set; }
        public List<ForeignKeyColumn> Columns { get; set; }
    }

    public sealed class ForeignKeyColumn
    {
        public string NameFrom { get; set; }
        public string NameTo { get; set; }
    }

    public sealed class UniqueKey 
    {
        public List<string> Columns { get; set; }
    }

    public sealed class ColumnDefinition 
    {
        public string Name { get; set; }
        public DbType DbType { get; set; }
        public SqlType SqlType { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsUniqueKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string BuilderName { get; set; }

        public ColumnDefinition()
        {
            SqlType = new SqlType();
        }
    }

    public sealed class DerivativeTable
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
