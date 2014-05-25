using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.Serialization
{
    [Serializable]
    [XmlRoot(Namespace = "urn:ForeignKeys")]
    [XmlType(Namespace = "urn:ForeignKeys")]
    public class ForeignKeysXml
    {
        [XmlElement("Server")]
        public List<ServerXml> Servers { get; set; }

        public ForeignKeysXml()
        {
            Servers = new List<ServerXml>();
        }

        public ForeignKeysXml(List<ServerXml> servers)
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
            [XmlElement("Add")]
            public List<AddForeignKeyXml> AddForeignKeys { get; set; }
            [XmlElement("Modify")]
            public List<ModifyForeignKeyXml> ModifyForeignKeys { get; set; }
            [XmlElement("Remove")]
            public List<RemoveForeignKeyXml> RemoveForeignKeys { get; set; }

            public TableXml()
            {
                AddForeignKeys = new List<AddForeignKeyXml>();
                ModifyForeignKeys = new List<ModifyForeignKeyXml>();
                RemoveForeignKeys = new List<RemoveForeignKeyXml>();
            }

            public TableXml(string name, 
                            List<AddForeignKeyXml> addForeignKey, 
                            List<ModifyForeignKeyXml> modifyForeignKeys, 
                            List<RemoveForeignKeyXml> removeForeignKeys)
            {
                Name = name;
                AddForeignKeys = addForeignKey;
                ModifyForeignKeys = modifyForeignKeys;
                RemoveForeignKeys = removeForeignKeys;
            }
        }

        public class AddForeignKeyXml
        {
            [XmlAttribute]
            public Int16 ServerIdDest { get; set; }
            [XmlAttribute]
            public string DatabaseDest { get; set; }
            [XmlAttribute]
            public string SchemaDest { get; set; }
            [XmlAttribute]
            public string TableDest { get; set; }
            [XmlElement("Collumn")]
            public List<CollumnXml> Collumns { get; set; }
            [XmlAttribute]
            public bool Active { get; set; }

            public AddForeignKeyXml()
            {
                Collumns = new List<CollumnXml>();
                Active = true;
            }

            public AddForeignKeyXml(Int16 serverIdDest, string databaseDest, string schemaDest, 
                                    string tableDest, List<CollumnXml> collumns, bool active)
            {
                Collumns = collumns;
                ServerIdDest = serverIdDest;
                DatabaseDest = databaseDest;
                SchemaDest = schemaDest;
                TableDest = tableDest;
                Active = active;
            }
        }

        public class ModifyForeignKeyXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public Int16 ServerIdDest { get; set; }
            [XmlAttribute]
            public string DatabaseDest { get; set; }
            [XmlAttribute]
            public string SchemaDest { get; set; }
            [XmlAttribute]
            public string TableDest { get; set; }
            [XmlElement("Collumn")]
            public List<CollumnXml> Collumns { get; set; }
            [XmlAttribute]
            public bool Active { get; set; }

            public ModifyForeignKeyXml()
            {
                Collumns = new List<CollumnXml>();
                Active = true;
            }

            public ModifyForeignKeyXml(string name, Int16 serverIdDest, string databaseDest, 
                                       string schemaDest, string tableDest, List<CollumnXml> collumns,
                                       bool active)
            {
                Name = name;
                ServerIdDest = serverIdDest;
                DatabaseDest = databaseDest;
                SchemaDest = schemaDest;
                TableDest = tableDest;
                Collumns = collumns;
                Active = active;
            }
        }

        public class RemoveForeignKeyXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlElement("Collumn")]
            public List<CollumnNameXml> Collumns { get; set; }
            [XmlAttribute]
            public bool Active { get; set; }

            public RemoveForeignKeyXml()
            {
                Collumns = new List<CollumnNameXml>();
                Active = true;
            }

            public RemoveForeignKeyXml(string name, List<CollumnNameXml> collumns, bool active)
            {
                Name = name;
                Collumns = collumns;
                Active = active;
            }
        }

        public class CollumnNameXml
        {
            [XmlAttribute]
            public string Name { get; set; }
        }

        public class CollumnXml
        {
            [XmlAttribute]
            public string Name { get; set; }
            [XmlAttribute]
            public string ColNameDest { get; set; }
        }
    }
}