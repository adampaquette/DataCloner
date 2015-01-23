using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DataCloner;
using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Framework;

using Class;
using Xunit;

namespace DataCloner.Tests
{
    public class ConfigurationXmlTests
    {
        private ConfigurationXml _config;

        public ConfigurationXmlTests()
        {
            _config = new ConfigurationXml();

            var cs = new ConnectionXml(1, "PROD", "DataCloner.DataAccess.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false", 1);
            var cs2 = new ConnectionXml(2, "UNI", "DataCloner.DataAccess.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false", 1);
            _config.ConnectionStrings.Add(cs);
            _config.ConnectionStrings.Add(cs2);

            var dataColumnBuilder1 = new TableModifiersXml.DataColumnBuilderXml()
            {
                BuilderName = "Client.Builder.CreatePK",
                Name = "col1"
            };

            var dataColumnBuilder2 = new TableModifiersXml.DataColumnBuilderXml()
            {
                BuilderName = "Client.Builder.CreateNAS",
                Name = "col4"
            };

            var derivativeTable1 = new TableModifiersXml.DerivativeSubTableXml()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Denied
            };

            var fkAdd1 = new TableModifiersXml.ForeignKeyAddXml()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table55",
                Columns = new List<TableModifiersXml.ForeignKeyColumnXml>()
                { 
                    new TableModifiersXml.ForeignKeyColumnXml()
                    {
                        NameFrom = "col1", 
                        NameTo = "col1"
                    },
                    new TableModifiersXml.ForeignKeyColumnXml()
                    {
                        NameFrom = "col2", 
                        NameTo = "col2"
                    }
                }
            };

            var fkRemove = new TableModifiersXml.ForeignKeyRemoveXml()
            {
                Columns = new List<TableModifiersXml.ForeignKeyRemoveColumnXml>
                {
                    new TableModifiersXml.ForeignKeyRemoveColumnXml
                    {
                        Name = "col3"
                    },
                    new TableModifiersXml.ForeignKeyRemoveColumnXml
                    {
                        Name = "col4"
                    }
                }
            };

            var table1 = new TableModifiersXml.TableModifierXml();
            table1.Name = "table1";
            table1.IsStatic = false;
            table1.DataBuilders.Add(dataColumnBuilder1);
            table1.DerativeTablesConfig.GlobalAccess = DerivativeTableAccess.Forced;
            table1.DerativeTablesConfig.Cascade = true;
            table1.DerativeTablesConfig.DerativeTables.Add(derivativeTable1);
            table1.ForeignKeys.ForeignKeyAdd.Add(fkAdd1);
            table1.ForeignKeys.ForeignKeyRemove.Add(fkRemove);

            var schema1 = new TableModifiersXml.SchemaXml()
            {
                Name = "dbo",
                Tables = new List<TableModifiersXml.TableModifierXml>() { table1 }
            };

            var database1 = new TableModifiersXml.DatabaseXml()
            {
                Name = "db",
                Schemas = new List<TableModifiersXml.SchemaXml>() { schema1 }
            };

            var server1 = new TableModifiersXml.ServerXml()
            {
                Id = 1,
                Databases = new List<TableModifiersXml.DatabaseXml>() { database1 }
            };

            _config.TableModifiers.Servers.Add(server1);
        }

        [Fact]
        public void SaveLoadConfigFile()
        {
            string fileName = "dcSaveLoadConfigFile.config";

            Assert.DoesNotThrow(() =>
            {
                _config.Save(fileName);
                var configLoaded = ConfigurationXml.Load(fileName);
            });

            File.Delete(fileName);
        }

        [Fact]
        public void ConnectionStringServerIdInvalid()
        {
            var config = new ConfigurationXml();
            config.ConnectionStrings.Add(new ConnectionXml(0, "", "", "", 0));

            Assert.Throws(typeof(InvalidDataException), () => { config.Validate(); });
        }

        [Fact]
        public void ConnectionStringNotFoundFromSameConfigAsId()
        {
            var config = new ConfigurationXml();
            config.ConnectionStrings.Add(new ConnectionXml(1, "", "", "", 2));

            Assert.Throws(typeof(InvalidDataException), () => { config.Validate(); });
        }
    }
}
