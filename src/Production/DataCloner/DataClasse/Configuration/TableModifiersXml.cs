using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataCloner.Enum;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class TableModifiersXml
    {
        [XmlElement("Server")]
        public List<ServerXml> Servers { get; set; }

        public TableModifiersXml()
        {
            Servers = new List<ServerXml>();
        }

        public TableModifiersXml(List<ServerXml> servers)
        {
            Servers = servers;
        }

        public class ServerXml
        {
            [XmlAttribute]
            public Int16 Id { get; set; }
            [XmlElement("Database")]
            public List<DatabaseXml> Databases { get; set; }

            public ServerXml()
            {
                Databases = new List<DatabaseXml>();
            }

            public ServerXml(List<DatabaseXml> databases, Int16 id)
            {
                Databases = databases;
                Id = id;
            }
        }

        public class DatabaseXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement("Schema")]
            public List<SchemaXml> Schemas { get; set; }

            public DatabaseXml()
            {
                Schemas = new List<SchemaXml>();
            }

            public DatabaseXml(List<SchemaXml> schemas, string name)
            {
                Schemas = schemas;
                Name = name;
            }
        }

        public class SchemaXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement("Table")]
            public List<TableModifierXml> Tables { get; set; }

            public SchemaXml()
            {
                Tables = new List<TableModifierXml>();
            }

            public SchemaXml(List<TableModifierXml> tables, string name)
            {
                Tables = tables;
                Name = name;
            }
        }

        public class TableModifierXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public bool IsStatic { get; set; }
            public DerativeTablesConfigXml DerativeTablesConfig { get; set; }
            [XmlArrayItem("Column")]
            public List<DataColumnBuilderXml> DataBuilders { get; set; }
            public ForeignKeysXml ForeignKeys { get; set; }

            public TableModifierXml()
            {
                DataBuilders = new List<DataColumnBuilderXml>();
                DerativeTablesConfig = new DerativeTablesConfigXml();
                ForeignKeys = new ForeignKeysXml();
            }
        }

        public class DataColumnBuilderXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public string BuilderName { get; set; }

            public DataColumnBuilderXml() { }
        }

        public class DerativeTablesConfigXml
        {
            [XmlAttribute]
            public DerivativeTableAccess GlobalAccess { get; set; }
            [XmlAttribute]
            public bool Cascade { get; set; }
            [XmlElement("Table")]
            public List<DerivativeSubTableXml> DerativeTables { get; set; }

            public DerativeTablesConfigXml() 
            {
                DerativeTables = new List<DerivativeSubTableXml>();
            }
        }

        public class DerivativeSubTableXml
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

            public DerivativeSubTableXml() { }
        }

        public class ForeignKeysXml
        {
            [XmlElement("Add")]
            public List<ForeignKeyAddXml> ForeignKeyAdd { get; set; }
            [XmlElement("Remove")]
            public List<ForeignKeyRemoveXml> ForeignKeyRemove { get; set; }

            public ForeignKeysXml()
            {
                ForeignKeyAdd = new List<ForeignKeyAddXml>();
                ForeignKeyRemove = new List<ForeignKeyRemoveXml>();
            }
        }

        public class ForeignKeyAddXml
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
            public List<ForeignKeyColumnXml> Columns { get; set; }

            public ForeignKeyAddXml()
            {
                Columns = new List<ForeignKeyColumnXml>();
            }
        }

        public class ForeignKeyColumnXml
        {
            [XmlAttribute]
            public string NameFrom { get; set; }
            [XmlAttribute]
            public string NameTo { get; set; }

            public ForeignKeyColumnXml() { }
        }

        public class ForeignKeyRemoveXml
        {
            [XmlElement("Column")]
            public List<ForeignKeyRemoveColumnXml> Columns { get; set; }

            public ForeignKeyRemoveXml() { }
        }

        public class ForeignKeyRemoveColumnXml
        {
            [XmlAttribute]
            public string Name { get; set; }

            public ForeignKeyRemoveColumnXml() { }
        }
    }
}
