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
        private Configuration _config;

        public ConfigurationXmlTests()
        {
            _config = new Configuration();

            var cs = new DataClasse.Configuration.Connection(1, "PROD", "DataCloner.DataAccess.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false", 1);
            var cs2 = new DataClasse.Configuration.Connection(2, "UNI", "DataCloner.DataAccess.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false", 1);
            _config.ConnectionStrings.Add(cs);
            _config.ConnectionStrings.Add(cs2);

            var dataColumnBuilder1 = new Modifiers.DataBuilder()
            {
                BuilderName = "Client.Builder.CreatePK",
                Name = "col1"
            };

            var dataColumnBuilder2 = new Modifiers.DataBuilder()
            {
                BuilderName = "Client.Builder.CreateNAS",
                Name = "col4"
            };

            var derivativeTable1 = new Modifiers.DerivativeSubTable()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table2",
                Access = DerivativeTableAccess.Denied
            };

            var fkAdd1 = new Modifiers.ForeignKeyAdd()
            {
                ServerId = 1,
                Database = "db",
                Schema = "dbo",
                Table = "table55",
                Columns = new List<Modifiers.ForeignKeyColumn>()
                { 
                    new Modifiers.ForeignKeyColumn()
                    {
                        NameFrom = "col1", 
                        NameTo = "col1"
                    },
                    new Modifiers.ForeignKeyColumn()
                    {
                        NameFrom = "col2", 
                        NameTo = "col2"
                    }
                }
            };

            var fkRemove = new Modifiers.ForeignKeyRemove()
            {
                Columns = new List<Modifiers.ForeignKeyRemoveColumn>
                {
                    new Modifiers.ForeignKeyRemoveColumn
                    {
                        Name = "col3"
                    },
                    new Modifiers.ForeignKeyRemoveColumn
                    {
                        Name = "col4"
                    }
                }
            };

            var table1 = new Modifiers.TableModifier();
            table1.Name = "table1";
            table1.IsStatic = false;
            table1.DataBuilders.Add(dataColumnBuilder1);
            table1.DerativeTablesConfig.GlobalAccess = DerivativeTableAccess.Forced;
            table1.DerativeTablesConfig.Cascade = true;
            table1.DerativeTablesConfig.DerativeTables.Add(derivativeTable1);
            table1.ForeignKeys.ForeignKeyAdd.Add(fkAdd1);
            table1.ForeignKeys.ForeignKeyRemove.Add(fkRemove);

            var schema1 = new Modifiers.SchemaModifier()
            {
                Name = "dbo",
                Tables = new List<Modifiers.TableModifier>() { table1 }
            };

            var database1 = new Modifiers.DatabaseModifier()
            {
                Name = "db",
                Schemas = new List<Modifiers.SchemaModifier>() { schema1 }
            };

            var server1 = new Modifiers.ServerModifier()
            {
                Id = 1,
                Databases = new List<Modifiers.DatabaseModifier>() { database1 }
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
                var configLoaded = Configuration.Load(fileName);
            });

            File.Delete(fileName);
        }

        [Fact]
        public void ConnectionStringServerIdInvalid()
        {
            var config = new Configuration();
            config.ConnectionStrings.Add(new DataClasse.Configuration.Connection(0, "", "", "", 0));

            Assert.Throws(typeof(InvalidDataException), () => { config.Validate(); });
        }

        [Fact]
        public void ConnectionStringNotFoundFromSameConfigAsId()
        {
            var config = new Configuration();
            config.ConnectionStrings.Add(new DataClasse.Configuration.Connection(1, "", "", "", 2));

            Assert.Throws(typeof(InvalidDataException), () => { config.Validate(); });
        }
    }
}
