using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DataCloner.Core.Configuration
{
    [DataContract]
    public class Modifiers
    {
        //TODO : [XmlElement("Server")]
        public List<ServerModifier> ServerModifiers { get; set; }
        //TODO :[XmlElement("Database")]
        public List<DatabaseModifier> DatabaseModifiers { get; set; }
        //TODO :[XmlElement("Schema")]
        public List<SchemaModifier> SchemaModifiers { get; set; }

        public Modifiers()
        {
            ServerModifiers = new List<ServerModifier>();
            DatabaseModifiers = new List<DatabaseModifier>();
            SchemaModifiers = new List<SchemaModifier>();
        }
    }

    [DataContract]
    public class ServerModifier
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public Int16 TemplateId { get; set; }
        [DataMember]
        public Int16 BasedOn { get; set; }
        //TODO :[XmlElement("Database")]
        public List<DatabaseModifier> Databases { get; set; }

        public ServerModifier()
        {
            Databases = new List<DatabaseModifier>();
        }
    }

    [DataContract]
    public class DatabaseModifier
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public Int16 TemplateId { get; set; }
        [DataMember]
        public Int16 BasedOn { get; set; }
        //TODO :[XmlElement("Schema")]
        public List<SchemaModifier> Schemas { get; set; }

        public DatabaseModifier()
        {
            Schemas = new List<SchemaModifier>();
        }
    }

    [DataContract]
    public class SchemaModifier
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public Int16 TemplateId { get; set; }
        [DataMember]
        public Int16 BasedOn { get; set; }
        //TODO :[XmlElement("Table")]
        public List<TableModifier> Tables { get; set; }

        public SchemaModifier()
        {
            Tables = new List<TableModifier>();
        }
    }

    [DataContract]
    public class TableModifier
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsStatic { get; set; }
        public DerativeTable DerativeTables { get; set; }
        //TODO :[XmlArrayItem("Column")]
        public List<DataBuilder> DataBuilders { get; set; }
        public ForeignKeys ForeignKeys { get; set; }

        public TableModifier()
        {
            DataBuilders = new List<DataBuilder>();
            DerativeTables = new DerativeTable();
            ForeignKeys = new ForeignKeys();
        }
    }

    [DataContract]
    public class DataBuilder
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string BuilderName { get; set; }
    }

    [DataContract]
    public class DerativeTable
    {
        [DataMember]
        public DerivativeTableAccess GlobalAccess { get; set; }
        [DataMember]
        public bool GlobalCascade { get; set; }
        //TODO :[XmlElement("Table")]
        public List<DerivativeSubTable> DerativeSubTables { get; set; }

        public DerativeTable()
        {
            DerativeSubTables = new List<DerivativeSubTable>();
        }
    }

    [DataContract]
    public class DerivativeSubTable
    {
        [DataMember]
        public string ServerId { get; set; }
        [DataMember]
        public string Database { get; set; }
        [DataMember]
        public string Schema { get; set; }
        [DataMember]
        public string Table { get; set; }
        [DataMember]
        public DerivativeTableAccess Access { get; set; }
        [DataMember]
        public bool Cascade { get; set; }
    }

    [DataContract]
    public class ForeignKeys
    {
        //TODO :[XmlElement("Add")]
        public List<ForeignKeyAdd> ForeignKeyAdd { get; set; }
        //TODO :[XmlElement("Remove")]
        public ForeignKeyRemove ForeignKeyRemove { get; set; }

        public ForeignKeys()
        {
            ForeignKeyAdd = new List<ForeignKeyAdd>();
            ForeignKeyRemove = new ForeignKeyRemove();
        }
    }

    [DataContract]
    public class ForeignKeyAdd
    {
        [DataMember]
        public string ServerId { get; set; }
        [DataMember]
        public string Database { get; set; }
        [DataMember]
        public string Schema { get; set; }
        [DataMember]
        public string Table { get; set; }

        //TODO :[XmlElement("Column")]
        public List<ForeignKeyColumn> Columns { get; set; }

        public ForeignKeyAdd()
        {
            Columns = new List<ForeignKeyColumn>();
        }
    }

    [DataContract]
    public class ForeignKeyColumn
    {
        [DataMember]
        public string NameFrom { get; set; }
        [DataMember]
        public string NameTo { get; set; }
    }

    [DataContract]
    public class ForeignKeyRemove
    {
        //TODO :[XmlElement("Column")]
        public List<ForeignKeyRemoveColumn> Columns { get; set; }

        public ForeignKeyRemove()
        {
            Columns = new List<ForeignKeyRemoveColumn>();
        }        
    }

    [DataContract]
    public class ForeignKeyRemoveColumn
    {
        [DataMember]
        public string Name { get; set; }
    }
}