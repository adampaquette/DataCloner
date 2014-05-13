﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DataCloner.DataClasse;
using DataCloner.Serialization;

namespace Class
{
    public class main
    {
        static int Main(string[] args)
        {
            var ti = new TableIdentifier {DatabaseName = "botnet", SchemaName = "botnet", TableName = "link"};
            var ri = new RowIdentifier {TableIdentifier = ti};
            ri.Columns.Add("fromPageHostId", 6);
            ri.Columns.Add("fromPageId", 4);

            //var m = new DataCloner.DataAccess.QueryDatabaseMySQL("server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            //var dt = m.GetFK(ti);
            //m.Select(ri);


            //var conn = new System.Data.SqlClient.SqlConnection("Data Source=une_sql_pgis;Initial Catalog=PGISCBL;Integrated Security=SSPI;");
            //conn.Open();
            //if (conn.Database != "PGISCBL")
            //    conn.ChangeDatabase("PGISCBL");
            ////var dt = conn.GetSchema("Columns");
            //conn.Close();

            ConfigTest();

            //var a = new DataCloner.DataCloner();
            //a.SQLTraveler(null, true, true);


            return 0;
        }

        public static void ConfigTest()
        {
            var config = new ConfigurationXml();

            //ConnectionXML
            //=============
            var cs = new ConnectionXml(1, "sql1", "DataCloner.DataAccess.QueryProviderMySQL", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            config.ConnectionStrings.Add(cs);

            //StaticTableXML 
            //==============            
            var schema1 = new StaticTableXml.SchemaXml {Name = "dbo"};
            schema1.Tables.Add(new StaticTableXml.TableXml("table1", true));
            schema1.Tables.Add(new StaticTableXml.TableXml("table2", true));

            var schema2 = new StaticTableXml.SchemaXml {Name = "master"};
            schema2.Tables.Add(new StaticTableXml.TableXml("person", true));
            schema2.Tables.Add(new StaticTableXml.TableXml("house", true));

            var listSchema = new List<StaticTableXml.SchemaXml> {schema1, schema2};

            var database = new StaticTableXml.DatabaseXml(listSchema, "db");
            var server = new StaticTableXml.ServerXml(new List<StaticTableXml.DatabaseXml> { database }, 1);
            var server2 = new StaticTableXml.ServerXml(new List<StaticTableXml.DatabaseXml> {database }, 2);
            var staticTable = new StaticTableXml(new List<StaticTableXml.ServerXml> { server, server2 });
            config.StaticTables = staticTable;

            //ManyToManyRelationshipsTablesXML
            //===============================
            var schemaManyToMany = new ManyToManyRelationshipsTablesXml.SchemaXml {Name = "dbo"};
            schemaManyToMany.Tables.Add(new ManyToManyRelationshipsTablesXml.TableXml("table1", true));
            schemaManyToMany.Tables.Add(new ManyToManyRelationshipsTablesXml.TableXml("table2", true));

            var listSchemaManyToMany = new List<ManyToManyRelationshipsTablesXml.SchemaXml> {schemaManyToMany};

            var databaseManyToMany = new ManyToManyRelationshipsTablesXml.DatabaseXml(listSchemaManyToMany, "db");
            var serverManyToMany = new ManyToManyRelationshipsTablesXml.ServerXml(new List<ManyToManyRelationshipsTablesXml.DatabaseXml> { databaseManyToMany }, 1);
            var manyToManyRelationshipsTable = new ManyToManyRelationshipsTablesXml(new List<ManyToManyRelationshipsTablesXml.ServerXml> { serverManyToMany });
            config.ManyToManyRelationshipsTables = manyToManyRelationshipsTable;

            //DerivativeTableAccess
            //=====================
            var schemaDerivativeTableAccess = new DerivativeTableAccessXml.SchemaXml {Name = "dbo"};
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXml.TableXml("table1", DerivativeTableAccessXml.AccessXml.Denied, true));
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXml.TableXml("table2", DerivativeTableAccessXml.AccessXml.Forced, true));

            var listSchemaDerivativeTableAccess = new List<DerivativeTableAccessXml.SchemaXml>
            {
                schemaDerivativeTableAccess
            };

            var databaseDerivativeTableAccess = new DerivativeTableAccessXml.DatabaseXml(listSchemaDerivativeTableAccess, "db");
            var serverDerivativeTableAccess = new DerivativeTableAccessXml.ServerXml(new List<DerivativeTableAccessXml.DatabaseXml> { databaseDerivativeTableAccess }, 1);
            var derivativeTableAccess = new DerivativeTableAccessXml(new List<DerivativeTableAccessXml.ServerXml> { serverDerivativeTableAccess });
            config.DerivativeTableAccess = derivativeTableAccess;

            //ForeignKeys
            //===========
            var serverfk1 = new ForeignKeysXml.ServerXml();
            var dbfk1 = new ForeignKeysXml.DatabaseXml();
            var schemafk1 = new ForeignKeysXml.SchemaXml();
            var tablefk1 = new ForeignKeysXml.TableXml();
            var addForeignKeyXmLfk1 = new ForeignKeysXml.AddForeignKeyXml();
            var removeForeignKeyXmLfk1 = new ForeignKeysXml.RemoveForeignKeyXml();
            var removeForeignKeyXmLfk2 = new ForeignKeysXml.RemoveForeignKeyXml();
            var collumnNameXmLfk1 = new ForeignKeysXml.CollumnNameXml();
            var collumnXmLfk1 = new ForeignKeysXml.CollumnXml();
            var collumnXmLfk2 = new ForeignKeysXml.CollumnXml();

            collumnXmLfk1.ColNameDest = "col1";
            collumnXmLfk1.Name = "col1";

            collumnXmLfk2.ColNameDest = "col2";
            collumnXmLfk2.Name = "col2";

            collumnNameXmLfk1.Name = "col3";

            addForeignKeyXmLfk1.ServerIdDest = 1;
            addForeignKeyXmLfk1.DatabaseDest = "db1";
            addForeignKeyXmLfk1.SchemaDest = "dbo";
            addForeignKeyXmLfk1.TableDest = "table1";
            addForeignKeyXmLfk1.Collumns.Add(collumnXmLfk1);
            addForeignKeyXmLfk1.Collumns.Add(collumnXmLfk2);

            removeForeignKeyXmLfk1.Name = "fk1";

            removeForeignKeyXmLfk2.Collumns.Add(collumnNameXmLfk1);

            tablefk1.Name = "table1";
            tablefk1.AddForeignKeys.Add(addForeignKeyXmLfk1);
            tablefk1.RemoveForeignKeys.Add(removeForeignKeyXmLfk1);
            tablefk1.RemoveForeignKeys.Add(removeForeignKeyXmLfk2);

            schemafk1.Name = "dbo";
            schemafk1.Tables.Add(tablefk1);

            dbfk1.Name = "db1";
            dbfk1.Schemas.Add(schemafk1);

            serverfk1.Id = 1;
            serverfk1.Databases.Add(dbfk1);

            config.ForeignKeys.Servers.Add(serverfk1);

            //Save / load from file
            //=====================
            var serialized = SerizlizeXml(config);
            config.Save();

            ConfigurationXml configLoaded;
            configLoaded = ConfigurationXml.Load();
        }

        public static string SerizlizeXml<T>(T obj)
        {
            var xs = new XmlSerializer(obj.GetType());
            var sw = new StringWriter();
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(sw, obj, ns);
            return sw.ToString();
        }
    }
}