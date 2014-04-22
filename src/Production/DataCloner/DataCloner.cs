using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataCloner.DataClasse;
using DataCloner.Serialization;
using DataCloner.DataAccess;

namespace DataCloner
{
    class DataCloner
    {
        private readonly Configuration _config; //Pas de singleton pour la performance

        public DataCloner()
        {
            _config = Configuration.Load();
        }

        public IRowIdentifier SQLTraveler(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFK)
        {
            RowIdentifier riReturn = null;
            var dispatcher = new QueryDispatcher(_config);
            var linesSource = dispatcher.Select(riSource);




            return riReturn;
        }
    }
}
