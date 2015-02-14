using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class ServerModifier
    {
        [XmlAttribute]
        public Int16 Id { get; set; }
        [XmlElement("Database")]
        public List<DatabaseModifier> Databases { get; set; }

        public ServerModifier()
        {
            Databases = new List<DatabaseModifier>();
        }

        public ServerModifier(List<DatabaseModifier> databases, Int16 id)
        {
            Databases = databases;
            Id = id;
        }
    }

    [Serializable]
    public class DatabaseModifier
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlElement("Schema")]
        public List<SchemaModifier> Schemas { get; set; }

        public DatabaseModifier()
        {
            Schemas = new List<SchemaModifier>();
        }

        public DatabaseModifier(List<SchemaModifier> schemas, string name)
        {
            Schemas = schemas;
            Name = name;
        }
    }

    [Serializable]
    public class SchemaModifier
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlElement("Table")]
        public List<TableModifier> Tables { get; set; }

        public SchemaModifier()
        {
            Tables = new List<TableModifier>();
        }

        public SchemaModifier(List<TableModifier> tables, string name)
        {
            Tables = tables;
            Name = name;
        }
    }

    [Serializable]
    public class TableModifier
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public bool IsStatic { get; set; }
        public DerativeTable DerativeTables { get; set; }
        [XmlArrayItem("Column")]
        public List<DataBuilder> DataBuilders { get; set; }
        public ForeignKeys ForeignKeys { get; set; }

        public TableModifier()
        {
            DataBuilders = new List<DataBuilder>();
            DerativeTables = new DerativeTable();
            ForeignKeys = new ForeignKeys();
        }
    }

    [Serializable]
    public class DataBuilder
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string BuilderName { get; set; }

        public DataBuilder() { }
    }

    [Serializable]
    public class DerativeTable
    {
        [XmlAttribute]
        public DerivativeTableAccess GlobalAccess { get; set; }
        [XmlAttribute]
        public bool Cascade { get; set; }
        [XmlElement("Table")]
        public List<DerivativeSubTable> DerativeSubTables { get; set; }

        public DerativeTable()
        {
            DerativeSubTables = new List<DerivativeSubTable>();
        }
    }

    [Serializable]
    public class DerivativeSubTable
    {
        [XmlAttribute]
        public Int16 ServerId { get; set; }
        [XmlAttribute]
        public string Database { get; set; }
        [XmlAttribute]
        public string Schema { get; set; }
        [XmlAttribute]
        public string Table { get; set; }
        [XmlAttribute]
        public DerivativeTableAccess Access { get; set; }
        [XmlAttribute]
        public bool Cascade { get; set; }

        public DerivativeSubTable() { }
    }

    [Serializable]
    public class ForeignKeys
    {
        [XmlElement("Add")]
        public List<ForeignKeyAdd> ForeignKeyAdd { get; set; }
        [XmlElement("Remove")]
        public List<ForeignKeyRemove> ForeignKeyRemove { get; set; }

        public ForeignKeys()
        {
            ForeignKeyAdd = new List<ForeignKeyAdd>();
            ForeignKeyRemove = new List<ForeignKeyRemove>();
        }
    }

    [Serializable]
    public class ForeignKeyAdd
    {
        [XmlAttribute]
        public Int16 ServerId { get; set; }
        [XmlAttribute]
        public string Database { get; set; }
        [XmlAttribute]
        public string Schema { get; set; }
        [XmlAttribute]
        public string Table { get; set; }

        [XmlElement("Column")]
        public List<ForeignKeyColumn> Columns { get; set; }

        public ForeignKeyAdd()
        {
            Columns = new List<ForeignKeyColumn>();
        }
    }

    [Serializable]
    public class ForeignKeyColumn
    {
        [XmlAttribute]
        public string NameFrom { get; set; }
        [XmlAttribute]
        public string NameTo { get; set; }

        public ForeignKeyColumn() { }
    }

    [Serializable]
    public class ForeignKeyRemove
    {
        [XmlElement("Column")]
        public List<ForeignKeyRemoveColumn> Columns { get; set; }

        public ForeignKeyRemove() { }
    }

    [Serializable]
    public class ForeignKeyRemoveColumn
    {
        [XmlAttribute]
        public string Name { get; set; }

        public ForeignKeyRemoveColumn() { }
    }
}