using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace DataCloner.DataAccess
{
    class QueryDispatcher : IQueryDispatcher
    {
        private List<IQueryDatabase> _conns;

        public IDbConnection Connection
        {
            get { throw new NotImplementedException(); }
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public DataTable GetFK(ITableIdentifier ti)
        {
            throw new NotImplementedException();
        }

        public long GetLastInsertedPK()
        {
            throw new NotImplementedException();
        }

        public DataTable Select(IRowIdentifier ri)
        {
            throw new NotImplementedException();
        }

        public void Insert(ITableIdentifier ti, DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        public void Update(IRowIdentifier ri, DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        public void Delete(IRowIdentifier ri)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
