using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    [XmlRoot(Namespace = "urn:DataBuilder")]
    [XmlType(Namespace = "urn:DataBuilder")]
    public class DataBuilderXml
    {
        [XmlElement("Server")]
        public List<ServerXml> Servers { get; set; }

        public DataBuilderXml()
        {
            Servers = new List<ServerXml>();
        }

        public DataBuilderXml(List<ServerXml> servers)
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
            public List<TableXml> Tables { get; set; }

            public SchemaXml()
            {
                Tables = new List<TableXml>();
            }

            public SchemaXml(List<TableXml> tables, string name)
            {
                Tables = tables;
                Name = name;
            }
        }

        public class TableXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement("Column")]
            public List<ColumnXml> Columns { get; set; }

            public TableXml() 
            {
                Columns = new List<ColumnXml>();
            }

            public TableXml(string name, List<ColumnXml> columns)
            {
                Name = name;
                Columns = columns;
            }
        }

        public class ColumnXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public string BuilderName { get; set; }
            [XmlAttribute]
            public string MethodName { get; set; }
            [XmlAttribute]
            public bool Active { get; set; }

            public ColumnXml()
            {
                Active = true;
            }

            public ColumnXml(string name, string builderName, string methodName, bool active)
            {
                Name = name;
                BuilderName = builderName;
                MethodName = methodName;
                Active = active;
            }
        }
    }
}
