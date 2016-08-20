﻿using System;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    public class Variable : IEquatable<Variable>
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public Int16 Server { get; set; }
        [XmlAttribute]
        public string Database { get; set; }
        [XmlAttribute]
        public string Schema { get; set; }

        public override bool Equals(object obj)
        {
            var o = obj as Variable;
            return Equals(o);
        }

        public bool Equals(Variable other)
        {
            return other != null && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}