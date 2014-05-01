using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    [XmlRoot(Namespace = "urn:ForeignKeys")]
    [XmlType(Namespace = "urn:ForeignKeys")]
    public class ForeignKeys
    {
        [XmlElement("Server")]
        public List<ServerXML> Servers { get; set; }

        public ForeignKeys()
        {
            Servers = new List<ServerXML>();
        }

        public ForeignKeys(List<ServerXML> servers)
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
            [XmlElement("Add")]
            public List<AddForeignKey> AddForeignKeys { get; set; }

            public TableXML(){}
            public TableXML(string name, List<AddForeignKey> addForeignKey)
            {
                Name = name;
                AddForeignKeys = addForeignKey;
            }
        }

        public class AddForeignKey
        {
            [XmlAttribute]
            public Int16 ServerIdDest { get; set; }
            [XmlAttribute]
            public string DatabaseDest { get; set; }
            [XmlAttribute]
            public string SchemaDest { get; set; }
            [XmlElement("Collumn")]
            public List<Collumn> Collumns { get; set; }

            public AddForeignKey()
            {
                Collumns = new List<Collumn>();
            }

            public AddForeignKey(List<Collumn> collumns, Int16 serverIdDest, string databaseDest, string schemaDest)
            {
                Collumns = collumns;
                ServerIdDest = serverIdDest;
                DatabaseDest = databaseDest;
                SchemaDest = schemaDest;
            }
        }

        public class Collumn
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public string ColNameDest { get; set; }

            public Collumn() {}
        }

    }
}