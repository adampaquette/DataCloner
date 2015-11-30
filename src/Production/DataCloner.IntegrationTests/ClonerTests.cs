using DataCloner.Archive;
using DataCloner.Configuration;
using DataCloner.Data;
using DataCloner.Internal;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace DataCloner.IntegrationTests
{
    public class ClonerTests
    {
        private const string DbEngine = "DbEngineToTest";
        private const string TestDatabase = "chinook";
        private const string TestSchema = "dbo";
        public static IEnumerable<object[]> DbEngineToTest => DatabaseInitializer.Connections;

        [Theory(Skip = "Generation of the cache files"), MemberData("DbEngine")]
        public void Should_NotFail_When_Settuping(SqlConnection conn)
        {
            var cloner = new Cloner(Utils.MakeDefaultSettings(conn));
        }

        [Theory, MemberData(DbEngine)]
        public void CloningDependencies_With_DefaultConfig(SqlConnection conn)
        {
            //Arrange
            var cloner = new Cloner(Utils.MakeDefaultSettings(conn));

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "customer",
                Columns = new ColumnsWithValue
                {
                    { "customerid", 1 }
                }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, false).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "customer",
                     Columns = new ColumnsWithValue { { "customerid", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "employee",
                     Columns = new ColumnsWithValue { { "employeeid", 3 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "employee",
                     Columns = new ColumnsWithValue { { "employeeid", 2 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "employee",
                     Columns = new ColumnsWithValue{ { "employeeid", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));

            var archive = cloner.ToDataArchive();
            archive.Description = "testing";
            archive.Save("archiveTest.dca");

            var archive2 = DataArchive.Load("archiveTest.dca");
        }

        [Theory, MemberData(DbEngine)]
        public void CloningDerivatives_With_GlobalAccessDenied(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.Add(new TableModifier
            {
                Name = "album",
                DerativeTables = new DerativeTable
                {
                    GlobalAccess = DerivativeTableAccess.Denied
                }
            });
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "artist",
                Columns = new ColumnsWithValue
                {
                    { "artistid", 1 }
                }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, true).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "artist",
                     Columns = new ColumnsWithValue { { "artistid", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "album",
                     Columns = new ColumnsWithValue { { "albumid", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "album",
                     Columns = new ColumnsWithValue { { "albumid", 4 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));
        }

        [Theory, MemberData(DbEngine)]
        public void CloningDerivatives_With_GlobalAccessForced(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "album",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Denied
                    }
                },
                new TableModifier
                {
                    Name = "artist",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Forced
                    }
                }
            });
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "artist",
                Columns = new ColumnsWithValue { { "artistid", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, false).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "artist",
                     Columns = new ColumnsWithValue { { "artistid", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "album",
                     Columns = new ColumnsWithValue { { "albumid", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "album",
                     Columns = new ColumnsWithValue { { "albumid", 4 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));
        }

        [Theory, MemberData(DbEngine)]
        public void CloningDerivatives_With_DerivativeSubTableAccessForced(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "album",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Denied
                    }
                },
                new TableModifier
                {
                    Name = "artist",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Forced,
                        DerativeSubTables = new List<DerivativeSubTable>
                        {
                            new DerivativeSubTable
                            {
                                ServerId = conn.Id.ToString(),
                                Database = TestDatabase,
                                Schema = TestSchema,
                                Table = "album",
                                Access = DerivativeTableAccess.Forced
                            }
                        }
                    }
                }
            });
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "artist",
                Columns = new ColumnsWithValue { { "artistid", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, false).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "artist",
                     Columns = new ColumnsWithValue { { "artistid", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "album",
                     Columns = new ColumnsWithValue { { "albumid", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "album",
                     Columns = new ColumnsWithValue { { "albumid", 4 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));
        }

        [Theory, MemberData(DbEngine)]
        public void CloningDerivatives_With_DerivativeSubTableAccessDenied(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "artist",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Forced,
                        DerativeSubTables = new List<DerivativeSubTable>
                        {
                            new DerivativeSubTable
                            {
                                ServerId = conn.Id.ToString(),
                                Database = TestDatabase,
                                Schema = TestSchema,
                                Table = "album",
                                Access = DerivativeTableAccess.Denied
                            }
                        }
                    }
                }
            });
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "artist",
                Columns = new ColumnsWithValue { { "artistid", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, true).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "artist",
                     Columns = new ColumnsWithValue { { "artistid", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));
        }

        [Theory, MemberData(DbEngine)]
        public void Cloning_With_StaticTable(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "artist",
                    IsStatic = true
                }
            });
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "album",
                Columns = new ColumnsWithValue { { "albumid", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, false).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "album",
                     Columns = new ColumnsWithValue { { "albumid", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));

            var da = cloner.ToDataArchive();
            da.Save("testingArchive.dca");
            var loaded = DataArchive.Load("testingArchive.dca");



            var bf = new BinaryFormatter();
            var a = "asads";
            var aa = "-----";

            var ms = new MemoryStream();
            bf.Serialize(ms, a);
            bf.Serialize(ms, aa);
            ms.Position = 0;
            var b = bf.Deserialize(ms);

            var equal = a == b;
        }

        [Theory, MemberData(DbEngine)]
        public void Cloning_Should_NotCloneDerivativeOfDependancy(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "playlisttrack",
                Columns = new ColumnsWithValue
                {
                    { "playlistid", 1 },
                    { "trackid", 1 }
                }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, false).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = TestDatabase,
                    Schema = TestSchema,
                    Table = "playlisttrack",
                    Columns = new ColumnsWithValue
                    {
                        { "playlistid", 1 },
                        { "trackid", 1 }
                    }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = TestDatabase,
                    Schema = TestSchema,
                    Table = "playlist",
                    Columns = new ColumnsWithValue { { "playlistid", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = TestDatabase,
                    Schema = TestSchema,
                    Table = "track",
                    Columns = new ColumnsWithValue { { "trackid", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = TestDatabase,
                    Schema = TestSchema,
                    Table = "album",
                    Columns = new ColumnsWithValue { { "albumid", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = TestDatabase,
                    Schema = TestSchema,
                    Table = "artist",
                    Columns = new ColumnsWithValue { { "artistid", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = TestDatabase,
                    Schema = TestSchema,
                    Table = "genre",
                    Columns = new ColumnsWithValue { { "genreid", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = TestDatabase,
                    Schema = TestSchema,
                    Table = "mediatype",
                    Columns = new ColumnsWithValue { { "mediatypeid", 1} }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));
        }

        [Theory, MemberData(DbEngine)]
        public void Cloning_With_ForeignKeyAdd(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "artist",
                    ForeignKeys = new ForeignKeys
                    {
                        ForeignKeyAdd = new List<ForeignKeyAdd>
                        {
                            new ForeignKeyAdd
                            {
                                ServerId = conn.Id.ToString(),
                                Database = TestDatabase,
                                Schema = TestSchema,
                                Table = "playlist",
                                Columns = new List<ForeignKeyColumn>
                                {
                                    new ForeignKeyColumn
                                    {
                                        NameFrom ="artistid",
                                        NameTo = "playlistid"
                                    }
                                }
                            }
                        }
                    }
                }
            });
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "artist",
                Columns = new ColumnsWithValue { { "artistid", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, false).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "artist",
                     Columns = new ColumnsWithValue { { "artistid", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "playlist",
                     Columns = new ColumnsWithValue { { "playlistid", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));
        }

        [Theory, MemberData(DbEngine)]
        public void Cloning_With_ForeignKeyRemove(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "album",
                    ForeignKeys = new ForeignKeys
                    {
                        ForeignKeyRemove = new ForeignKeyRemove
                        {
                            Columns = new List<ForeignKeyRemoveColumn>
                            {
                                new ForeignKeyRemoveColumn
                                {
                                     Name = "artistid"
                                }
                            }
                        }
                    }
                }
            });
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "album",
                Columns = new ColumnsWithValue { { "albumid", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            cloner.QueryCommiting += (s, e) => e.Cancel = true;
            cloner.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            cloner.Append(source, false).Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = TestDatabase,
                     Schema = TestSchema,
                     Table = "album",
                     Columns = new ColumnsWithValue { { "albumid", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData));
        }

        [Theory, MemberData(DbEngine)]
        public void Cloning_With_DataBuilder(SqlConnection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "employee",
                    DataBuilders = new List<DataBuilder>
                    {
                        new DataBuilder{ Name = "firstname",  BuilderName = "StringDataBuilder" },
                        new DataBuilder{ Name = "lastname",  BuilderName = "StringDataBuilder" },
                        new DataBuilder{ Name = "reportsto",  BuilderName = "AutoIncrementDataBuilder" }                        
                    }
                }
            });
            var cloner = new Cloner(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = TestDatabase,
                Schema = TestSchema,
                Table = "employee",
                Columns = new ColumnsWithValue { { "employeeid", 1 } }
            };

            IDbCommand command = null;
            cloner.QueryCommiting += (s, e) =>
            {
                e.Cancel = true;
                command = e.Command;
            };

            //Act
            cloner.Append(source, false).Execute();

            //Assert
            var paramFirstName = command?.Parameters["@firstname0"] as IDataParameter;
            Assert.Matches("(.+){20}", paramFirstName.Value.ToString());

            var paramLastName = command?.Parameters["@lastname0"] as IDataParameter;
            Assert.Matches("(.+){20}", paramLastName.Value.ToString());

            var paramReportsTo = command?.Parameters["@reportsto0"] as IDataParameter;
            Assert.IsType<int>(paramReportsTo.Value);
        }
    }
}
