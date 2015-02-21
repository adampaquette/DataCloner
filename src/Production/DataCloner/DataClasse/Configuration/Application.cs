﻿using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class Application
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlArrayItem("add")]
        public List<Connection> ConnectionStrings { get; set; }
        public List<ClonerConfiguration> ClonerConfigurations { get; set; }
        public List<Map> Maps { get; set; }

        public Application()
        {
            ConnectionStrings = new List<Connection>();
            ClonerConfigurations = new List<ClonerConfiguration>();
            Maps = new List<Map>();
        }
    }
}