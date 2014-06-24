using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using DataCloner.DataClasse;
using DataCloner.Interface;
using DataCloner.DataClasse.Cache;
using IQueryProvider = DataCloner.Interface.IQueryProvider;

namespace DataCloner.DataAccess
{
    public class QueryProviderSqlServer : IQueryProvider
    {
        private SqlConnection _conn;
        private readonly bool _isReadOnly;
        //private ITableCacheDictionnary _cache = new TableCacheDictionnary();

        public QueryProviderSqlServer(string connectionString)
        {
            _conn = new SqlConnection(connectionString);
        }

        public QueryProviderSqlServer(string connectionString, bool readOnly)
            : this(connectionString)
        {
            _isReadOnly = readOnly;
        }

        ~QueryProviderSqlServer()
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

        public DataTable GetForeignKeys(ITableIdentifier ti)
        {
            var sql = "SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS";

            throw new NotImplementedException();
        }

        public Int64 GetLastInsertedPk()
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
            var sql = string.Format(
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


        public string[] GetDatabasesName()
        {
            throw new NotImplementedException();
        }

        public void GetColumns(Action<IDataReader,Int16,string> reader,String database)
        {
            throw new NotImplementedException();
        }

        public void GetForeignKeys(Action<IDataReader, short, string> reader, string database)
        {
            throw new NotImplementedException();
        }
    }
}
