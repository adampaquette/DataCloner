using DataCloner.Core.Internal;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Core.Plan
{
    public class ResultSet
    {
        public List<RowIdentifier> Results { get; }

        internal ResultSet(Dictionary<string, ExecutionPlan> exucutionPlanByServer)
        {
            Results = ParseClonedRows(exucutionPlanByServer);
        }   

        private static List<RowIdentifier> ParseClonedRows(Dictionary<string, ExecutionPlan> exucutionPlanByServer)
        {
            var clonedRows = new List<RowIdentifier>();

            foreach (var server in exucutionPlanByServer)
            {
                foreach (var row in server.Value.InsertSteps.Where(s=>s.Depth==0))
                {
                    var pkTemp = row.TableMetadata.BuildPkFromDataRow(row.Datarow);

                    //Clone for new reference
                    var clonedPk = new ColumnsWithValue();
                    foreach (var col in pkTemp)
                    {
                        var sqlVar = col.Value as SqlVariable;
                        clonedPk.Add(col.Key, sqlVar != null ? sqlVar.Value : col.Value);
                    }

                    var riReturn = new RowIdentifier
                    {
                        ServerId = row.DestinationTable.ServerId,
                        Database = row.DestinationTable.Database,
                        Schema = row.DestinationTable.Schema,
                        Table = row.DestinationTable.Table,
                        Columns = clonedPk
                    };

                    //Construit la pk de retour
                    clonedRows.Add(riReturn);
                }
            }
         
            return clonedRows;
        }
    }
}