using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using DataCloner.Core.Framework;
using System.Threading.Tasks;

namespace DataCloner.Core.Configuration
{
    [Serializable]
    [XmlRoot("Project")]
    public class ProjectContainer
    {
        [XmlAttribute]
        public string ToolsVersion { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlArrayItem("Add")]        
        public List<Connection> ConnectionStrings { get; set; }
        public Modifiers Templates { get; set; }
        [XmlArrayItem("Behaviour")]        
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

        public static async Task<ProjectContainer> LoadAsync(string path)
        {
            return await Extensions.LoadXmlAsync<ProjectContainer>(path).ConfigureAwait(false);
        }
    }
}