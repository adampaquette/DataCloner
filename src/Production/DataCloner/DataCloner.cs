using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Interface;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

using Murmur;

namespace DataCloner
{
    class DataCloner
    {        
        private QueryDispatcher _dispatcher;
        private CachedTables _cacheTable;

        public Dictionary<Tuple<Int16, String>, Tuple<Int16, String>> ServerMap {get;set;}

        public DataCloner()
        {
        }

        public void Initialize(string cacheName = Configuration.CacheName)
        {
            _dispatcher = new QueryDispatcher();
            _dispatcher.Initialize(cacheName);
            _cacheTable = _dispatcher.Cache.CachedTables;
        }

        public IRowIdentifier SqlTraveler(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFk)
        {
            RowIdentifier riReturn;
            object[][] sourceRows = _dispatcher.Select(riSource);
            int nbRows = sourceRows.Length;
            var table = _cacheTable.GetTable(riSource.ServerId, riSource.DatabaseName, riSource.SchemaName, riSource.TableName);
            var fks = table.ForeignKeys;

            if (shouldReturnFk && nbRows > 1)
                throw new Exception("The foreignkey is not unique!");

            //For each row
            for (int i = 0; i < nbRows; i++)
            {

            }



            return null;
        }


    }
}
