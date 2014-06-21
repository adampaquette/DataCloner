using DataCloner.DataAccess;
using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.DataClasse.Configuration;
using DataCloner.Interface;

using System.IO;
using System.Text;
using System.Security.Cryptography;

using Murmur;

namespace DataCloner
{
    class DataCloner
    {        
        private QueryDispatcher _dispatcher;

        public DataCloner()
        {
        }

        public IRowIdentifier SqlTraveler(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFk)
        {
            //RowIdentifier riReturn = null;
            //_dispatcher = new QueryDispatcher(_cache);
            //var linesSource = _dispatcher.Select(riSource);

            return null;
        }


    }
}
