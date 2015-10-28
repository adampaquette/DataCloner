using System;
using System.ComponentModel.DataAnnotations;
using DataCloner.Configuration;
using GalaSoft.MvvmLight.Ioc;
using DataCloner.GUI.Framework;

namespace DataCloner.GUI.ViewModel
{
    class ConnectionViewModel : ValidatableModel
    {
        internal Int16 _id;
        internal string _name;
        internal string _providerName;
        internal string _connectionString;

        [Required]
        public Int16 Id
        {
            get { return _id; }
            set { SetPropertyAndValidate(ref _id, value); }
        }

        [Required]
        public string Name
        {
            get { return _name; }
            set { SetPropertyAndValidate(ref _name, value); }
        }

        [Required]
        public string ProviderName
        {
            get { return _providerName; }
            set { SetPropertyAndValidate(ref _providerName, value); }
        }

        [Required]
        public string ConnectionString
        {
            get { return _connectionString; }
            set { SetPropertyAndValidate(ref _connectionString, value); }
        }

        [PreferredConstructor]
        public ConnectionViewModel()
        {
            //if (IsInDesignMode)
            //{
            //    Id = 1;
            //    Name = "UNI";
            //    ProviderName = "System.Data.SqlClient";
            //    ConnectionString = @"Data Source=.\SQLEXPRESS;Integrated Security=True;Database=northwind;";
            //}
        }
    }
}
