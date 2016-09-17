using DataCloner.Core.Configuration;
using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using DataCloner.Core.IntegrationTests;

namespace DataCloner.Core.Debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var epb = new ExecutionPlanBuilderTest();

                foreach (var connection in DatabaseInitializer.Connections)
                {
                    var conn = connection[0] as Connection;

                    epb.CloningDependencies_With_DefaultConfig(conn);
                    epb.CloningDerivatives_With_GlobalAccessDenied(conn);
                    epb.CloningDerivatives_With_GlobalAccessForced(conn);
                    epb.CloningDerivatives_With_DerivativeSubTableAccessForced(conn);
                    epb.CloningDerivatives_With_DerivativeSubTableAccessDenied(conn);
                    epb.Cloning_With_StaticTable(conn);
                    epb.Cloning_Should_NotCloneDerivativeOfDependancy(conn);
                    epb.Cloning_With_ForeignKeyAdd(conn);
                    epb.Cloning_With_ForeignKeyRemove(conn);
                    epb.Cloning_With_DataBuilder(conn);
                }

                CreateConfiguration();
                TestSerializingConfiguration();
                TestBehaviorBuilder();
            }
            catch (System.Exception)
            {
                throw;
            }

            Console.WriteLine("All tests passed!");
            Console.ReadKey();
        }

        public static ConfigurationProject CreateConfiguration()
        {
            var project = new ConfigurationProject { Name = "MainApp" };

            //ConnectionStrings
            project.ConnectionStrings.AddRange(new List<Connection>
            {
                new Connection(1, "UNI", "MySql.Data.MySqlClient", "server=UNI;user Id=root; password=toor; database=master; pooling=true"),
                new Connection(2, "FON", "System.Data.Sqlite", "server=FON;user Id=root; password=toor; database=master; pooling=true"),
                new Connection(3, "PROD_PGIS", "System.Data.Oracle", "server=PROD_PGIS;user Id=root; password=toor; database=master; pooling=true"),
                new Connection(4, "PROD_ARIEL", "System.Data.PosgreSql", "server=PROD_ARIEL;user Id=root; password=toor; database=master; pooling=true"),
                new Connection(5, "PROD_SIEBEL", "System.Data.MsSql", "server=PROD_SIEBEL;user Id=root; password=toor; database=master; pooling=true")
            });

            //Variables
            project.Variables = new List<Variable>
            {
                new Variable { Name = "PGIS_FROM", Server = 1, Database = "pgis", Schema = "dbo" },
                new Variable { Name = "ARIEL_FROM", Server = 1, Database = "ariel", Schema = "dbo" },
                new Variable { Name = "SIEBEL_FROM", Server = 1, Database = "siebel", Schema = "dbo" },
                new Variable { Name = "PGIS_TO", Server = 1, Database = "pgis", Schema = "dbo" },
                new Variable { Name = "ARIEL_TO", Server = 1, Database = "ariel", Schema = "dbo" },
                new Variable { Name = "SIEBEL_TO", Server = 1, Database = "siebel", Schema = "dbo" },
            };

            //Templates
            var pgisDbo = new DbSettings
            {
                Id = 1,
                Var = "PGIS_FROM",
                Description = "Configuration par défaut du serveur PGIS, schéma DBO.",
                Tables = new List<Table>
                {
                    new Table { Name = "domaineValeur", IsStatic = NullableBool.True},
                    new Table
                    {
                        Name = "transmission",
                        DerativeTableGlobal = new DerivativeTableGlobal()
                        {
                            GlobalAccess = DerivativeTableAccess.Forced,
                            GlobalCascade = NullableBool.True,
                            DerivativeTables = new List<DerivativeTable>
                            {
                                 new DerivativeTable { DestinationVar = "PGIS_TO", Name = "demande" }
                            }
                        },
                        ForeignKeys = new ForeignKeys
                        {
                             ForeignKeyAdd = new List<ForeignKeyAdd>
                             {
                                 new ForeignKeyAdd
                                 {
                                     DestinationVar = "SIEBEL_TO",
                                     TableTo = "s_srv_req",
                                     Columns = new List<ForeignKeyColumn> { new ForeignKeyColumn { NameFrom = "noreferencetransmission", NameTo = "sr_num" } }
                                 }
                             }
                        },
                        DataBuilders = new List<DataBuilder>
                        {
                            new DataBuilder { Name = "noreferencetransmission", BuilderName = "Client.Builder.CreatePK" }
                        }
                    },
                    new Table
                    {
                        Name = "propositionrachat",
                        ForeignKeys = new ForeignKeys
                        {
                            ForeignKeyAdd = new List<ForeignKeyAdd>
                            {
                                new ForeignKeyAdd
                                {
                                    DestinationVar = "ARIEL_TO",
                                    TableTo = "s_srv_req",
                                    Columns = new List<ForeignKeyColumn>
                                    {
                                        new ForeignKeyColumn { NameFrom = "noreferencetransmission", NameTo = "sr_num" }
                                    }
                                }
                            }
                        },
                        DataBuilders = new List<DataBuilder>
                        {
                            new DataBuilder { Name = "col1", BuilderName = "Client.Builder.Random" }
                        }
                    }
                }
            };
            var arielFrom = new DbSettings { Id = 2, Var = "ARIEL_FROM" };
            var siebelFrom = new DbSettings { Id = 3, Var = "SIEBEL_FROM" };

            project.Templates.AddRange(new List<DbSettings> { pgisDbo, arielFrom, siebelFrom });

            //Behaviors
            project.Behaviors.AddRange(new List<Behavior>
            {
                new Behavior
                {
                    Id = 1,
                    Name = "Default",
                    Description = "Configuration par défaut",
                    DbSettings = new List<DbSettings>
                    {
                        new DbSettings { Id = 1, BasedOn = 1 },
                        new DbSettings { Id = 2, BasedOn = 2 },
                        new DbSettings { Id = 3, BasedOn = 3 }
                    }
                },
                new Behavior
                {
                    Id = 2,
                    Name = "Client",
                    Description = "Duplication d'un client"
                }
            });

            //Maps
            project.Maps.AddRange(new List<MapFrom>
            {
                new MapFrom
                {
                    Name = "UNI",
                    UsableBehaviours = "1",
                    Variables = new List<Variable>
                    {
                        new Variable { Name = "PGIS_FROM", Server = 1, Database = "pgis", Schema = "dbo" },
                        new Variable { Name = "ARIEL_FROM", Server = 1, Database = "ariel", Schema = "dbo" },
                        new Variable { Name = "SIEBEL_FROM", Server = 1, Database = "siebel", Schema = "dbo" }
                    },
                    MapTos = new List<MapTo>
                    {
                        new MapTo
                        {
                            Name = "UNI",
                            Variables = new List<Variable>
                            {
                                new Variable { Name = "PGIS_TO", Server = 1, Database = "pgis", Schema = "dbo" },
                                new Variable { Name = "ARIEL_TO", Server = 1, Database = "ariel", Schema = "dbo" },
                                new Variable { Name = "SIEBEL_TO", Server = 1, Database = "siebel", Schema = "dbo" },
                            }
                        },
                        new MapTo
                        {
                            Name = "FON",
                            Variables = new List<Variable>
                            {
                                new Variable { Name = "PGIS_TO", Server = 2, Database = "pgis", Schema = "dbo" },
                                new Variable { Name = "ARIEL_TO", Server = 2, Database = "ariel", Schema = "dbo" },
                                new Variable { Name = "SIEBEL_TO", Server = 2, Database = "siebel", Schema = "dbo" },
                            }
                        },
                        new MapTo
                        {
                            Name = "PROD",
                            Variables = new List<Variable>
                            {
                                new Variable { Name = "PGIS_TO", Server = 3, Database = "pgis", Schema = "dbo" },
                                new Variable { Name = "ARIEL_TO", Server = 4, Database = "ariel", Schema = "dbo" },
                                new Variable { Name = "SIEBEL_TO", Server = 5, Database = "siebel", Schema = "dbo" },
                            }
                        }
                    },
                    Roads = new List<Road>
                    {
                        new Road { SourceVar = "PGIS_FROM", DestinationVar = "PGIS_TO" },
                        new Road { SourceVar = "ARIEL_FROM", DestinationVar = "PGIS_TO" },
                        new Road { SourceVar = "SIEBEL_FROM", DestinationVar = "PGIS_TO" }
                    }
                }
            });

            return project;
        }

        public static void TestSerializingConfiguration()
        {
            var project = CreateConfiguration();
            var xml = project.SerializeXml();
        }

        public static void TestBehaviorBuilder()
        {
            var project = CreateConfiguration();
            var behavior = project.BuildBehavior(1);
        }
    }
}
