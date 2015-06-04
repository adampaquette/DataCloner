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

            Servers = compiledConfig.Values.ToList();
        }

        private void MergeServer(ModifiersTemplates templates,
                                 List<Variable> variables,
                                 Dictionary<string, ServerModifier> compiledConfig,
                                 ServerModifier srvToMerge)
        {
            //Remplacement de la variable
            if (srvToMerge.Id.IsVariable())
                srvToMerge.Id = variables.First(v => v.Name == srvToMerge.Id).Value;

            if (!compiledConfig.ContainsKey(srvToMerge.Id))
                compiledConfig.Add(srvToMerge.Id, srvToMerge);

            //Template
            if (srvToMerge.UseTemplateId > 0)
            {
                var srvTemplate = Servers.FirstOrDefault(s => s.TemplateId == srvToMerge.UseTemplateId);
                if (srvTemplate == null)
                    srvTemplate = templates.ServerModifiers.First(s => s.TemplateId == srvToMerge.UseTemplateId);

                MergeServer(templates, variables, compiledConfig, srvTemplate);
                srvToMerge.UseTemplateId = 0;
            }

            //Merge
            foreach (var database in srvToMerge.Databases)
                MergeDatabase(templates, variables, srvToMerge, database);
        }

        private void MergeDatabase(ModifiersTemplates templates,
                                   List<Variable> variables,
                                   ServerModifier compiledServer,
                                   DatabaseModifier dbToMerge)
        {
            //Remplacement de la variable
            if (dbToMerge.Name.IsVariable())
                dbToMerge.Name = variables.First(v => v.Name == dbToMerge.Name).Value;

            if (!compiledServer.Databases.Exists(d => d.Name == dbToMerge.Name))
                compiledServer.Databases.Add(dbToMerge);

            //Template
            if (dbToMerge.UseTemplateId > 0)
            {
                var allDb = new List<DatabaseModifier>();

                //Append templates
                allDb.AddRange(templates.DatabaseModifiers);
                templates.ServerModifiers.ForEach(s => allDb.AddRange(s.Databases));

                //Append configs
                Servers.ForEach(s => allDb.AddRange(s.Databases));

                var db = allDb.First(d => d.TemplateId == dbToMerge.UseTemplateId);

                MergeDatabase(templates, variables, compiledServer, db);
                dbToMerge.UseTemplateId = 0;
            }

            //Merge
            foreach (var schema in dbToMerge.Schemas)
                MergeSchema(templates, variables, dbToMerge, schema);
        }

        private void MergeSchema(ModifiersTemplates templates,
                                 List<Variable> variables,
                                 DatabaseModifier compiledDatabase,
                                 SchemaModifier schemaToMerge)
        {
            //Remplacement de la variable
            if (schemaToMerge.Name.IsVariable())
                schemaToMerge.Name = variables.First(v => v.Name == schemaToMerge.Name).Value;

            if (!compiledDatabase.Schemas.Exists(d => d.Name == schemaToMerge.Name))
                compiledDatabase.Schemas.Add(schemaToMerge);

            //Template
            if (schemaToMerge.UseTemplateId > 0)
            {
                var allSchema = new List<SchemaModifier>();

                //Append templates
                allSchema.AddRange(templates.SchemaModifiers);
                templates.ServerModifiers.ForEach(s => s.Databases.ForEach(d => allSchema.AddRange(d.Schemas)));

                //Append configs
                Servers.ForEach(s => s.Databases.ForEach(d => allSchema.AddRange(d.Schemas)));

                var sch = allSchema.First(d => d.TemplateId == schemaToMerge.UseTemplateId);

                MergeSchema(templates, variables, compiledDatabase, sch);
                schemaToMerge.UseTemplateId = 0;
            }

            //Merge
            foreach (var table in schemaToMerge.Tables)
                MergeTable(variables, schemaToMerge, table);
        }

        private void MergeTable(List<Variable> variables,
                                SchemaModifier schema,
                                TableModifier tblToMerge)
        {
            //Remplacement de la variable
            if (tblToMerge.Name.IsVariable())
                tblToMerge.Name = variables.First(v => v.Name == tblToMerge.Name).Value;

            if (!schema.Tables.Exists(d => d.Name == tblToMerge.Name))
                schema.Tables.Add(tblToMerge);

            //TODO : 
            //MERGE ALL
        }
    }
}