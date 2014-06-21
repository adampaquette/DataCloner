using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;
using DataCloner.Enum;

namespace Class
{
    public class main
    {
        static int Main(string[] args)
        {
            /*ConfigTest();
            StaticTableTest();
            ExtensionsTest();*/
            DeriavativeTableTest();

            return 0;
        }

        public void GeneralDBTest()
        { 
            var ti = new TableIdentifier { DatabaseName = "botnet", SchemaName = "botnet", TableName = "link" };
            var ri = new RowIdentifier { TableIdentifier = ti };
            ri.Columns.Add("fromPageHostId", 6);
            ri.Columns.Add("fromPageId", 4);

            //var m = new DataCloner.DataAccess.QueryProviderMySql("server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false");
            //var dt = m.GetFK(ti);
            //m.Select(ri);

            //var conn = new System.Data.SqlClient.SqlConnection("Data Source=une_sql_pgis;Initial Catalog=PGISCBL;Integrated Security=SSPI;");
            //conn.Open();
            //if (conn.Database != "PGISCBL")
            //    conn.ChangeDatabase("PGISCBL");
            ////var dt = conn.GetSchema("Columns");
            //conn.Close();

            //var a = new DataCloner.DataCloner();
            //a.SQLTraveler(null, true, true);
        }

        public static void DeriavativeTableTest()
        {
            var dt = new DerivativeTable();
            var tTo1 = new DerivativeTable.TableTo()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = AccessXml.Forced,
                Cascade = true
            };

            var tTo2 = new DerivativeTable.TableTo()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table3",
                Access = AccessXml.Denied,
                Cascade = false
            };

            dt.Add(1, "db", "dbo", "table1",  tTo1);
            dt.Add(1, "db", "dbo", "table1", tTo2);
            dt.Add(1, "db", "dbo", "table2", tTo1);

            var ms = new MemoryStream();
            dt.Serialize(ms);

            ms.Position = 0;
            var dtDeserialize = DerivativeTable.Deserialize(ms);
            var msDeserialize = new MemoryStream();
            dtDeserialize.Serialize(msDeserialize);

