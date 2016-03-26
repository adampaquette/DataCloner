using System;
using System.Collections.Generic;
using System.Linq;

namespace DataCloner.Configuration
{
    [Serializable]
    public static class BehaviourExtension
    {
        /// <summary>
        /// Build the behaviour in-memory from the templates.
        /// </summary>
        /// <param name="templates">Templates of the Application config section.</param>
        /// <param name="variables">Variables of the selected Map config section.</param>
        public static Behaviour Build(this Behaviour behaviour, Modifiers templates, List<Variable> variables)
        {
            var compiledServersModifier = new Dictionary<string, ServerModifier>();

            foreach (var server in behaviour.Modifiers.ServerModifiers)
                MergeServer(behaviour, templates, variables, compiledServersModifier, server);

            return new Behaviour
            {
                Id = behaviour.Id,
                Name = behaviour.Name,
                Description = behaviour.Description,

                Modifiers = new Modifiers
                {
                    /*DatabaseModifiers et SchemaModifiers se retrouvent compilés sous les modificateurs de serveur*/
                    ServerModifiers = compiledServersModifier.Values.ToList()
                }
            };
        }

        private static void MergeServer(Behaviour behaviour,
                                        Modifiers templates,
                                        List<Variable> variables,
                                        Dictionary<string, ServerModifier> compiledConfig,
                                        ServerModifier srvToMerge)
        {
            //Remplacement de la variable
            if (srvToMerge.Id.IsVariable())
                srvToMerge.Id = variables.First(v => v.Name.ParseConfigVariable().Key == srvToMerge.Id.ParseConfigVariable().Key).Value;

            if (!compiledConfig.ContainsKey(srvToMerge.Id))
                compiledConfig.Add(srvToMerge.Id, srvToMerge);

            //Template
            if (srvToMerge.BasedOn > 0)
            {
                var srvTemplate = behaviour.Modifiers.ServerModifiers.FirstOrDefault(s => s.TemplateId == srvToMerge.BasedOn);
                if (srvTemplate == null)
                    srvTemplate = templates.ServerModifiers.First(s => s.TemplateId == srvToMerge.BasedOn);

                MergeServer(behaviour, templates, variables, compiledConfig, srvTemplate);
                srvToMerge.TemplateId = 0;
                srvToMerge.BasedOn = 0;
            }

            //Merge
            foreach (var database in srvToMerge.Databases)
            {
                var dstServer = compiledConfig[srvToMerge.Id];
                MergeDatabase(behaviour, templates, variables, dstServer, database);
            }
        }

        private static void MergeDatabase(Behaviour behaviour,
                                          Modifiers templates,
                                          List<Variable> variables,
                                          ServerModifier compiledServer,
                                          DatabaseModifier dbToMerge)
        {
            //Remplacement de la variable
            if (dbToMerge.Name.IsVariable())
                dbToMerge.Name = variables.First(v => v.Name.ParseConfigVariable().Key == dbToMerge.Name.ParseConfigVariable().Key).Value;
            dbToMerge.Name = dbToMerge.Name.ToLower();

            if (!compiledServer.Databases.Exists(d => d.Name == dbToMerge.Name))
                compiledServer.Databases.Add(dbToMerge);

            //Template
            if (dbToMerge.BasedOn > 0)
            {
                var allDb = new List<DatabaseModifier>();

                //Pass 1 : Search inside the behaviour
                allDb.AddRange(behaviour.Modifiers.DatabaseModifiers);
                behaviour.Modifiers.ServerModifiers.ForEach(s => allDb.AddRange(s.Databases));

                var db = allDb.FirstOrDefault(d => d.TemplateId == dbToMerge.BasedOn);
                if (db == null)
                {
                    //Pass 2 : Search inside the templates
                    allDb.Clear();
                    allDb.AddRange(templates.DatabaseModifiers);
                    templates.ServerModifiers.ForEach(s => allDb.AddRange(s.Databases));
                }
                db = allDb.First(d => d.TemplateId == dbToMerge.BasedOn);

                MergeDatabase(behaviour, templates, variables, compiledServer, db);
                dbToMerge.TemplateId = 0;
                dbToMerge.BasedOn = 0;
            }

            //Merge
            foreach (var schema in dbToMerge.Schemas)
            {
                var dstDb = compiledServer.Databases.First(d => d.Name == dbToMerge.Name);
                MergeSchema(behaviour, templates, variables, dstDb, schema);
            }
        }

        private static void MergeSchema(Behaviour behaviour,
                                        Modifiers templates,
                                        List<Variable> variables,
                                        DatabaseModifier compiledDatabase,
                                        SchemaModifier schemaToMerge)
        {
            //Remplacement de la variable
            if (schemaToMerge.Name.IsVariable())
                schemaToMerge.Name = variables.First(v => v.Name.ParseConfigVariable().Key == schemaToMerge.Name.ParseConfigVariable().Key).Value;
            schemaToMerge.Name = schemaToMerge.Name.ToLower();

            if (!compiledDatabase.Schemas.Exists(d => d.Name == schemaToMerge.Name))
                compiledDatabase.Schemas.Add(schemaToMerge);

            //Template
            if (schemaToMerge.BasedOn > 0)
            {
                var allSchema = new List<SchemaModifier>();

                //Pass 1 : Search inside the behaviour
                allSchema.AddRange(behaviour.Modifiers.SchemaModifiers);
                behaviour.Modifiers.ServerModifiers.ForEach(s => s.Databases.ForEach(d => allSchema.AddRange(d.Schemas)));

                var sch = allSchema.First(d => d.TemplateId == schemaToMerge.BasedOn);
                if (sch == null)
                {
                    //Pass 2 : Search inside the templates
                    allSchema.Clear();
                    allSchema.AddRange(templates.SchemaModifiers);
                    templates.ServerModifiers.ForEach(s => s.Databases.ForEach(d => allSchema.AddRange(d.Schemas)));
                }
                sch = allSchema.First(d => d.TemplateId == schemaToMerge.BasedOn);

                MergeSchema(behaviour, templates, variables, compiledDatabase, sch);
                schemaToMerge.TemplateId = 0;
                schemaToMerge.BasedOn = 0;
            }

            //Merge
            foreach (var table in schemaToMerge.Tables)
            {
                var dstSchema = compiledDatabase.Schemas.First(d => d.Name == schemaToMerge.Name);
                MergeTable(variables, dstSchema, table);
            }
        }

        private static void MergeTable(List<Variable> variables,
                                       SchemaModifier schema,
                                       TableModifier tblToMerge)
        {
            //Remplacement de la variable
            if (tblToMerge.Name.IsVariable())
                tblToMerge.Name = variables.First(v => v.Name.ParseConfigVariable().Key == tblToMerge.Name.ParseConfigVariable().Key).Value;
            tblToMerge.Name = tblToMerge.Name.ToLower();

            if (!schema.Tables.Exists(d => d.Name == tblToMerge.Name))
                schema.Tables.Add(tblToMerge);

            //TODO : 
            //MERGE ALL
        }
    }
}