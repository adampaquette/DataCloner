using System;
using System.ComponentModel.DataAnnotations;
using DataCloner.DataClasse.Configuration;
using GalaSoft.MvvmLight.Ioc;

namespace DataCloner.GUI.ViewModel
{
    class ConnectionViewModel : AnnotationViewModelBase
    {
        private Int16 _id;
        private string _name;
        private string _providerName;
        private string _connectionString;

        [Required]
        public Int16 Id
        {
            get { return _id; }
            set { ValidateAndSet(ref _id, value); }
        }

        [Required]
        public string Name
        {
            get { return _name; }
            set { ValidateAndSet(ref _name, value); }
        }

        [Required]
        public string ProviderName
        {
            get { return _providerName; }
            set { ValidateAndSet(ref _providerName, value); }
        }

        [Required]
        public string ConnectionString
        {
            get { return _connectionString; }
            set { ValidateAndSet(ref _connectionString, value); }
        }

        [PreferredConstructor]
        public ConnectionViewModel()
        {
            if (IsInDesignMode)
            {
                Id = 1;
                Name = "UNI";
                ProviderName = "System.Data.SqlClient";
                ConnectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=northwind;";
            }
        }

        public ConnectionViewModel(Connection connection)
        {
            Id = connection.Id;
            Name = connection.Name;
            ProviderName = connection.ProviderName;
            ConnectionString = connection.ConnectionString;
        }
    }
}