            if (!ms.ToArray().SequenceEqual(msDeserialize.ToArray()))
                throw new Exception("");
        }

        public static void ExtensionsTest()
        {
            var t = new int[] { 1, 2, 3, 4 };

            //RemoveAt
            var m = t.RemoveAt(0);
            if (!m.SequenceEqual(new int[] { 2, 3, 4 }))
                throw new Exception("");

            m = m.RemoveAt(2);
            if (!m.SequenceEqual(new int[] { 2, 3 }))
                throw new Exception("");

            //Remove
            var n = t.Remove(4);
            if (!n.SequenceEqual(new int[] { 1, 2, 3 }))
                throw new Exception("");

            //Add
            var l = t.Add(5).Add(1);
            if (!l.SequenceEqual(new int[] { 1, 2, 3, 4, 5, 1 }))
                throw new Exception("");
        }

        public static void StaticTableTest()
        {
            var values = new string[] { "table1", "table2", "table3", "table4" };
            var st = new StaticTable();

            st.Add(1, "database", "schema", "TABLE1");
            st.Add(1, "dataBASE", "schema", "taBle1");
            st.Add(1, "database", "schema", "table2");
            st.Add(1, "database", "schema", "taBle3");
            st.Add(1, "database", "schema", "table4");

            st.Add(2, "database1", "schema", "table1");
            st.Add(2, "database1", "schema", "table1");
            st.Add(2, "database2", "schema", "table2");
            st.Add(2, "database2", "schema", "table3");
            st.Add(3, "database3", "schema", "table4");

            if (!st[1, "DATAbase", "sChema"].SequenceEqual(values))
                throw new Exception("");

            if (st[9, "DATAbase", "sChema"] != null)
                throw new Exception("Devrait être null");

            if (!st.Contains(3, "dATabase3", "sCHEMA", "table4"))
                throw new Exception("");

            if (!st.Remove(3, "dATabase3", "sCHEMA", "table4"))
                throw new Exception("");

            if (st.Contains(3, "dATabase3", "sCHEMA", "table4"))
                throw new Exception("");

            if (st[3, "dATabase3", "sCHEMA"] != null)
                throw new Exception("");
        }


        public static void ConfigTest()
        {
            var config = new ConfigurationXml();

            //ConnectionXML
            //=============
            var cs = new ConnectionXml(1, "DataCloner.DataAccess.QueryProviderMySQL", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false", 1);
            config.ConnectionStrings.Add(cs);

            //StaticTableXml
            //==============            
            var schema1 = new StaticTableXml.SchemaXml { Name = "dbo" };
            schema1.Tables.Add(new StaticTableXml.TableXml("table1", true));
            schema1.Tables.Add(new StaticTableXml.TableXml("table2", true));

            var schema2 = new StaticTableXml.SchemaXml { Name = "master" };
            schema2.Tables.Add(new StaticTableXml.TableXml("person", true));
            schema2.Tables.Add(new StaticTableXml.TableXml("house", true));

            var listSchema = new List<StaticTableXml.SchemaXml> { schema1, schema2 };

            var database = new StaticTableXml.DatabaseXml(listSchema, "db");
            var server = new StaticTableXml.ServerXml(new List<StaticTableXml.DatabaseXml> { database }, 1);
            var server2 = new StaticTableXml.ServerXml(new List<StaticTableXml.DatabaseXml> { database }, 2);
            var staticTable = new StaticTableXml(new List<StaticTableXml.ServerXml> { server, server2 });
            config.StaticTables = staticTable;

            //DataBuilderXml 
            //==============  
            var col1DB = new DataBuilderXml.ColumnXml("ID", "DataCloner.Builder.Generic", "CreateID", true);
            var col2DB = new DataBuilderXml.ColumnXml("NAS", "Client.Builder.Builder1", "CreateNAS", true);

            var schemaDB1 = new DataBuilderXml.SchemaXml { Name = "master" };
            schemaDB1.Tables.Add(new DataBuilderXml.TableXml("person", new List<DataBuilderXml.ColumnXml>() { col1DB, col2DB }));

            var listSchemaDB = new List<DataBuilderXml.SchemaXml> { schemaDB1 };

            var databaseDB = new DataBuilderXml.DatabaseXml(listSchemaDB, "db");
            var serverDB = new DataBuilderXml.ServerXml(new List<DataBuilderXml.DatabaseXml> { databaseDB }, 1);
            var dataBuilders = new DataBuilderXml(new List<DataBuilderXml.ServerXml> { serverDB });
            config.DataBuilders = dataBuilders;

            //DerivativeTableAccessXml
            //========================
            var toDTA = new DerivativeTableAccessXml.TableToXml("table1", AccessXml.Forced, true, true);
            var lstToDTA = new List<DerivativeTableAccessXml.TableToXml>() { toDTA };
            var schemaDerivativeTableAccess = new DerivativeTableAccessXml.SchemaXml { Name = "dbo" };
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXml.TableFromXml("table1", AccessXml.Denied, true, true, null ));
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXml.TableFromXml("table2", AccessXml.Forced, true, false, null));
            schemaDerivativeTableAccess.Tables.Add(new DerivativeTableAccessXml.TableFromXml("table3", AccessXml.NotSet, true, false, lstToDTA));

            var listSchemaDerivativeTableAccess = new List<DerivativeTableAccessXml.SchemaXml>
            {
                schemaDerivativeTableAccess
            };

            var databaseDerivativeTableAccess = new DerivativeTableAccessXml.DatabaseXml(listSchemaDerivativeTableAccess, "db");
            var serverDerivativeTableAccess = new DerivativeTableAccessXml.ServerXml(new List<DerivativeTableAccessXml.DatabaseXml> { databaseDerivativeTableAccess }, 1);
            var derivativeTableAccess = new DerivativeTableAccessXml(new List<DerivativeTableAccessXml.ServerXml> { serverDerivativeTableAccess });
            config.DerivativeTableAccess = derivativeTableAccess;

            //ForeignKeysXml
            //==============
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
            var serialized = config.SerializeXml();
            config.Save();

            ConfigurationXml configLoaded;
            configLoaded = ConfigurationXml.Load();
        }
    }
}