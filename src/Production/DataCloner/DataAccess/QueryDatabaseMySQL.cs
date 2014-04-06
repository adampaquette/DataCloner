using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace DataCloner.DataAccess
{
    public class QueryDatabaseMySQL : IQueryDatabase
    {
        private MySqlConnection _conn;
        private bool _isReadOnly;

        public QueryDatabaseMySQL(string connectionString)
        {
            _conn = new MySqlConnection(connectionString);
            _conn.Open();
        }

        public QueryDatabaseMySQL(string connectionString, bool readOnly)
            : this(connectionString)
        {
            _isReadOnly = readOnly;
        }

        ~QueryDatabaseMySQL()
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
            DataTable dtReturn = new DataTable();
            
            //TODO SELECT DATABASE
            string SQL =
                "SELECT " +
                "	TC.TABLE_SCHEMA," +
                "	TC.TABLE_NAME," +
                "   k.COLUMN_NAME," +
                "   K.REFERENCED_TABLE_SCHEMA," +
                "	K.REFERENCED_TABLE_NAME," +
                "	K.REFERENCED_COLUMN_NAME " +
                "FROM information_schema.TABLE_CONSTRAINTS TC " +
                "LEFT JOIN information_schema.KEY_COLUMN_USAGE K ON TC.CONSTRAINT_NAME = K.CONSTRAINT_NAME " +
                "WHERE TC.CONSTRAINT_TYPE = 'FOREIGN KEY' " +
                "AND TC.TABLE_SCHEMA = @shema " +
                "AND TC.TABLE_NAME = @table";

            MySqlCommand cmd = new MySqlCommand(SQL, _conn);
            cmd.Parameters.AddWithValue("@shema", ti.SchemaName);
            cmd.Parameters.AddWithValue("@table", ti.TableName);

            new MySqlDataAdapter(cmd).Fill(dtReturn);

            return dtReturn;
        }

        public object GetLastInsertedPK()
        {
            throw new NotImplementedException();
        }

        public DataTable Select(IRowIdentifier ri)
        {
            throw new NotImplementedException();
        }

        public bool Insert(ITableIdentifier ti, System.Data.DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        public bool Update(IRowIdentifier ri, System.Data.DataRow[] rows)
        {
            throw new NotImplementedException();
        }

        public bool Delete(IRowIdentifier ri)
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
            if(disposing)
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

        //public bool OpenConnection()
        //{
        //    try
        //    {
        //        _conn.Open();
        //        return true;
        //    }
        //    catch (MySqlException ex)
        //    {
        //        switch (ex.Number)
        //        {
        //            case 0:
        //                throw new MySqlException("Cannot connect to server", ex.Number);
        //                break;

        //            case 1042:
        //                MessageBox.Show("Unable to connect to any of the specified MySQL hosts");
        //                break;

        //            case 1045:
        //                MessageBox.Show("Invalid username/password");
        //                break;
        //        }
        //        return false;
        //    }
        //}


    }
}
