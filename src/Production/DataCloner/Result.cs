using DataCloner.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloner
{
    public class Result
    {
        public List<RowIdentifier> Clones { get; private set; }

        internal Result(List<RowIdentifier> sources, IQueryDispatcher dispatcher)
        {
            Clones = new List<RowIdentifier>();

            foreach (var r in sources)
                GetClonedRows(r, dispatcher);
        }   

        private List<RowIdentifier> GetClonedRows(RowIdentifier riSource, IQueryDispatcher dispatcher)
        {
            var clonedRows = new List<RowIdentifier>();
            var srcRows = dispatcher.Select(riSource);
            if (srcRows.Length <= 0) return clonedRows;
            var table = metadata.GetTable(riSource);

            //By default the destination server is the source if no road is found.
            var serverDst = new ServerIdentifier
            {
                ServerId = riSource.ServerId,
                Database = riSource.Database,
                Schema = riSource.Schema
            };

            if (MetadataCtn.ServerMap.ContainsKey(serverDst))
                serverDst = MetadataCtn.ServerMap[serverDst];

            foreach (var row in srcRows)
            {
                var srcKey = table.BuildRawPkFromDataRow(row);
                var dstKey = _keyRelationships.GetKey(serverDst.ServerId, serverDst.Database,
                                                      serverDst.Schema, riSource.Table, srcKey);
                if (dstKey == null) continue;
                var pkTemp = table.BuildPkFromRawKey(dstKey);

                //Clone for new reference
                var clonedPk = new ColumnsWithValue();
                foreach (var col in pkTemp)
                {
                    var sqlVar = col.Value as SqlVariable;
                    clonedPk.Add(col.Key, sqlVar != null ? sqlVar.Value : col.Value);
                }

                var riReturn = new RowIdentifier
                {
                    ServerId = serverDst.ServerId,
                    Database = serverDst.Database,
                    Schema = serverDst.Schema,
                    Table = riSource.Table,
                    Columns = clonedPk
                };

                //Construit la pk de retour
                clonedRows.Add(riReturn);
            }
            return clonedRows;
        }
    }
}
