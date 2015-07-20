using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class Modifiers
    {
        [XmlElement("ServerModifier")]
        public List<ServerModifier> ServerModifiers { get; set; }
        [XmlElement("DatabaseModifier")]
        public List<DatabaseModifier> DatabaseModifiers { get; set; }
        [XmlElement("SchemaModifier")]
        public List<SchemaModifier> SchemaModifiers { get; set; }

        public Modifiers()
        {
            ServerModifiers = new List<ServerModifier>();
            DatabaseModifiers = new List<DatabaseModifier>();
            SchemaModifiers = new List<SchemaModifier>();
        }
    }

    [Serializable]
    public class ServerModifier
    {
        [XmlAttribute]
        public string Id { get; set; }
        [XmlAttribute]
        public Int16 TemplateId { get; set; }
        [XmlAttribute]
        public Int16 UseTemplateId { get; set; }
        [XmlElement("DatabaseModifier")]
        public List<DatabaseModifier> Databases { get; set; }

        public ServerModifier()
        {
            Databases = new List<DatabaseModifier>();
        }
    }

    [Serializable]
    public class DatabaseModifier
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public Int16 TemplateId { get; set; }
        [XmlAttribute]
        public Int16 UseTemplateId { get; set; }
        [XmlElement("SchemaModifier")]
        public List<SchemaModifier> Schemas { get; set; }

        public DatabaseModifier()
        {
            Schemas = new List<SchemaModifier>();
        }
    }

    [Serializable]
    public class SchemaModifier
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public Int16 TemplateId { get; set; }
        [XmlAttribute]
        public Int16 UseTemplateId { get; set; }
        [XmlElement("TableModifier")]
        public List<TableModifier> Tables { get; set; }

        public SchemaModifier()
        {
            Tables = new List<TableModifier>();
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
    }

    [Serializable]
    public class DerativeTable
    {
        [XmlAttribute]
        public DerivativeTableAccess GlobalAccess { get; set; }
        [XmlAttribute]
        public bool GlobalCascade { get; set; }
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
        public string ServerId { get; set; }
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
    }

    [Serializable]
    public class ForeignKeys
    {
        [XmlElement("Add")]
        public List<ForeignKeyAdd> ForeignKeyAdd { get; set; }
        [XmlElement("Remove")]
        public ForeignKeyRemove ForeignKeyRemove { get; set; }

        public ForeignKeys()
        {
            ForeignKeyAdd = new List<ForeignKeyAdd>();
            ForeignKeyRemove = new ForeignKeyRemove();
        }
    }

    [Serializable]
    public class ForeignKeyAdd
    {
        [XmlAttribute]
        public string ServerId { get; set; }
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
    }

    [Serializable]
    public class ForeignKeyRemove
    {
        [XmlElement("Column")]
        public List<ForeignKeyRemoveColumn> Columns { get; set; }

        public ForeignKeyRemove()
        {
            Columns = new List<ForeignKeyRemoveColumn>();
        }        
    }

    [Serializable]
    public class ForeignKeyRemoveColumn
    {
        [XmlAttribute]
        public string Name { get; set; }
    }
}