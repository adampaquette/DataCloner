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
            var config = new Configuration();

            //ConnectionStrings
            var cs = new Connection(1, "sql1", "DataCloner.DataAccess.QueryProviderMySQL", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            config.ConnectionStrings.Add(cs);

            //Static tables   
            var schema2 = new StaticTable.SchemaXML();
            schema2.Name = "master";
            schema2.Tables.Add(new StaticTable.TableXML("person", true));
            schema2.Tables.Add(new StaticTable.TableXML("house", true));
            
            var schema1 = new StaticTable.SchemaXML();
            schema1.Name = "dbo";
            schema1.Tables.Add(new StaticTable.TableXML("table1", true));
            schema1.Tables.Add(new StaticTable.TableXML("table2", true));

            var listSchema = new List<StaticTable.SchemaXML>();
            listSchema.Add(schema1);
            listSchema.Add(schema2);

            var database = new StaticTable.DatabaseXML(listSchema, "db");
            var server = new StaticTable.ServerXML(new List<StaticTable.DatabaseXML>() { database }, 1);
            var server2 = new StaticTable.ServerXML(new List<StaticTable.DatabaseXML>() {database }, 2);
            var staticTable = new StaticTable(new List<StaticTable.ServerXML>() { server, server2 });
            config.StaticTables = staticTable;

            //manyToManyRelationshipsTable
            var schemaManyToMany = new ManyToManyRelationshipsTable.SchemaXML();
            schemaManyToMany.Name = "dbo";
            schemaManyToMany.Tables.Add(new ManyToManyRelationshipsTable.TableXML("table1", true));
            schemaManyToMany.Tables.Add(new ManyToManyRelationshipsTable.TableXML("table2", true));

            var listSchemaManyToMany = new List<ManyToManyRelationshipsTable.SchemaXML>();
            listSchemaManyToMany.Add(schemaManyToMany);

            var databaseManyToMany = new ManyToManyRelationshipsTable.DatabaseXML(listSchemaManyToMany, "db");
            var serverManyToMany = new ManyToManyRelationshipsTable.ServerXML(new List<ManyToManyRelationshipsTable.DatabaseXML>() { databaseManyToMany }, 1);
            var manyToManyRelationshipsTable = new ManyToManyRelationshipsTable(new List<ManyToManyRelationshipsTable.ServerXML>() { serverManyToMany });
            config.ManyToManyRelationshipsTable = manyToManyRelationshipsTable;

            //DerivativeTableAccess
            var schemaDerivativeTableAccess = new DerivativeTableAccess.SchemaXML();
            schemaDerivativeTableAccess.Name = "dbo";
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccess.TableXML("table1", DerivativeTableAccess.AccessXML.Denied, true));
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccess.TableXML("table2", DerivativeTableAccess.AccessXML.Forced, true));

            var listSchemaDerivativeTableAccess = new List<DerivativeTableAccess.SchemaXML>();
            listSchemaDerivativeTableAccess.Add(schemaDerivativeTableAccess);

            var databaseDerivativeTableAccess = new DerivativeTableAccess.DatabaseXML(listSchemaDerivativeTableAccess, "db");
            var serverDerivativeTableAccess = new DerivativeTableAccess.ServerXML(new List<DerivativeTableAccess.DatabaseXML>() { databaseDerivativeTableAccess }, 1);
            var derivativeTableAccess = new DerivativeTableAccess(new List<DerivativeTableAccess.ServerXML>() { serverDerivativeTableAccess });
            config.DerivativeTableAccess = derivativeTableAccess;

            //Save / load from file
            string serialized = SerizlizeXML(config);
            config.Save();

            Configuration configLoaded;
            configLoaded = Configuration.Load();
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