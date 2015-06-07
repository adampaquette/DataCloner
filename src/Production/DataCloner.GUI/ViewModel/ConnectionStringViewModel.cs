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

        public string Name
        {
            get { return _name; }
            set { Set("Name", ref _name, value); }
        }

        public string ProviderName
        {
            get { return _providerName; }
            set { Set("ProviderName", ref _providerName, value); }
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set { Set("ConnectionString", ref _connectionString, value); }
        }

        public ConnectionStringViewModel()
        {
            if (IsInDesignMode)
            {
                Id = 1;
                Name = "UNI";
                ProviderName = "System.Data.SqlClient";
                ConnectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=northwind;";
            }
        }
    }
}
