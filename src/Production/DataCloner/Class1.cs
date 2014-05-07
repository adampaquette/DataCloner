using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DataCloner.Serialization;

namespace Class
{
    public class main
    {
        static int Main(string[] args)
        {
            var ti = new DataCloner.DataClasse.TableIdentifier();
            ti.DatabaseName = "botnet";
            ti.SchemaName = "botnet";
            ti.TableName = "link";

            var ri = new DataCloner.DataClasse.RowIdentifier();
            ri.TableIdentifier = ti;
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

            configTest();

            //var a = new DataCloner.DataCloner();
            //a.SQLTraveler(null, true, true);


            return 0;
        }

        public static void configTest()
        {
            var config = new ConfigurationXML();

            //ConnectionXML
            //=============
            var cs = new ConnectionXML(1, "sql1", "DataCloner.DataAccess.QueryProviderMySQL", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            config.ConnectionStrings.Add(cs);

            //StaticTableXML 
            //==============            
            var schema1 = new StaticTableXML.SchemaXML();
            schema1.Name = "dbo";
            schema1.Tables.Add(new StaticTableXML.TableXML("table1", true));
            schema1.Tables.Add(new StaticTableXML.TableXML("table2", true));

            var schema2 = new StaticTableXML.SchemaXML();
            schema2.Name = "master";
            schema2.Tables.Add(new StaticTableXML.TableXML("person", true));
            schema2.Tables.Add(new StaticTableXML.TableXML("house", true));

            var listSchema = new List<StaticTableXML.SchemaXML>();
            listSchema.Add(schema1);
            listSchema.Add(schema2);

            var database = new StaticTableXML.DatabaseXML(listSchema, "db");
            var server = new StaticTableXML.ServerXML(new List<StaticTableXML.DatabaseXML>() { database }, 1);
            var server2 = new StaticTableXML.ServerXML(new List<StaticTableXML.DatabaseXML>() {database }, 2);
            var staticTable = new StaticTableXML(new List<StaticTableXML.ServerXML>() { server, server2 });
            config.StaticTables = staticTable;

            //ManyToManyRelationshipsTablesXML
            //===============================
            var schemaManyToMany = new ManyToManyRelationshipsTablesXML.SchemaXML();
            schemaManyToMany.Name = "dbo";
            schemaManyToMany.Tables.Add(new ManyToManyRelationshipsTablesXML.TableXML("table1", true));
            schemaManyToMany.Tables.Add(new ManyToManyRelationshipsTablesXML.TableXML("table2", true));

            var listSchemaManyToMany = new List<ManyToManyRelationshipsTablesXML.SchemaXML>();
            listSchemaManyToMany.Add(schemaManyToMany);

            var databaseManyToMany = new ManyToManyRelationshipsTablesXML.DatabaseXML(listSchemaManyToMany, "db");
            var serverManyToMany = new ManyToManyRelationshipsTablesXML.ServerXML(new List<ManyToManyRelationshipsTablesXML.DatabaseXML>() { databaseManyToMany }, 1);
            var manyToManyRelationshipsTable = new ManyToManyRelationshipsTablesXML(new List<ManyToManyRelationshipsTablesXML.ServerXML>() { serverManyToMany });
            config.ManyToManyRelationshipsTables = manyToManyRelationshipsTable;

            //DerivativeTableAccess
            //=====================
            var schemaDerivativeTableAccess = new DerivativeTableAccessXML.SchemaXML();
            schemaDerivativeTableAccess.Name = "dbo";
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXML.TableXML("table1", DerivativeTableAccessXML.AccessXML.Denied, true));
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXML.TableXML("table2", DerivativeTableAccessXML.AccessXML.Forced, true));

            var listSchemaDerivativeTableAccess = new List<DerivativeTableAccessXML.SchemaXML>();
            listSchemaDerivativeTableAccess.Add(schemaDerivativeTableAccess);

            var databaseDerivativeTableAccess = new DerivativeTableAccessXML.DatabaseXML(listSchemaDerivativeTableAccess, "db");
            var serverDerivativeTableAccess = new DerivativeTableAccessXML.ServerXML(new List<DerivativeTableAccessXML.DatabaseXML>() { databaseDerivativeTableAccess }, 1);
            var derivativeTableAccess = new DerivativeTableAccessXML(new List<DerivativeTableAccessXML.ServerXML>() { serverDerivativeTableAccess });
            config.DerivativeTableAccess = derivativeTableAccess;

            //ForeignKeys
            //===========
            var serverfk1 = new ForeignKeysXML.ServerXML();
            var dbfk1 = new ForeignKeysXML.DatabaseXML();
            var schemafk1 = new ForeignKeysXML.SchemaXML();
            var tablefk1 = new ForeignKeysXML.TableXML();
            var AddForeignKeyXMLfk1 = new ForeignKeysXML.AddForeignKeyXML();
            var RemoveForeignKeyXMLfk1 = new ForeignKeysXML.RemoveForeignKeyXML();
            var RemoveForeignKeyXMLfk2 = new ForeignKeysXML.RemoveForeignKeyXML();
            var CollumnNameXMLfk1 = new ForeignKeysXML.CollumnNameXML();
            var CollumnXMLfk1 = new ForeignKeysXML.CollumnXML();
            var CollumnXMLfk2 = new ForeignKeysXML.CollumnXML();

            CollumnXMLfk1.ColNameDest = "col1";
            CollumnXMLfk1.Name = "col1";

            CollumnXMLfk2.ColNameDest = "col2";
            CollumnXMLfk2.Name = "col2";

            CollumnNameXMLfk1.Name = "col3";

            AddForeignKeyXMLfk1.ServerIdDest = 1;
            AddForeignKeyXMLfk1.DatabaseDest = "db1";
            AddForeignKeyXMLfk1.SchemaDest = "dbo";
            AddForeignKeyXMLfk1.TableDest = "table1";
            AddForeignKeyXMLfk1.Collumns.Add(CollumnXMLfk1);
            AddForeignKeyXMLfk1.Collumns.Add(CollumnXMLfk2);

            RemoveForeignKeyXMLfk1.Name = "fk1";

            RemoveForeignKeyXMLfk2.Collumns.Add(CollumnNameXMLfk1);

            tablefk1.Name = "table1";
            tablefk1.AddForeignKeys.Add(AddForeignKeyXMLfk1);
            tablefk1.RemoveForeignKeys.Add(RemoveForeignKeyXMLfk1);
            tablefk1.RemoveForeignKeys.Add(RemoveForeignKeyXMLfk2);

            schemafk1.Name = "dbo";
            schemafk1.Tables.Add(tablefk1);

            dbfk1.Name = "db1";
            dbfk1.Schemas.Add(schemafk1);

            serverfk1.Id = 1;
            serverfk1.Databases.Add(dbfk1);

            config.ForeignKeys.Servers.Add(serverfk1);

            //Save / load from file
            //=====================
            string serialized = SerizlizeXML(config);
            config.Save();

            ConfigurationXML configLoaded;
            configLoaded = ConfigurationXML.Load();
        }

        public static string SerizlizeXML<T>(T obj)
        {
            var xs = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var sw = new System.IO.StringWriter();
            var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(sw, obj, ns);
            return sw.ToString();
        }
    }
}