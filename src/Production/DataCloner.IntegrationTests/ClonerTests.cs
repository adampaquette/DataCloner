using DataCloner.Core.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Xunit;

namespace DataCloner.Core.IntegrationTests
{
    public class ClonerTests
    {
        public static IEnumerable<object[]> DbEngineToTest => DatabaseInitializer.Connections;

        [Theory(Skip = "Generation of the cache files"), MemberData(nameof(DbEngineToTest))]
        public void Should_NotFail_When_Settuping(Connection conn)
        {
            var epb = new ExecutionPlanBuilder(Utils.MakeDefaultSettings(conn));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void CloningDependencies_With_DefaultConfig(Connection conn)
        {
            //Arrange
            var executionPlanBuilder = new ExecutionPlanBuilder(Utils.MakeDefaultSettings(conn));
            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Customer",
                Columns = new ColumnsWithValue
                {
                    { "CustomerId", 1 }
                }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, false).Compile();
            query.Execute();
            query.Commiting += (s, e) => e.Cancel = true;

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Customer",
                     Columns = new ColumnsWithValue { { "CustomerId", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Employee",
                     Columns = new ColumnsWithValue { { "EmployeeId", 3 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Employee",
                     Columns = new ColumnsWithValue { { "EmployeeId", 2 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Employee",
                     Columns = new ColumnsWithValue{ { "EmployeeId", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void CloningDerivatives_With_GlobalAccessDenied(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.Add(new TableModifier
            {
                Name = "Album",
                DerativeTables = new DerativeTable
                {
                    GlobalAccess = DerivativeTableAccess.Denied
                }
            });

            var executionPlanBuilder = new ExecutionPlanBuilder(config);
            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Artist",
                Columns = new ColumnsWithValue
                {
                    { "ArtistId", 1 }
                }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, true).Compile();
            query.Execute();
            query.Commiting += (s, e) => e.Cancel = true;

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Artist",
                     Columns = new ColumnsWithValue { { "ArtistId", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Album",
                     Columns = new ColumnsWithValue { { "AlbumId", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Album",
                     Columns = new ColumnsWithValue { { "AlbumId", 4 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void CloningDerivatives_With_GlobalAccessForced(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "Album",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Denied
                    }
                },
                new TableModifier
                {
                    Name = "Artist",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Forced
                    }
                }
            });
            var executionPlanBuilder = new ExecutionPlanBuilder(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Artist",
                Columns = new ColumnsWithValue { { "ArtistId", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, false).Compile();
            query.Execute();
            query.Commiting += (s, e) => e.Cancel = true;

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Artist",
                     Columns = new ColumnsWithValue { { "ArtistId", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Album",
                     Columns = new ColumnsWithValue { { "AlbumId", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Album",
                     Columns = new ColumnsWithValue { { "AlbumId", 4 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void CloningDerivatives_With_DerivativeSubTableAccessForced(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "Album",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Denied
                    }
                },
                new TableModifier
                {
                    Name = "Artist",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Forced,
                        DerativeSubTables = new List<DerivativeSubTable>
                        {
                            new DerivativeSubTable
                            {
                                ServerId = conn.Id.ToString(),
                                Database = Utils.TestDatabase(conn),
                                Schema = Utils.TestSchema(conn),
                                Table = "Album",
                                Access = DerivativeTableAccess.Forced
                            }
                        }
                    }
                }
            });

            var executionPlanBuilder = new ExecutionPlanBuilder(config);
            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Artist",
                Columns = new ColumnsWithValue { { "ArtistId", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, false).Compile();
            query.Execute();
            query.Commiting += (s, e) => e.Cancel = true;

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Artist",
                     Columns = new ColumnsWithValue { { "ArtistId", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Album",
                     Columns = new ColumnsWithValue { { "AlbumId", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Album",
                     Columns = new ColumnsWithValue { { "AlbumId", 4 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void CloningDerivatives_With_DerivativeSubTableAccessDenied(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "Artist",
                    DerativeTables = new DerativeTable
                    {
                        GlobalAccess = DerivativeTableAccess.Forced,
                        DerativeSubTables = new List<DerivativeSubTable>
                        {
                            new DerivativeSubTable
                            {
                                ServerId = conn.Id.ToString(),
                                Database = Utils.TestDatabase(conn),
                                Schema = Utils.TestSchema(conn),
                                Table = "Album",
                                Access = DerivativeTableAccess.Denied
                            }
                        }
                    }
                }
            });

            var executionPlanBuilder = new ExecutionPlanBuilder(config);
            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Artist",
                Columns = new ColumnsWithValue { { "ArtistId", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, true).Compile();
            query.Execute();
            query.Commiting += (s, e) => e.Cancel = true;

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Artist",
                     Columns = new ColumnsWithValue { { "ArtistId", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void Cloning_With_StaticTable(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "Artist",
                    IsStatic = true
                }
            });
            var executionPlanBuilder = new ExecutionPlanBuilder(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Album",
                Columns = new ColumnsWithValue { { "AlbumId", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, false).Compile();
            query.Commiting += (s, e) => e.Cancel = true;
            query.Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Album",
                     Columns = new ColumnsWithValue { { "AlbumId", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));          
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void Cloning_Should_NotCloneDerivativeOfDependancy(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var executionPlanBuilder = new ExecutionPlanBuilder(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "PlaylistTrack",
                Columns = new ColumnsWithValue
                {
                    { "PlaylistId", 1 },
                    { "TrackId", 1 }
                }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, false).Compile();
            query.Execute();
            query.Commiting += (s, e) => e.Cancel = true;

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = Utils.TestDatabase(conn),
                    Schema = Utils.TestSchema(conn),
                    Table = "PlaylistTrack",
                    Columns = new ColumnsWithValue
                    {
                        { "PlaylistId", 1 },
                        { "TrackId", 1 }
                    }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = Utils.TestDatabase(conn),
                    Schema = Utils.TestSchema(conn),
                    Table = "Playlist",
                    Columns = new ColumnsWithValue { { "PlaylistId", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = Utils.TestDatabase(conn),
                    Schema = Utils.TestSchema(conn),
                    Table = "Track",
                    Columns = new ColumnsWithValue { { "TrackId", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = Utils.TestDatabase(conn),
                    Schema = Utils.TestSchema(conn),
                    Table = "Album",
                    Columns = new ColumnsWithValue { { "AlbumId", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = Utils.TestDatabase(conn),
                    Schema = Utils.TestSchema(conn),
                    Table = "Artist",
                    Columns = new ColumnsWithValue { { "ArtistId", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = Utils.TestDatabase(conn),
                    Schema = Utils.TestSchema(conn),
                    Table = "Genre",
                    Columns = new ColumnsWithValue { { "GenreId", 1} }
                },
                new RowIdentifier
                {
                    ServerId = conn.Id,
                    Database = Utils.TestDatabase(conn),
                    Schema = Utils.TestSchema(conn),
                    Table = "MediaType",
                    Columns = new ColumnsWithValue { { "MediaTypeId", 1} }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void Cloning_With_ForeignKeyAdd(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "Artist",
                    ForeignKeys = new ForeignKeys
                    {
                        ForeignKeyAdd = new List<ForeignKeyAdd>
                        {
                            new ForeignKeyAdd
                            {
                                ServerId = conn.Id.ToString(),
                                Database = Utils.TestDatabase(conn),
                                Schema = Utils.TestSchema(conn),
                                Table = "Playlist",
                                Columns = new List<ForeignKeyColumn>
                                {
                                    new ForeignKeyColumn
                                    {
                                        NameFrom ="ArtistId",
                                        NameTo = "PlaylistId"
                                    }
                                }
                            }
                        }
                    }
                }
            });

            var executionPlanBuilder = new ExecutionPlanBuilder(config);
            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Artist",
                Columns = new ColumnsWithValue { { "ArtistId", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, false).Compile();
            query.Commiting += (s, e) => e.Cancel = true;
            query.Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Artist",
                     Columns = new ColumnsWithValue { { "ArtistId", 1 } }
                },
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Playlist",
                     Columns = new ColumnsWithValue { { "PlaylistId", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void Cloning_With_ForeignKeyRemove(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "Album",
                    ForeignKeys = new ForeignKeys
                    {
                        ForeignKeyRemove = new ForeignKeyRemove
                        {
                            Columns = new List<ForeignKeyRemoveColumn>
                            {
                                new ForeignKeyRemoveColumn
                                {
                                     Name = "ArtistId"
                                }
                            }
                        }
                    }
                }
            });

            var executionPlanBuilder = new ExecutionPlanBuilder(config);
            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Album",
                Columns = new ColumnsWithValue { { "AlbumId", 1 } }
            };

            var clonedData = new List<RowIdentifier>();
            executionPlanBuilder.StatusChanged += (s, e) =>
            {
                if (e.Status == Status.Cloning)
                    clonedData.Add(e.SourceRow);
            };

            //Act
            var query = executionPlanBuilder.Append(source, false).Compile();
            query.Commiting += (s, e) => e.Cancel = true;
            query.Execute();

            //Assert
            var expectedData = new List<RowIdentifier>
            {
                new RowIdentifier
                {
                     ServerId = conn.Id,
                     Database = Utils.TestDatabase(conn),
                     Schema = Utils.TestSchema(conn),
                     Table = "Album",
                     Columns = new ColumnsWithValue { { "AlbumId", 1 } }
                }
            };

            Assert.True(Enumerable.SequenceEqual(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        [Theory, MemberData(nameof(DbEngineToTest))]
        public void Cloning_With_DataBuilder(Connection conn)
        {
            //Arrange
            var config = Utils.MakeDefaultSettings(conn);
            var tablesConfig = config.GetDefaultSchema();
            tablesConfig.AddRange(new List<TableModifier>
            {
                new TableModifier
                {
                    Name = "Employee",
                    DataBuilders = new List<DataBuilder>
                    {
                        new DataBuilder{ Name = "FirstName",  BuilderName = "StringDataBuilder" },
                        new DataBuilder{ Name = "LastName",  BuilderName = "StringDataBuilder" },
                        new DataBuilder{ Name = "ReportsTo",  BuilderName = "AutoIncrementDataBuilder" }
                    }
                }
            });
            var executionPlanBuilder = new ExecutionPlanBuilder(config);

            var source = new RowIdentifier
            {
                ServerId = conn.Id,
                Database = Utils.TestDatabase(conn),
                Schema = Utils.TestSchema(conn),
                Table = "Employee",
                Columns = new ColumnsWithValue { { "EmployeeId", 1 } }
            };

            //Act
            var query = executionPlanBuilder.Append(source, false).Compile();

            IDbCommand command = null;
            query.Commiting += (s, e) =>
            {
                e.Cancel = true;
                command = e.Command;
            };
            query.Execute();

            //Assert
            var paramFirstName = command?.Parameters["@FirstName0"] as IDataParameter;
            Assert.Matches("(.+){20}", paramFirstName.Value.ToString());

            var paramLastName = command?.Parameters["@LastName0"] as IDataParameter;
            Assert.Matches("(.+){20}", paramLastName.Value.ToString());

            var paramReportsTo = command?.Parameters["@ReportsTo0"] as IDataParameter;
            Assert.True(paramReportsTo.Value.IsNumericType());
        }
    }
}
