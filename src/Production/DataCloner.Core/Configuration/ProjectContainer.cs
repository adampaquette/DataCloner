using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using DataCloner.Core.Framework;

namespace DataCloner.Core.Configuration
{
    [DataContract]
    //TODO :[XmlRoot("Project")]
    public class ProjectContainer
    {
        [DataMember]
        public string ToolsVersion { get; set; }
        [DataMember]
        public string Name { get; set; }
        //TODO :[XmlArrayItem("Add")]
        public List<Connection> ConnectionStrings { get; set; }
        public Modifiers Templates { get; set; }
        //TODO :[XmlArrayItem("Behaviour")]
        public List<Behaviour> Behaviours { get; set; }
        public List<Map> Maps { get; set; }

        public ProjectContainer()
        {
            ConnectionStrings = new List<Connection>();
            Templates = new Modifiers();
            Behaviours = new List<Behaviour>();
            Maps = new List<Map>();
        }

        public void Save(string path)
        {
            this.SaveXml(path);
        }

        public static ProjectContainer Load(string path)
        {
            return Extensions.LoadXml<ProjectContainer>(path);
        }
    }
}