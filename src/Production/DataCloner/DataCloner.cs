using DataCloner.DataClasse;
using DataCloner.Interface;
using DataCloner.Serialization;
using DataCloner.DataAccess;

namespace DataCloner
{
    class DataCloner
    {
        private readonly ConfigurationXml _config; //Pas de singleton pour la performance

        public DataCloner()
        {
            _config = ConfigurationXml.Load();
        }

        public IRowIdentifier SqlTraveler(IRowIdentifier riSource, bool getDerivatives, bool shouldReturnFk)
        {
            RowIdentifier riReturn = null;
            var dispatcher = new QueryDispatcher(_config);
            var linesSource = dispatcher.Select(riSource);




            return riReturn;
        }
    }
}
