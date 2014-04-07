using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace DataCloner.DataAccess
{
    class QueryDatabaseSQLServer : IQueryDatabase
    {
        private IDbConnection _conn ;
        private bool _isReadOnly;

        public QueryDatabaseSQLServer(string connectionString)
        {
            _conn = new SqlConnection(connectionString);
        }

        public QueryDatabaseSQLServer(string connectionString, bool readOnly) : this(connectionString)
        {
            _isReadOnly = readOnly;
        }

        ~QueryDatabaseSQLServer()
        {
            Dispose(false);
        }

        IDbConnection IQueryDatabase.Connection
        {
            get { return _conn; }
        }

        bool IQueryDatabase.IsReadOnly
        {
            get { return _isReadOnly; }
        }

        DataTable IQueryDatabase.GetFK(ITableIdentifier ti)
        {
            string sql = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS";

            throw new NotImplementedException();
        }

        Int64 IQueryDatabase.GetLastInsertedPK()
        {
            throw new NotImplementedException();
        }

        DataTable IQueryDatabase.Select(IRowIdentifier ri)
        {
            throw new NotImplementedException();
        }

        void IQueryDatabase.Insert(ITableIdentifier ti, System.Data.DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        void IQueryDatabase.Update(IRowIdentifier ri, System.Data.DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        void IQueryDatabase.Delete(IRowIdentifier ri)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_conn != null)
                {
                    if (_conn.State != ConnectionState.Closed)
                        _conn.Close();
                    _conn.Dispose();
                    _conn = null;
                }
            }
        }
    }
}
