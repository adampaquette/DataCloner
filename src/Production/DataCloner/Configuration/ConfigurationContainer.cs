using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DataCloner.Configuration
{
    [Serializable]
    [XmlRoot("Configuration")]
    public class ConfigurationContainer
    {
        public const string ConfigFileName = "dc.config";

        public List<Application> Applications { get; set; }

        public ConfigurationContainer()
        {
            Applications = new List<Application>();
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

        public static ConfigurationContainer Load(string path)
        {
            var xs = new XmlSerializer(typeof(ConfigurationContainer));
            if (!File.Exists(path)) return null;
            var fs = new FileStream(path, FileMode.Open);
            var cReturn = (ConfigurationContainer)xs.Deserialize(fs);
            fs.Close();
            //cReturn.Validate();
            return cReturn;
        }

        //public void Validate()
        //{
        //    foreach (var cs in ConnectionStrings)
        //    {
        //        if (cs.Id == 0)
        //            throw new InvalidDataException("The connection string's Id cannot be 0. Index start at 1.");

        //        if (cs.SameConfigAsId > 0 && ConnectionStrings.Where(c => c.Id == cs.SameConfigAsId).FirstOrDefault() == null)
        //            throw new InvalidDataException(String.Format("The connection string's Id {0} cannot be found for the attribute SameConfigAsId. " +
        //                                                         "Zero represent nothing.", cs.SameConfigAsId));
        //    }
        //}
    }
}