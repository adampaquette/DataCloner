using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataCloner.Core.Configuration;
using DataCloner.Core.Metadata;
using Xunit;

namespace DataCloner.Core.Debug
{
    public class ExecutionPlanBuilderTest
    {
        public void CloningDependencies_With_DefaultConfig(Connection conn)
        {
            //Arrange
            var executionPlanBuilder = new ExecutionPlanBuilder(Utils.MakeDefaultProject(conn), Utils.MakeDefaultContext());

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

            Assert.True(Utils.ScrambledEquals(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }

        public void CloningDerivatives_With_GlobalAccessDenied(Connection conn)
        {
            //Arrange
            var project = Utils.MakeDefaultProject(conn);

            project.Templates[0].Tables.Add(new Table
            {
                Name = "Album",
                DerativeTableGlobal = new DerivativeTableGlobal
                {
                    GlobalAccess = DerivativeTableAccess.Denied
                }
            });

            var executionPlanBuilder = new ExecutionPlanBuilder(project, Utils.MakeDefaultContext());
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

            Assert.True(Utils.ScrambledEquals(clonedData, expectedData, RowIdentifierComparer.OrdinalIgnoreCase));
        }
    }
}
