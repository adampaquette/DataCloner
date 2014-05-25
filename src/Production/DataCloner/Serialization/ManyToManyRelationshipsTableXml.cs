using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    [XmlRoot(Namespace = "urn:ManyToMany")]
    [XmlType(Namespace = "urn:ManyToMany")]
    public class ManyToManyRelationshipsTablesXml
    {
        [XmlElement("Server")]
        public List<ServerXml> Servers { get; set; }

        public ManyToManyRelationshipsTablesXml()
        {
            Servers = new List<ServerXml>();
        }

        public ManyToManyRelationshipsTablesXml(List<ServerXml> servers)
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
            [XmlAttribute]
            public bool Active { get; set; }

            public TableXml() 
            {
                Active = true;
            }

            public TableXml(string name, bool active)
            {
                Name = name;
                Active = active;
            }
        }
    }
}
