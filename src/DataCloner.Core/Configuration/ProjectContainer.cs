using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

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

        [XmlArrayItem("Var")]
        public List<Variable> Variables { get; set; }

        public List<DbSettings> Templates { get; set; }

        [XmlArrayItem("Behaviour")]        
        public List<Behaviour> Behaviours { get; set; }

        public List<MapFrom> Maps { get; set; }

        public ProjectContainer()
        {
            ConnectionStrings = new List<Connection>();
            Templates = new List<DbSettings>();
            Behaviours = new List<Behaviour>();
            Maps = new List<MapFrom>();
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