﻿namespace DataCloner.Core.Tests
{
    public class ConfigurationTests
    {
        private readonly ProjectContainer _proj;

        public ConfigurationTests()
        {
            _proj = new ProjectContainer {Name = "MainApp"};
            _proj.ConnectionStrings.Add(new Connection(1, "PROD", "DataCloner.Data.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false"));
            _proj.ConnectionStrings.Add(new Connection(2, "UNI", "DataCloner.Data.QueryProviderMySql", "server=localhost;user id=root; password=cdxsza; database=mysql; pooling=false"));

            var table1 = new TableModifier
            {
                Name = "table1",
                IsStatic = false,
                DataBuilders = new List<DataBuilder>
                {
                    new DataBuilder
                    {
                        BuilderName = "Client.Builder.CreatePK",
                        Name = "col1"
                    }
                },
                DerativeTables = new DerativeTable
                {
                    GlobalAccess = DerivativeTableAccess.Forced,
                    GlobalCascade = true,
                    DerativeSubTables = new List<DerivativeSubTable>
                    {
                        new DerivativeSubTable
                        {
                            ServerId = "1",
                            Database = "db",
                            Schema = "dbo",
                            Table = "table2",
                            Access = DerivativeTableAccess.Denied
                        }
                    }
                }
            };

            table1.ForeignKeys.ForeignKeyAdd.Add(new ForeignKeyAdd
            {
                ServerId = "1",
                Database = "db",
                Schema = "dbo",
                Table = "table55",
                Columns = new List<ForeignKeyColumn>
                {
                    new ForeignKeyColumn
                    {
                        NameFrom = "col1",
                        NameTo = "col1"
                    },
                    new ForeignKeyColumn
                    {
                        NameFrom = "col2",
                        NameTo = "col2"
                    }
                }
            });

            table1.ForeignKeys.ForeignKeyRemove = new ForeignKeyRemove
            {
                Columns = new List<ForeignKeyRemoveColumn>
                {
                    new ForeignKeyRemoveColumn
                    {
                        Name = "col3"
                    },
                    new ForeignKeyRemoveColumn
                    {
                        Name = "col4"
                    }
                }
            };

            var server1 = new ServerModifier
            {
                Id = "1",
                Databases = new List<DatabaseModifier>
                {
                    new DatabaseModifier
                    {
                        Name = "db",
                        Schemas = new List<SchemaModifier>
                        {
                            new SchemaModifier
                            {
                                Name = "dbo",
                                Tables = new List<TableModifier> { table1 }
                            }
                        }
                    }
                }
            };

            var clonerBehaviour = new Behaviour
            {
                Id = 1,
                Name = "Basic clone",
                Description = "Only cloning besic data",
                //Servers = new List<ServerModifier> { server1 }
            };

            _proj.Behaviours.Add(clonerBehaviour);

            _proj.Maps = new List<Map>
            {
                new Map
                {
                     From = "UNI",
                     To = "FON",
                     UsableBehaviours = "1,2",
                     Variables = new List<Variable>
                     {
                         new Variable { Name = "", Value=""}
                     },
                     Roads = new List<Road>
                     {
                         new Road
                         {
                             ServerSrc = "1", SchemaSrc = "dbo", DatabaseSrc = "myDB",
                             ServerDst = "1", SchemaDst = "dbo", DatabaseDst = "myDB"
                         }
                     }
                }
            };
        }

        [Fact]
        public async void SaveLoadConfigFile()
        {
            const string fileName = "dcSaveLoadConfigFile.config";
            
            _proj.Save(fileName);
            var configLoaded = await ProjectContainer.LoadAsync(fileName);
            //var temp = _config.SerializeXml();

            File.Delete(fileName);
        }
    }
}
