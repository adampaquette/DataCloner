using System;
using GalaSoft.MvvmLight;

namespace DataCloner.GUI.ViewModel
{
    public class ConnectionStringViewModel : ViewModelBase
    {
        private Int16 _id;
        private string _name;
        private string _providerName;
        private string _connectionString;

        public Int16 Id
        {
            get { return _id; }
            set { Set("Id", ref _id, value); }
        }

        public ConnectionStringViewModel ()
        {
            Id = 12356;
        }
    }
}
