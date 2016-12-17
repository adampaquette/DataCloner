using DataCloner.Core.Configuration;
using DataCloner.Core.Framework;
using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataCloner.Core.Plan
{
    /// <summary>
    /// Represent a context containing all the metadatas, connections and maps used by an ExecutionPlanBuilder.
    /// </summary>
    /// <returns>Used only by <see cref="ExecutionPlanBuilder". Internal purpose only./></returns>
    public sealed class ExecutionContext : IExecutionContext
    {
        public string ExecutionContextCacheHash { get; set; }
        public Dictionary<SehemaIdentifier, SehemaIdentifier> Map { get; set; }
        public ConnectionsContext ConnectionsContext { get; set; }

        public ExecutionContext()
        {
            ConnectionsContext = new ConnectionsContext();
            Map = new Dictionary<SehemaIdentifier, SehemaIdentifier>();
        }

        /// <summary>
        /// Verify if the last build of the container's metadata still match with the current cloning context.
        /// </summary>
        public void Initialize(Project project, CloningContext context = null)
        {
            LoadCache(project, context);
        }

        #region Load

        /// <summary>
        /// Verify if the last build of the container's metadata still match with the current cloning context.
        /// </summary>
        private void LoadCache(Project project, CloningContext context = null)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (project.ConnectionStrings == null || !project.ConnectionStrings.Any())
                throw new NullReferenceException(nameof(project.ConnectionStrings));

            Behavior behavior = null;
            var containerFileName = project.Name + "_";
            Configuration.Environment source = null;
            Configuration.Environment destination = null;

            //Hash the selected map, connnectionStrings and the cloner 
            //configuration to see if it match the lasted builded container
            var configData = new MemoryStream();
            SerializationHelper.Serialize(configData, project.ConnectionStrings);

            //Append context data
            if (context != null)
            {
                if (context.SourceEnvironment != null)
                {
                    //Map
                    source = project.Environments.FirstOrDefault(e => e.Name == context.SourceEnvironment);
                    if (source == null)
                        throw new Exception($"Source environment name '{context.SourceEnvironment}' not found in configuration file for application '{project.Name}'!");
                    destination = project.Environments.FirstOrDefault(e => e.Name == context.DestinationEnvironment);
                    if (destination == null)
                        throw new Exception($"Destination environment name '{context.DestinationEnvironment}' not found in configuration file for application '{project.Name}'!");

                    containerFileName += context.SourceEnvironment + "_" + context.DestinationEnvironment;
                    SerializationHelper.Serialize(configData, source);

                    //Behavior
                    if (!string.IsNullOrWhiteSpace(context.Behaviour))
                    {
                        behavior = project.BuildBehavior(context.Behaviour);

                        SerializationHelper.Serialize(configData, behavior);
                        SerializationHelper.Serialize(configData, project.ExtractionTemplates);

                        containerFileName += "_" + context.Behaviour;
                    }
                }
                else
                    containerFileName += "_noMap";
            }
            containerFileName += ".cache";

            //Hash user config
            configData.Position = 0;
            string currentHash;
            using (var md5Hash = MD5.Create())
                currentHash = Encoding.UTF8.GetString(md5Hash.ComputeHash(configData));

            //If in-memory container is good, we use it
            if (ExecutionContextCacheHash == currentHash)
                return;

            Metadatas metadatas = null;
            //If we can load cache from disk
            if ((context == null || !context.UseInMemoryCacheOnly))
            {
                //If container on disk is good, we use it
                if (TryLoadContainerFromFile(containerFileName, currentHash, ref metadatas))
                {
                    InitExecutionContext(project, source, destination, currentHash, metadatas);
                    return;
                }
            }

            //Else we rebuild the container
            var schemas = new HashSet<SchemaVar>(source.Schemas);
            metadatas = MetadataBuilder.BuildMetadata(project.ConnectionStrings, behavior, schemas);
            InitExecutionContext(project, source, destination, currentHash, metadatas);

            if ((context == null || !context.UseInMemoryCacheOnly))
                Save(containerFileName);

        }

        private void InitExecutionContext(Project project, Configuration.Environment sourceEnvir, 
                                          Configuration.Environment destinationEnvir, 
                                          string configFileHash, Metadatas metadatas = null)
        {
            ExecutionContextCacheHash = configFileHash;

            //Init connection strings
            ConnectionsContext.Initialize(project.ConnectionStrings, metadatas);

            //Init maps
            foreach (var sourceSchema in sourceEnvir.Schemas)
            {
                var destinationSchema = destinationEnvir.Schemas.FirstOrDefault(s => s.Id == sourceSchema.Id);
                if (destinationSchema == null)
                    throw new Exception($"The destination schema {sourceSchema.Id} is not found in the environment {destinationEnvir.Name}. Please declare it.");

                var source = new SehemaIdentifier
                {
                    ServerId = sourceSchema.Server,
                    Database = sourceSchema.Database,
                    Schema = sourceSchema.Schema
                };

                var destination = new SehemaIdentifier
                {
                    ServerId = destinationSchema.Server,
                    Database = destinationSchema.Database,
                    Schema = destinationSchema.Schema
                };

                if (!Map.ContainsKey(source))
                    Map.Add(source, destination);
            }
        }

        private bool TryLoadContainerFromFile(string containerFile, string currentConfigHash, ref Metadatas metadatas)
        {
            if (File.Exists(containerFile))
            {
                using (var fs = new FileStream(containerFile, FileMode.Open))
                using (var br = new BinaryReader(fs, Encoding.UTF8, true))
                {
                    var fileHash = br.ReadString();

                    //Check if container file match with config file version
                    if (fileHash == currentConfigHash)
                    {
                        DeserializeBody(br, ref metadatas);
                        return true;
                    }
                }
            }
            return false;
        }

        public static ExecutionContext Deserialize(BinaryReader input, FastAccessList<object> referenceTracking = null)
        {
            var config = new ExecutionContext { ExecutionContextCacheHash = input.ReadString() };

            //DeserializeBody(input, config, referenceTracking);

            return config;
        }

        public static ExecutionContext Deserialize(Stream input, FastAccessList<object> referenceTracking = null)
        {
            return Deserialize(new BinaryReader(input, Encoding.UTF8, true));
        }

        private static void DeserializeBody(BinaryReader input,
                                            ref Metadatas metadatas,
                                            FastAccessList<object> referenceTracking = null)
        {
            //var nbServerMap = input.ReadInt32();
            //for (var i = 0; i < nbServerMap; i++)
            //{
            //    var src = SehemaIdentifier.Deserialize(input);
            //    var dst = SehemaIdentifier.Deserialize(input);
            //    config.Map.Add(src, dst);
            //}
            //var nbConnection = input.ReadInt32();
            //for (var i = 0; i < nbConnection; i++)
            //    config.ConnectionStrings.Add(SqlConnection.Deserialize(input));

            metadatas = Metadatas.Deserialize(input, referenceTracking);
        }

        #endregion

        #region Save

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                Serialize(fs);
            }
        }

        public void Serialize(Stream output, FastAccessList<object> referenceTracking = null)
        {
            Serialize(new BinaryWriter(output, Encoding.UTF8, true), referenceTracking);
        }

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking = null)
        {
            output.Write(ExecutionContextCacheHash);

            //if (Map != null)
            //{
            //    output.Write(Map.Count);
            //    foreach (var sm in Map)
            //    {
            //        sm.Key.Serialize(output);
            //        sm.Value.Serialize(output);
            //    }
            //}
            //else
            //    output.Write(0);

            //output.Write(ConnectionStrings.Count);
            //foreach (var cs in ConnectionStrings)
            //    cs.Serialize(output);

            ConnectionsContext.Metadatas.Serialize(output, referenceTracking);

            output.Flush();
        }

        #endregion
    }
}