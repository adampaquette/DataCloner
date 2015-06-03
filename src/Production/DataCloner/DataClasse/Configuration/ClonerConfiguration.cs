using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using DataCloner.Framework;

namespace DataCloner.DataClasse.Configuration
{
    [Serializable]
    public class ClonerConfiguration
    {
        [XmlAttribute]
        public Int16 Id { get; set; }
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public string Description { get; set; }
        [XmlElement("ServerModifier")]
        public List<ServerModifier> Servers { get; set; }

        public ClonerConfiguration()
        {
            Servers = new List<ServerModifier>();
        }

        public void Build(ModifiersTemplates templates, List<Variable> variables)
        {
            var compiledConfig = new Dictionary<string, ServerModifier>();

            foreach (var server in Servers)
                MergeServer(templates, variables, compiledConfig, server);
        }

        private void MergeServer(ModifiersTemplates templates, 
                                 List<Variable> variables, 
                                 Dictionary<string, ServerModifier> compiledConfig,
                                 ServerModifier server)
        {
            //Remplacement de la variable
            if (server.Id.IsVariable())
                server.Id = variables.First(v => v.Name == server.Id).Value;

            if (!compiledConfig.CountainKey(server.Id))
                compiledConfig.Add(server.Id, server);
            
            //Template
            if (server.UseTemplateId > 0)
            {
                var srvTemplate = Servers.FirstOrDefault(s => s.TemplateId == server.UseTemplateId);
                if (srvTemplate == null)
                    srvTemplate = templates.ServerModifiers.First(s => s.TemplateId == server.UseTemplateId);

                MergeServer(templates, variables, compiledConfig, srvTemplate);
            }
            
            //Merge
            var srv = compiledConfig[server.Id];
            foreach (var database in server.Databases)
            {
                if (database.Name.IsVariable())
                    database.Name = variables.First(v => v.Name == database.Name).Value;
                
                var db = srv.Databases.FirstOrDefault(d => d.Name == database.Name);
                if (db == null)    
                    srv.Databases.Add(db);
             
            }
            
            
        }
    }
}