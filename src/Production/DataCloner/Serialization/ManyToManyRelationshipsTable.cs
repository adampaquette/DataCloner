using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    [XmlRoot(Namespace = "urn:ManyToMany")]
    [XmlType(Namespace = "urn:ManyToMany")]
    public class ManyToManyRelationshipsTable
    {
        [XmlElement("Server")]
        public List<ServerXML> Servers { get; set; }

        public ManyToManyRelationshipsTable()
        {
            Servers = new List<ServerXML>();
        }

        public ManyToManyRelationshipsTable(List<ServerXML> servers)
        {
            Servers = servers;
        }

        public class ServerXML
        {
            [XmlAttribute]
            public Int16 Id { get; set; }
            [XmlElement("Database")]
            public List<DatabaseXML> Databases { get; set; }

            public ServerXML()
            {
                Databases = new List<DatabaseXML>();
            }

            public ServerXML(List<DatabaseXML> databases, Int16 id)
            {
                Databases = databases;
                Id = id;
            }
        }

        public class DatabaseXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement("Schema")]
            public List<SchemaXML> Schemas { get; set; }

            public DatabaseXML()
            {
                Schemas = new List<SchemaXML>();
            }

            public DatabaseXML(List<SchemaXML> schemas, string name)
            {
                Schemas = schemas;
                Name = name;
            }
        }

        public class SchemaXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement("Table")]
            public List<TableXML> Tables { get; set; }

            public SchemaXML()
            {
                Tables = new List<TableXML>();
            }

            public SchemaXML(List<TableXML> tables, string name)
            {
                Tables = tables;
                Name = name;
            }
        }

        public class TableXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public bool Active { get; set; }

            public TableXML() { }
            public TableXML(string name, bool active)
            {
                Name = name;
                Active = active;
            }
        }
    }
}
