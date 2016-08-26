using System.Collections.Generic;
using System.Runtime.Serialization;
using DataCloner.Core.Framework;
using System.Threading.Tasks;

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

        public static async Task<ProjectContainer> LoadAsync(string path)
        {
            return await Extensions.LoadXmlAsync<ProjectContainer>(path).ConfigureAwait(false);
        }
    }
}