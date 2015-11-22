using DataCloner.Internal;
using DataCloner.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner
{
    public class Result
    {
        public List<RowIdentifier> Clones { get; private set; }

        internal Result(Dictionary<Int16, ExecutionPlan> exucutionPlanByServer)
        {
            Clones = ParseClonedRows(exucutionPlanByServer);
        }   

        private List<RowIdentifier> ParseClonedRows(Dictionary<Int16, ExecutionPlan> exucutionPlanByServer)
        {
            var clonedRows = new List<RowIdentifier>();

            foreach (var server in exucutionPlanByServer)
            {
                foreach (var row in server.Value.InsertSteps.Where(s=>s.Depth==0))
                {
                    var pkTemp = row.TableSchema.BuildPkFromDataRow(row.DataRow);

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
