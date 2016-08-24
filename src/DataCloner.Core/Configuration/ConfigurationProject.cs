using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DataCloner.Core.Configuration
{
    [DebuggerDisplay("{Name}")]
    [Serializable]
    [XmlRoot("Project")]
    public class ConfigurationProject
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

        [XmlArrayItem("Behavior")]        
        public List<Behavior> Behaviors { get; set; }

        public List<MapFrom> Maps { get; set; }

        public ConfigurationProject()
        {
            ConnectionStrings = new List<Connection>();
            Templates = new List<DbSettings>();
            Behaviors = new List<Behavior>();
            Maps = new List<MapFrom>();
        }

        public void Save(string path)
        {
            this.SaveXml(path);
        }

        public static async Task<ConfigurationProject> LoadAsync(string path)
        {
            return await Extensions.LoadXmlAsync<ConfigurationProject>(path).ConfigureAwait(false);
        }
    }
}