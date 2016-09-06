using DataCloner.Core.Configuration;
using DataCloner.Core.Data;
using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataCloner.Core.Metadata.Context
{
    /// <summary>
    /// Represent a file containing all the metadatas used by an execution context.
    /// </summary>
    public sealed class MetadataCache : IMetadataCache
    {
        public string ConfigFileHash { get; set; }
        public Metadatas Metadatas { get; set; }

        private ExecutionContext ExecutionContextCache { get; }

        public MetadataCache()
        {
            Metadatas = new Metadatas();
        }

        /// <summary>
        /// Verify if the last build of the container's metadata still match with the current cloning context.
        /// </summary>
        public ExecutionContext LoadCache(ConfigurationProject project, CloningContext context = null)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (project.ConnectionStrings == null || !project.ConnectionStrings.Any())
                throw new NullReferenceException(nameof(project.ConnectionStrings));

            Behavior behavior = null;
            var containerFileName = project.Name + "_";
            MapFrom mapFrom = null;
            MapTo mapTo = null;
            
            //Hash the selected map, connnectionStrings and the cloner 
            //configuration to see if it match the lasted builded container
            var configData = new MemoryStream();
            SerializationHelper.Serialize(configData, project.ConnectionStrings);

            HashSet<Variable> variables = null;

            //Append context data
            if (context != null)
            {
                variables = project.GetVariablesForMap(context.From, context.To);

                if (context.From != null)
                {
                    //Map
                    mapFrom = project.Maps.FirstOrDefault(m => m.Name == context.From);
                    if (mapFrom == null)
                        throw new Exception($"MapFrom name '{context.From}' not found in configuration file for application '{project.Name}'!");
                    mapTo = mapFrom.MapTos.FirstOrDefault(m => m.Name == context.To);
                    if (mapTo == null)
                        throw new Exception($"MapTo name '{context.To}' not found in configuration file for application '{project.Name}'!");

                    containerFileName += context.From + "_" + context.To;
                    SerializationHelper.Serialize(configData, mapFrom);

                    //Behavior
                    if (context.BehaviourId.HasValue)
                    {
                        if (mapFrom.UsableBehaviours != null && mapFrom.UsableBehaviours.Split(',').ToList().Contains(context.BehaviourId.ToString()))
                        {
                            behavior = project.BuildBehavior(context.BehaviourId.GetValueOrDefault());

                            SerializationHelper.Serialize(configData, behavior);
                            SerializationHelper.Serialize(configData, project.Templates);

                            containerFileName += "_" + context.BehaviourId;
                        }                        
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
            if (ConfigFileHash == currentHash)
            {
                //Do nothing
            }
            //If container on disk is good, we use it
            else if ((context == null || !context.UseInMemoryCacheOnly) && TryLoadContainerFromFile(containerFileName, currentHash))
            {
                //queryProxy.Init(ConnectionStrings, Metadatas);
                InitExecutionContext(project, mapFrom?.Roads, variables);
            }
            //We rebuild the container
            else
            {
                ConfigFileHash = currentHash;
                InitExecutionContext(project, mapFrom?.Roads, variables);

                //queryProxy.Init(ConnectionStrings, Metadatas);
                MetadataBuilder.BuildMetadata(ExecutionContextCache.DbConnections, behavior, variables);

                Save(containerFileName);
            }

            return ExecutionContextCache;
        }

        private void InitExecutionContext(ConfigurationProject project, List<Road> roads, HashSet<Variable> variables)
        {
            //Init connection strings
            ExecutionContextCache.DbConnections = new Dictionary<short, IDbConnection>();
            foreach (var conn in project.ConnectionStrings)
            {
                var dbConnection = DbProviderFactories.GetFactory(conn.ProviderName).CreateConnection();
                dbConnection.ConnectionString = conn.ConnectionString;
                ExecutionContextCache.DbConnections.Add(conn.Id, dbConnection);
            }

            //Init metadatas
            ExecutionContextCache.Metadatas = Metadatas;

            //Init maps
            foreach (var road in roads)
            {
                var sourceVar = variables.First(v => v.Name == road.SourceVar);
                var destinationVar = variables.First(v => v.Name == road.DestinationVar);

                var source = new SehemaIdentifier
                {
                    ServerId = sourceVar.Server,
                    Database = sourceVar.Database,
                    Schema = sourceVar.Schema
                };

                var destination = new SehemaIdentifier
                {
                    ServerId = destinationVar.Server,
                    Database = destinationVar.Database,
                    Schema = destinationVar.Schema
                };

                if (!ExecutionContextCache.Map.ContainsKey(source))
                    ExecutionContextCache.Map.Add(source, destination);
            }
        }

        private bool TryLoadContainerFromFile(string containerFile, string currentConfigHash)
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
                        ConfigFileHash = fileHash;
                        DeserializeBody(br, this);
                        return true;
                    }
                }
            }
            return false;
        }

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                Serialize(fs);
            }
        }

        private static void DeserializeBody(BinaryReader input, MetadataCache config,
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

            config.Metadatas = Metadatas.Deserialize(input, referenceTracking);
        }

        public void Serialize(Stream output, FastAccessList<object> referenceTracking = null)
        {
            Serialize(new BinaryWriter(output, Encoding.UTF8, true), referenceTracking);
        }

        public static MetadataCache Deserialize(Stream input, FastAccessList<object> referenceTracking = null)
        {
            return Deserialize(new BinaryReader(input, Encoding.UTF8, true));
        }

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking = null)
        {
            output.Write(ConfigFileHash);

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
            Metadatas.Serialize(output, referenceTracking);

            output.Flush();
        }

        public static MetadataCache Deserialize(BinaryReader input, FastAccessList<object> referenceTracking = null)
        {
            var config = new MetadataCache { ConfigFileHash = input.ReadString() };

            DeserializeBody(input, config, referenceTracking);

            return config;
        }
    }
}