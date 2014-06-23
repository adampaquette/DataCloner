using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.IO;

using DataCloner.Enum;

namespace DataCloner.DataClasse.Cache
{
    internal sealed class TableDef
    {
        public string Name { get; set; }
        public bool IsStatic { get; set; }
        public string SelectCommand { get; set; }
        public string InsertCommand { get; set; }
        public DerivativeTable[] DerivativeTables { get; set; }
        public ForeignKey[] ForeignKeys { get; set; }
        public SchemaColumn[] SchemaColumns { get; set; }

        public TableDef()
        {
            DerivativeTables = new DerivativeTable[]{};
            ForeignKeys = new ForeignKey[]{};
            SchemaColumns = new SchemaColumn[]{};
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
            stream.Write(Name == null ? "": Name);
            stream.Write(IsStatic);
            stream.Write(SelectCommand == null ? "" : SelectCommand);
            stream.Write(InsertCommand == null ? "" : InsertCommand);

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
                stream.Write(SchemaColumns[i].Type);
                stream.Write(SchemaColumns[i].Order);
                stream.Write(SchemaColumns[i].IsPrimary);
                stream.Write(SchemaColumns[i].IsForeignKey);
                stream.Write(SchemaColumns[i].IsAutoIncrement);
                stream.Write(SchemaColumns[i].BuilderName == null ? "" : SchemaColumns[i].BuilderName);
            }
        }

        public static TableDef Deserialize(BinaryReader stream)
        {
            Int32 nbRows, nbRows2;
            TableDef t = new TableDef();
            List<DerivativeTable> dtList = new List<DerivativeTable>();
            List<ForeignKey> fkList = new List<ForeignKey>();
            List<ForeignKeyColumn> fkColList = new List<ForeignKeyColumn>();
            List<SchemaColumn> schemaColList = new List<SchemaColumn>();            

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
                List<SchemaColumn> colSchemaList = new List<SchemaColumn>();

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
            for (int j = 0; j < nbRows; j++)
            {
                schemaColList.Add(new SchemaColumn()
                {
                    Name = stream.ReadString(),
                    Type = stream.ReadString(),
                    Order = stream.ReadInt16(),
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

    internal sealed class ForeignKey
    {
        public Int16 ServerIdTo { get; set; }
        public string DatabaseTo { get; set; }
        public string SchemaTo { get; set; }
        public string TableTo { get; set; }
        public ForeignKeyColumn[] Columns { get; set; }


    }

    internal sealed class ForeignKeyColumn
    {
        public string NameFrom { get; set; }
        public string NameTo { get; set; }
    }

    internal sealed class SchemaColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Int16 Order { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsAutoIncrement { get; set; }
        public string BuilderName { get; set; }

        public static SchemaColumn Create(IDataRecord record)
        {
            return new SchemaColumn
            {
                 //Name = record.GetValue(0);
            };
        }
    }

    internal sealed class DerivativeTable
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
}
