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


                    var proj = Utils.MakeDefaultProject(conn);
                    proj.Save("test.dcp");
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

        public static Project CreateConfiguration()
        {
            var project = new Project { Name = "MainApp" };

            //ConnectionStrings
            project.ConnectionStrings.AddRange(new List<Connection>
            {
                new Connection("UNI", "MySql.Data.MySqlClient", "server=UNI;user Id=root; password=toor; database=master; pooling=true"),
                new Connection("FON", "System.Data.Sqlite", "server=FON;user Id=root; password=toor; database=master; pooling=true"),
                new Connection("PROD_PGIS", "System.Data.Oracle", "server=PROD_PGIS;user Id=root; password=toor; database=master; pooling=true"),
                new Connection("PROD_ARIEL", "System.Data.PosgreSql", "server=PROD_ARIEL;user Id=root; password=toor; database=master; pooling=true"),
                new Connection("PROD_SIEBEL", "System.Data.MsSql", "server=PROD_SIEBEL;user Id=root; password=toor; database=master; pooling=true")
            });

            //EnvironmentComposition
            project.EnvironmentComposition = new List<SchemaVar>
            {
                new SchemaVar { Id = "PGIS", Server = "UNI", Database = "pgis", Schema = "dbo" },
                new SchemaVar { Id = "ARIEL", Server = "UNI", Database = "ariel", Schema = "dbo" },
                new SchemaVar { Id = "SIEBEL", Server = "UNI", Database = "siebel", Schema = "dbo" },
            };

            //Extraction templates
            var pgisDbo = new DbSettings
            {
                Id = "DefaultPGIS",
                ForSchemaId = "PGIS",
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
                                 new DerivativeTable { DestinationSchema = "PGIS", Name = "demande" }
                            }
                        },
                        ForeignKeys = new ForeignKeys
                        {
                             ForeignKeyAdd = new List<ForeignKeyAdd>
                             {
                                 new ForeignKeyAdd
                                 {
                                     DestinationSchema = "SIEBEL",
                                     DestinationTable = "s_srv_req",
                                     Columns = new List<ForeignKeyColumn> { new ForeignKeyColumn { Source = "noreferencetransmission", Destination = "sr_num" } }
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
                                    DestinationSchema = "ARIEL",
                                    DestinationTable = "s_srv_req",
                                    Columns = new List<ForeignKeyColumn>
                                    {
                                        new ForeignKeyColumn { Source = "noreferencetransmission", Destination = "sr_num" }
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
            var arielFrom = new DbSettings { Id = "DefaultARIEL", ForSchemaId = "ARIEL" };
            var siebelFrom = new DbSettings { Id = "DefaultSIEBEL", ForSchemaId = "SIEBEL" };

            project.ExtractionTemplates.AddRange(new List<DbSettings> { pgisDbo, arielFrom, siebelFrom });

            //Extraction behaviors
            project.ExtractionBehaviors.AddRange(new List<Behavior>
            {
                new Behavior
                {
                    Id = "Default",
                    Description = "Configuration par défaut",
                    DbSettings = new List<DbSettings>
                    {
                        new DbSettings { Id = "1", BasedOn = "DefaultPGIS" },
                        new DbSettings { Id = "2", BasedOn = "DefaultARIEL" },
                        new DbSettings { Id = "3", BasedOn = "DefaultSIEBEL" }
                    }
                },
                new Behavior
                {
                    Id = "Client",
                    Description = "Duplication d'un client"
                }
            });

            //Environments
            project.Environments.AddRange(new List<Configuration.Environment>
            {
                new Configuration.Environment
                {

                    Name = "UNI",
                    Schemas = new List<SchemaVar>
                    {
                        new SchemaVar { Id = "PGIS", Server = "UNI", Database = "pgis", Schema = "dbo" },
                        new SchemaVar { Id = "ARIEL", Server = "UNI", Database = "ariel", Schema = "dbo" },
                        new SchemaVar { Id = "SIEBEL", Server = "UNI", Database = "siebel", Schema = "dbo" }
                    }
                },
                new Configuration.Environment
                {

                    Name = "FON",
                    Schemas = new List<SchemaVar>
                    {
                        new SchemaVar { Id = "PGIS", Server = "FON", Database = "pgis", Schema = "dbo" },
                        new SchemaVar { Id = "ARIEL", Server = "FON", Database = "ariel", Schema = "dbo" },
                        new SchemaVar { Id = "SIEBEL", Server = "FON", Database = "siebel", Schema = "dbo" }
                    }
                },
                new Configuration.Environment
                {

                    Name = "PROD",
                    Schemas = new List<SchemaVar>
                    {
                        new SchemaVar { Id = "PGIS", Server = "PROD_PGIS", Database = "pgis", Schema = "dbo" },
                        new SchemaVar { Id = "ARIEL", Server = "PROD_ARIEL", Database = "ariel", Schema = "dbo" },
                        new SchemaVar { Id = "SIEBEL", Server = "PROD_SIEBEL", Database = "siebel", Schema = "dbo" }
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
            var behavior = project.BuildBehavior("Default");
        }
    }
}
