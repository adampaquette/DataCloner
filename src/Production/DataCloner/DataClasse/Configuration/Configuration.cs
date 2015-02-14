using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    [XmlRoot("Configuration")]
    public class Configuration
    {
        public const string CONFIG_FILE_NAME = "dc.config";

        public List<Application> Applications { get; set; }

        public Configuration()
        {
            Applications = new List<Application>();
        }

        public void Save(string path)
        {
            var xs = new XmlSerializer(GetType());
            var fs = new FileStream(path, FileMode.Create);
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(fs, this, ns);
            fs.Close();
        }

        public static Configuration Load(string path)
        {
            var xs = new XmlSerializer(typeof(Configuration));
            if (!File.Exists(path)) return null;
            var fs = new FileStream(path, FileMode.Open);
            var cReturn = (Configuration)xs.Deserialize(fs);
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