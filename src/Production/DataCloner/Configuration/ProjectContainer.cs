using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DataCloner.Configuration
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
            var fs = new FileStream(path, FileMode.Create);
            var ser = new XmlSerializer(GetType());
            var tw = new XmlTextWriter(fs, System.Text.Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                Indentation = 4
            };

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            
            ser.Serialize(tw, this, ns);

            fs.Close();
        }

        public static ProjectContainer Load(string path)
        {
            var xs = new XmlSerializer(typeof(ProjectContainer));
            if (!File.Exists(path)) return null;
            var fs = new FileStream(path, FileMode.Open);
            var cReturn = (ProjectContainer)xs.Deserialize(fs);
            fs.Close();
            //cReturn.Validate();
            return cReturn;
        }       
    }
}