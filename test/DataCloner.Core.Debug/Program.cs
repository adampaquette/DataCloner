using DataCloner.Core.Configuration;
using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;

namespace DataCloner.Core.Debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                TestConfiguration();
            }
            catch (System.Exception)
            {

                throw;
            }

            Console.WriteLine("All tests passed!");
            Console.ReadKey();
        }

        public static void TestConfiguration()
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
            var PGIS_DBO = new DbSettings
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
                        DerativeTables = new DerivativeTableGlobal()
                        {
                            GlobalAccess = DerivativeTableAccess.Forced,
                            GlobalCascade = NullableBool.True,
                            DerivativeSubTables = new List<DerivativeTable>
                            {
                                 new DerivativeTable { Destination = "PGIS_TO", Name = "demande" }
                            }
                        },
                        ForeignKeys = new ForeignKeys
                        {
                             ForeignKeyAdd = new List<ForeignKeyAdd>
                             {
                                 new ForeignKeyAdd
                                 {
                                     Destination = "SIEBEL_TO",
                                     Table = "s_srv_req",
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
                                    Destination = "ARIEL_TO",
                                    Table = "s_srv_req",
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
            var ARIEL_FROM = new DbSettings { Id = 2, Var = "ARIEL_FROM" };
            var SIEBEL_FROM = new DbSettings { Id = 3, Var = "SIEBEL_FROM" };

            project.Templates.AddRange(new List<DbSettings> { PGIS_DBO, ARIEL_FROM, SIEBEL_FROM });

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
                        new Road { Source = "PGIS_FROM", Destination = "PGIS_TO" },
                        new Road { Source = "ARIEL_FROM", Destination = "PGIS_TO" },
                        new Road { Source = "SIEBEL_FROM", Destination = "PGIS_TO" }
                    }
                }
            });

            var xml = project.SerializeXml();
        }
    }
}
