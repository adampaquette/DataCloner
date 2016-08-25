using DataCloner.Core.Configuration;
using DataCloner.Core.Data;
using DataCloner.Core.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataCloner.Core.Metadata.Context
{
    /// <summary>
    /// Represent a file containing all the metadatas used by an execution context.
    /// </summary>
    public sealed class MetadataStorage : IMetadataStorage
    {
        internal delegate void Initialiser(IQueryProxy queryProxy, CloningContext settings, ref MetadataStorage container);

        public string ConfigFileHash { get; set; }
        public Dictionary<SehemaIdentifier, SehemaIdentifier> Map { get; set; }
        public List<SqlConnection> ConnectionStrings { get; set; }
        public Metadatas Metadatas { get; set; }

        public MetadataStorage()
        {
            Map = new Dictionary<SehemaIdentifier, SehemaIdentifier>();
            ConnectionStrings = new List<SqlConnection>();
            Metadatas = new Metadatas();
        }

        /// <summary>
        /// Verify if the last build of the container's metadata still match with the current cloning context.
        /// </summary>
        public void LoadMetadata(ConfigurationProject project, ref IQueryProxy queryProxy, CloningContext context = null)
        {
            if (queryProxy == null) throw new ArgumentNullException(nameof(queryProxy));
            if (project == null) throw new ArgumentNullException(nameof(project));
            if (project.ConnectionStrings != null &&
                !project.ConnectionStrings.Any())
                throw new NullReferenceException("settings.Project.ConnectionStrings");

            var containerFileName = project.Name + "_";
            MapFrom map = null;
            Behavior behavior = null;

            //Hash the selected map, connnectionStrings and the cloner 
            //configuration to see if it match the lasted builded container
            var configData = new MemoryStream();
            SerializationHelper.Serialize(configData, project.ConnectionStrings);

            //Append context data
            if (context != null)
            {
                if (context.From != null)
                {
                    //Map
                    map = project.Maps.FirstOrDefault(m => m.Name == context.From);
                    if (map == null)
                        throw new Exception($"Map name '{context.From}' not found in configuration file for application '{project.Name}'!");
                    containerFileName += context.From + "_" + context.To;
                    SerializationHelper.Serialize(configData, map);

                    //Behavior
                    if (context.BehaviourId.HasValue)
                    {
                        if (map.UsableBehaviours != null && map.UsableBehaviours.Split(',').ToList().Contains(context.BehaviourId.ToString()))
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
            var currentHash = "";
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
                queryProxy.Init(ConnectionStrings, Metadatas);
            }
            //We rebuild the container
            else
            {
                ConfigFileHash = currentHash;
                //Copy connection strings
                foreach (var cs in project.ConnectionStrings)
                {
                    ConnectionStrings.Add(
                        new SqlConnection(cs.Id)
                        {
                            ProviderName = cs.ProviderName,
                            ConnectionString = cs.ConnectionString
                        });
                }

                queryProxy.Init(ConnectionStrings, Metadatas);
                MetadataBuilder.BuildMetadata(project.ConnectionStrings, queryProxy, behavior);

                Save(containerFileName);
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

        private static MetadataStorage DeserializeBody(BinaryReader input, MetadataStorage config,
                                                       FastAccessList<object> referenceTracking = null)
        {
            var nbServerMap = input.ReadInt32();
            for (var i = 0; i < nbServerMap; i++)
            {
                var src = SehemaIdentifier.Deserialize(input);
                var dst = SehemaIdentifier.Deserialize(input);
                config.Map.Add(src, dst);
            }
            var nbConnection = input.ReadInt32();
            for (var i = 0; i < nbConnection; i++)
                config.ConnectionStrings.Add(SqlConnection.Deserialize(input));

            config.Metadatas = Metadatas.Deserialize(input, referenceTracking);

            return config;
        }

        public void Serialize(Stream output, FastAccessList<object> referenceTracking = null)
        {
            Serialize(new BinaryWriter(output, Encoding.UTF8, true), referenceTracking);
        }

        public static MetadataStorage Deserialize(Stream input, FastAccessList<object> referenceTracking = null)
        {
            return Deserialize(new BinaryReader(input, Encoding.UTF8, true));
        }

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking = null)
        {
            output.Write(ConfigFileHash);

            if (Map != null)
            {
                output.Write(Map.Count);
                foreach (var sm in Map)
                {
                    sm.Key.Serialize(output);
                    sm.Value.Serialize(output);
                }
            }
            else
                output.Write(0);

            output.Write(ConnectionStrings.Count);
            foreach (var cs in ConnectionStrings)
                cs.Serialize(output);
            Metadatas.Serialize(output, referenceTracking);

            output.Flush();
        }

        public static MetadataStorage Deserialize(BinaryReader input, FastAccessList<object> referenceTracking = null)
        {
            var config = new MetadataStorage { ConfigFileHash = input.ReadString() };

            DeserializeBody(input, config, referenceTracking);

            return config;
        }
    }
}