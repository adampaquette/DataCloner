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
    public class Project
    {
        [XmlAttribute]
        public string ToolsVersion { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        [XmlArrayItem("Add")]        
        public List<Connection> ConnectionStrings { get; set; }

        [XmlArrayItem("Schema")]
        public List<SchemaVar> EnvironmentComposition { get; set; }

        public List<Environment> Environments { get; set; }

        public List<DbSettings> ExtractionTemplates { get; set; }

        [XmlArrayItem("Behavior")]        
        public List<Behavior> ExtractionBehaviors { get; set; }

        public Project()
        {
            ConnectionStrings = new List<Connection>();
            ExtractionTemplates = new List<DbSettings>();
            ExtractionBehaviors = new List<Behavior>();
            Environments = new List<Environment>();
        }

        public void Save(string path)
        {
            this.SaveXml(path);
        }

        public static async Task<Project> LoadAsync(string path)
        {
            return await SerializationHelper.LoadXmlAsync<Project>(path).ConfigureAwait(false);
        }
    }
}