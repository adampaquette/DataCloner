﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    public class Configuration
    {
        public static readonly string FileName = "dc.config";

        [XmlArrayItem("add")]
        public List<Connection> ConnectionStrings { get; set; }
        [XmlElement]
        public List<StaticTable> StaticTables { get; set; }
        [XmlElement]
        public List<ManyToManyRelationshipsTable> ManyToManyRelationshipsTable { get; set; }
        [XmlElement]
        public List<DerivativeTableAccess> DerivativeTableAccess { get; set; }

        public Configuration()
        {
            
            ConnectionStrings = new List<Connection>();
            StaticTables = new List<StaticTable>();
            ManyToManyRelationshipsTable = new List<ManyToManyRelationshipsTable>();
            DerivativeTableAccess = new List<DerivativeTableAccess>();
        }

        public void Save()
        {
            var xs = new System.Xml.Serialization.XmlSerializer(this.GetType());
            var fs = new System.IO.FileStream(FileName, System.IO.FileMode.Create);
            var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(fs, this, ns);
            fs.Close();
        }

        public static Configuration Load()
        {
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(Configuration));
            if (System.IO.File.Exists(FileName))
            {

                var fs = new System.IO.FileStream(FileName, System.IO.FileMode.Open);
                var cReturn = (Configuration)xs.Deserialize(fs);
                fs.Close();
                return cReturn;
            }
            return new Configuration();            
        }
    }

    [Serializable]
    public class Connection
    {
        [XmlAttribute]
        public Int16 Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ProviderName { get; set; }
        [XmlAttribute]
        public string ConnectionString { get; set; }

        public Connection() { }
        public Connection(Int16 id, string name, string providerName, string connectionString)
        {
            Id = id;
            Name = name;
            ProviderName = providerName;
            ConnectionString = connectionString;
        }
    }

    [Serializable]
    [XmlRoot(Namespace = "urn:StaticTable")]
    [XmlType(Namespace = "urn:StaticTable")]
    public class StaticTable
    {
        [XmlElement]
        public List<ServerXML> Server { get; set; }

        public StaticTable()
        {
            Server = new List<ServerXML>();
        }

        public StaticTable(List<ServerXML> server)
        {
            Server = server;
        }

        public class ServerXML
        { 
            [XmlAttribute]
            public Int16 Id { get; set; }
            [XmlElement]
            public List<DatabaseXML> Database { get; set; }

            public ServerXML()
            {
                Database = new List<DatabaseXML>();
            }

            public ServerXML(List<DatabaseXML> databases, Int16 id)
            {
                Database = databases;
                Id = id;
            }
        }

        public class DatabaseXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement]
            public List<SchemaXML> Schema { get; set; }

            public DatabaseXML()
            {
                Schema = new List<SchemaXML>();
            }

            public DatabaseXML(List<SchemaXML> schemas, string name)
            {
                Schema = schemas;
                Name = name;
            }
        }

        public class SchemaXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement]
            public List<TableXML> Table { get; set; }

            public SchemaXML()
            {
                Table = new List<TableXML>();
            }

            public SchemaXML(List<TableXML> tables, string name)
            {
                Table = tables;
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
    
    [Serializable]
    [XmlRoot(Namespace = "urn:ManyToMany")]
    [XmlType(Namespace = "urn:ManyToMany")]
    public class ManyToManyRelationshipsTable
    {
        [XmlElement]
        public List<ServerXML> Server { get; set; }

        public ManyToManyRelationshipsTable()
        {
            Server = new List<ServerXML>();
        }

        public ManyToManyRelationshipsTable(List<ServerXML> server)
        {
            Server = server;
        }

        public class ServerXML
        {
            [XmlAttribute]
            public Int16 Id { get; set; }
            [XmlElement]
            public List<DatabaseXML> Database { get; set; }

            public ServerXML()
            {
                Database = new List<DatabaseXML>();
            }

            public ServerXML(List<DatabaseXML> databases, Int16 id)
            {
                Database = databases;
                Id = id;
            }
        }

        public class DatabaseXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement]
            public List<SchemaXML> Schema { get; set; }

            public DatabaseXML()
            {
                Schema = new List<SchemaXML>();
            }

            public DatabaseXML(List<SchemaXML> schemas, string name)
            {
                Schema = schemas;
                Name = name;
            }
        }

        public class SchemaXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement]
            public List<TableXML> Table { get; set; }

            public SchemaXML()
            {
                Table = new List<TableXML>();
            }

            public SchemaXML(List<TableXML> tables, string name)
            {
                Table = tables;
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

    [Serializable]
    [XmlRoot(Namespace = "urn:DerivativeTableAccess")]
    [XmlType(Namespace = "urn:DerivativeTableAccess")]
    public class DerivativeTableAccess
    {
        [XmlElement]
        public List<ServerXML> Server { get; set; }

        public DerivativeTableAccess()
        {
            Server = new List<ServerXML>();
        }

        public DerivativeTableAccess(List<ServerXML> server)
        {
            Server = server;
        }

        public class ServerXML
        {
            [XmlAttribute]
            public Int16 Id { get; set; }
            [XmlElement]
            public List<DatabaseXML> Database { get; set; }

            public ServerXML()
            {
                Database = new List<DatabaseXML>();
            }

            public ServerXML(List<DatabaseXML> databases, Int16 id)
            {
                Database = databases;
                Id = id;
            }
        }

        public class DatabaseXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement]
            public List<SchemaXML> Schema { get; set; }

            public DatabaseXML()
            {
                Schema = new List<SchemaXML>();
            }

            public DatabaseXML(List<SchemaXML> schemas, string name)
            {
                Schema = schemas;
                Name = name;
            }
        }

        public class SchemaXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement]
            public List<TableXML> Table { get; set; }

            public SchemaXML()
            {
                Table = new List<TableXML>();
            }

            public SchemaXML(List<TableXML> tables, string name)
            {
                Table = tables;
                Name = name;
            }
        }

        public class TableXML
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public AccessXML Access { get; set; }
            [XmlAttribute]
            public bool Active { get; set; }

            public TableXML() { }
            public TableXML(string name, AccessXML access, bool active)
            {
                Name = name;
                Access = access;
                Active = active;
            }
        }

        public enum AccessXML
        { 
            Denied,
            Forced,
            Inherited
        }
    }   
}
