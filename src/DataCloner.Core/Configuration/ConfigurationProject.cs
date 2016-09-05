using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            return await SerializationHelper.LoadXmlAsync<ConfigurationProject>(path).ConfigureAwait(false);
        }

        public HashSet<Variable> GetVariablesForMap(string nameFrom, string nameTo)
        {
            var mapFrom = Maps.FirstOrDefault(m => m.Name == nameFrom);
            if(mapFrom == null)
                throw new NullReferenceException($"The MapFrom node {nameFrom} was not found.");

            var mapTo = mapFrom.MapTos.FirstOrDefault(m => m.Name == nameTo);
            if (mapTo == null)
                throw new NullReferenceException($"The MapTo node {nameTo} was not found.");

            var variables = new HashSet<Variable>(Variables);

            //Override values in cascade
            foreach (var v in mapFrom.Variables)
            {
                if (variables.Contains(v))
                    variables.Remove(v);//Will it blend?
                variables.Add(v);
            }
            foreach (var v in mapTo.Variables)
            {
                if (variables.Contains(v))
                    variables.Remove(v);//Will it blend?
                variables.Add(v);
            }

            return variables;
        }
    }
}