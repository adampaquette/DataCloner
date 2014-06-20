using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataCloner.Enum;

namespace DataCloner.Serialization
{
    [Serializable]
    [XmlRoot(Namespace = "urn:DerivativeTableAccess")]
    [XmlType(Namespace = "urn:DerivativeTableAccess")]
    public class DerivativeTableAccessXml
    {
        [XmlElement("Server")]
        public List<ServerXml> Servers { get; set; }

        public DerivativeTableAccessXml()
        {
            Servers = new List<ServerXml>();
        }

        public DerivativeTableAccessXml(List<ServerXml> servers)
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
            [XmlElement("TableFrom")]
            public List<TableFromXml> Tables { get; set; }

            public SchemaXml()
            {
                Tables = new List<TableFromXml>();
            }

            public SchemaXml(List<TableFromXml> tables, string name)
            {
                Tables = tables;
                Name = name;
            }
        }

        public class TableFromXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public AccessXml Access { get; set; }
            [XmlAttribute]
            public bool Cascade { get; set; }
            [XmlAttribute]
            public bool Active { get; set; }
            [XmlElement("To")]
            public List<TableToXml> TablesTo { get; set; }

            public TableFromXml() 
            {
                Active = true;
                TablesTo = new List<TableToXml>();
            }

            public TableFromXml(string name, AccessXml access,bool cascade, bool active, List<TableToXml> tablesTo)
            {
                Name = name;
                Access = access;
                Cascade = cascade;
                Active = active;
                TablesTo = tablesTo;
            }
        }

        public class TableToXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public AccessXml Access { get; set; }
            [XmlAttribute]
            public bool Cascade { get; set; }
            [XmlAttribute]
            public bool Active { get; set; }

            public TableToXml()
            {
                Active = true;
            }

            public TableToXml(string name, AccessXml access, bool cascade, bool active)
            {
                Name = name;
                Access = access;
                Cascade = cascade;
                Active = active;
            }
        }
    }   
}
