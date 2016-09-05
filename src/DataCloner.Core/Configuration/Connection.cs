﻿using System;
using System.Diagnostics;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Id.ToString() + \"_\" + Name}")]
    [Serializable]
    public class Connection : IEquatable<Connection>
    {
        [XmlAttribute]
        public short Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string ProviderName { get; set; }
        [XmlAttribute]
        public string ConnectionString { get; set; }

        public Connection() { }
        public Connection(short id, string name, string providerName, string connectionString)
        {
            Id = id;
            Name = name;
            ProviderName = providerName;
            ConnectionString = connectionString;
        }

        public override string ToString()
        {
            return ProviderName + " " + Id.ToString();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Connection;
            return Equals(o);
        }

        public bool Equals(Connection other)
        {
            return other != null && other.Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}