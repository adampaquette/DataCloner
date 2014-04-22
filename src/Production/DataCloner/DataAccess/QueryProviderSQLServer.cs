using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using DataCloner.DataClasse;

namespace DataCloner.DataAccess
{
    public class QueryProviderSQLServer : IQueryProvider
    {
        private SqlConnection _conn;
        private bool _isReadOnly;
        //private ITableCacheDictionnary _cache = new TableCacheDictionnary();

        public QueryProviderSQLServer(string connectionString)
        {
            _conn = new SqlConnection(connectionString);
        }

        public QueryProviderSQLServer(string connectionString, bool readOnly)
            : this(connectionString)
        {
            _isReadOnly = readOnly;
        }

        ~QueryProviderSQLServer()
        {
            Dispose(false);
        }

        public IDbConnection Connection
        {
            get { return _conn; }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public DataTable GetFK(ITableIdentifier ti)
        {
            string sql = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS";

            throw new NotImplementedException();
        }

        public Int64 GetLastInsertedPK()
        {
            throw new NotImplementedException();
        }

        public DataTable Select(IRowIdentifier ri)
        {
            throw new NotImplementedException();
        }

        public void Insert(ITableIdentifier ti, System.Data.DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        public void Update(IRowIdentifier ri, System.Data.DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        public void Delete(IRowIdentifier ri)
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

        #region IQueryDatabase Membres

        /// <summary>
        /// Construit ou récupère la cache.
        /// </summary>
        public void Init()
        {
            string sql = string.Format(
               "SELECT " +
               "	TABLE_SCHEMA," +
               "	TABLE_NAME," +
               "	COLUMN_NAME " +
               "FROM {0}.INFORMATION_SCHEMA.COLUMNS " +
               "WHERE TABLE_CATALOG = @database " +
               "ORDER BY " +
               "	TABLE_SCHEMA," +
               "	TABLE_NAME", "pgisCBL");

            var dt = new DataTable();
            var cmd = new SqlCommand(sql, _conn);
            var reader = cmd.ExecuteReader();


            //_cache.Add(


            var sbTemp = new StringBuilder("");

            while (reader.Read())
            {
                var ti = new TableIdentifier();
                //var tc = new TableCache();

            }

            //new SqlDataAdapter(cmd).Fill(dt);
            //var nbRows = dt.Rows.Count;
            //for (int i = 0; i < nbRows; i++)
            //{ 

            //}
        }

        #endregion
    }
}
