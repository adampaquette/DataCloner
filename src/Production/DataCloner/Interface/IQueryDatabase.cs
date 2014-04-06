using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Interface;
using System.Data;

namespace DataCloner.Interface
{
    public interface IQueryDatabase : IDbConnection
    {
        bool IsReadOnly { get; }
        DataTable GetFK();
        object GetLastInsertedPK();
        DataTable Select(IRowIdentifier ri);
        bool Insert(ITableIdentifier ti, DataRow[] rows);
        bool Update(IRowIdentifier ri, DataRow[] rows);
        bool Delete(IRowIdentifier ri);
    }
}
