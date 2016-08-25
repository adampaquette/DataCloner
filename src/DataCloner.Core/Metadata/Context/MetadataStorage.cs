﻿using DataCloner.Core.Configuration;
using DataCloner.Core.Data;
using DataCloner.Core.Framework;
using DataCloner.Core.Metadata.Context;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DataCloner.Core.Metadata.Context
{
    /// <summary>
    /// Represent a file containing all the metadatas used by an execution context.
    /// </summary>
    public sealed class MetadataStorage
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
        /// <param name="queryProxy">Query proxy</param>
        /// <param name="context">Current cloning context</param>
        /// <param name="container">Container</param>
        public static void VerifyIntegrityWithSettings(IQueryProxy queryProxy, CloningContext context, ref MetadataStorage container)
        {
            //if (dispatcher == null) throw new ArgumentNullException(nameof(dispatcher));
            //if (context == null) throw new ArgumentNullException(nameof(context));
            //if (context.Project == null) throw new ArgumentNullException(nameof(context.Project));
            //if (String.IsNullOrWhiteSpace(context.Project.Name)) throw new ArgumentException(nameof(context.Project.Name));
            //if (context.Project.ConnectionStrings != null &&
            //    !context.Project.ConnectionStrings.Any())
            //    throw new NullReferenceException("settings.Project.ConnectionStrings");

            //var project = context.Project;
            //var containerFileName = project.Name + "_";
            //MapFrom map = null;
            //Behavior clonerBehaviour = null;

            ////Hash the selected map, connnectionStrings and the cloner 
            ////configuration to see if it match the lasted builded container
            //var configData = new MemoryStream();
            //SerializationHelper.Serialize(configData, project.ConnectionStrings);

            //if (context.MapId.HasValue)
            //{
            //    map = context.Project.Maps.FirstOrDefault(m => m.Id == context.MapId);
            //    if (map == null)
            //        throw new Exception($"Map id '{context.MapId}' not found in configuration file for application '{project.Name}'!");
            //    containerFileName += map.From + "-" + map.To;
            //    SerializationHelper.Serialize(configData, map);

            //    if (map.UsableBehaviours != null && map.UsableBehaviours.Split(',').ToList().Contains(context.BehaviourId.ToString()))
            //    {
            //        clonerBehaviour = project.Behaviors.FirstOrDefault(c => c.Id == context.BehaviourId);
            //        if (clonerBehaviour == null)
            //            throw new KeyNotFoundException(
            //                $"There is no behaviour '{context.BehaviourId}' in the configuration for the appName name '{project.Name}'.");

            //        SerializationHelper.Serialize(configData, clonerBehaviour);
            //        SerializationHelper.Serialize(configData, project.Templates);
            //    }
            //}
            //else
            //    containerFileName += "defaultMap";

            //if (context.BehaviourId != null)
            //    containerFileName += "_" + context.BehaviourId;
            //containerFileName += ".cache";

            ////Hash user config
            //configData.Position = 0;
            ////var murmur = MurmurHash.Create32(managed: false);
            ////var configHash = Encoding.UTF8.GetString(murmur.ComputeHash(configData));
            //var configHash = container.ConfigFileHash + "random";

            ////If in-memory container is good, we use it
            //if (container != null && container.ConfigFileHash == configHash)
            //    return;

            //if (!context.UseInMemoryCacheOnly)
            //{
            //    //If container on disk is good, we use it
            //    container = TryLoadContainer(containerFileName, configHash);
            //    if (container != null)
            //    {
            //        dispatcher.InitProviders(container.Metadatas, container.ConnectionStrings);
            //        return;
            //    }
            //}

            ////We rebuild the container
            //container = BuildMetadataWithSettings(dispatcher, containerFileName, project, clonerBehaviour, map, configHash);

            ////Persist container
            //if (!context.UseInMemoryCacheOnly)
            //    container.Save(containerFileName);
        }

        /// <summary>
        /// Verify if the last build of the container's metadata still match with the sql servers (application's connectionStrings).
        /// </summary>
        /// <param name="queryProxy">Query proxy</param>
        /// <param name="proj">Project config</param>
        /// <param name="container">Metadata container</param>
        public static void VerifyIntegrityOfSqlMetadata(IQueryProxy queryProxy, ConfigurationProject proj, ref MetadataStorage container)
        {
            if (queryProxy == null) throw new ArgumentNullException(nameof(queryProxy));
            if (proj == null) throw new ArgumentNullException(nameof(proj));
            if (proj.ConnectionStrings != null && !proj.ConnectionStrings.Any()) throw new NullReferenceException("ConnectionStrings");

            var containerFileName = proj.Name + ".schema";

            //Hash the connnectionStrings to see if it match the last build
            var configData = new MemoryStream();
            SerializationHelper.Serialize(configData, proj.ConnectionStrings);
            configData.Position = 0;

            //HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            //var configHash = Encoding.UTF8.GetString(murmur.ComputeHash(configData));
            var configHash = container.ConfigFileHash + "random";

            //If in-memory container is good, we use it
            if (container != null && container.ConfigFileHash == configHash)
                return;
            //If container on disk is good, we use it
            container = TryLoadContainer(containerFileName, configHash);
            if (container != null)
                queryProxy.Init(container.Metadatas, container.ConnectionStrings);
            //We rebuild the container
            else
                container = BuildMetadata(queryProxy, containerFileName, proj.ConnectionStrings, configHash);
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

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                Serialize(fs);
            }
        }

        public static MetadataStorage Deserialize(BinaryReader input, FastAccessList<object> referenceTracking = null)
        {
            var config = new MetadataStorage { ConfigFileHash = input.ReadString() };

            DeserializeBody(input, config, referenceTracking);

            return config;
        }

        /// <summary>
        /// Also merge user configs
        /// </summary>
        /// <param name="queryProxy"></param>
        /// <param name="containerFileName"></param>
        /// <param name="proj"></param>
        /// <param name="clonerBehaviour"></param>
        /// <param name="map"></param>
        /// <param name="configHash"></param>
        /// <returns></returns>
        private static MetadataStorage BuildMetadataWithSettings(IQueryProxy queryProxy, string containerFileName,
                                                                 ConfigurationProject proj, Behavior clonerBehaviour,
                                                                 MapFrom map, string configHash)
        {
            var container = new MetadataStorage { ConfigFileHash = configHash, Map = map.ConvertToDictionnary() };

            ////Get servers source
            //var serversSource = map.Roads.Select(r => r.ServerSrc).Distinct().ToList();

            ////Replace variables
            //for (int i = 0; i < serversSource.Count(); i++)
            //{
            //    if (serversSource[i].IsVariable())
            //    {
            //        var configVar = map.Variables.FirstOrDefault(v => v.Name == serversSource[i]);
            //        if (configVar == null)
            //            throw new Exception($"The variable '{0}' is not found in the map with id='{1}'.", serversSource[i], map.Id));
            //        serversSource[i] = configVar.Value.ToString();
            //    }
            //}

            ////Copy connection strings
            //foreach (var cs in app.ConnectionStrings.Where(c => serversSource.Contains(c.Id.ToString())))
            //    container.ConnectionStrings.Add(new SqlConnection(cs.Id, cs.ProviderName, cs.ConnectionString));

            //Copy connection strings
            foreach (var cs in proj.ConnectionStrings)
            {
                container.ConnectionStrings.Add(
                    new SqlConnection(cs.Id)
                    {
                        ProviderName = cs.ProviderName,
                        ConnectionString = cs.ConnectionString
                    });
            }

            if (container.ConnectionStrings.Count() == 0)
                throw new Exception("No connectionStrings!");

            FetchMetadata(queryProxy, ref container);

            Behavior behaviour = null;
            if (clonerBehaviour != null)
            {
                List<Variable> vars = null;
                if (map != null)
                    vars = map.Variables;

                //TODO : behaviour = clonerBehaviour.BuildBehavior(proj.Templates, vars);
            }
            container.Metadatas.FinalizeMetadata(behaviour);

            return container;
        }

        /// <summary>
        /// Only build the database schema.
        /// </summary>
        /// <param name="queryProxy"></param>
        /// <param name="containerFileName"></param>
        /// <param name="connections"></param>
        /// <param name="configHash"></param>
        /// <returns></returns>
        private static MetadataStorage BuildMetadata(IQueryProxy queryProxy, string containerFileName,
                                                       List<Connection> connections, string configHash)
        {
            var container = new MetadataStorage { ConfigFileHash = configHash };

            //Copy connection strings
            foreach (var cs in connections)
            {
                container.ConnectionStrings.Add(
                    new SqlConnection(cs.Id)
                    {
                        ProviderName = cs.ProviderName,
                        ConnectionString = cs.ConnectionString
                    });
            }

            FetchMetadata(queryProxy, ref container);
            container.Metadatas.FinalizeSchema();

            //Save container
            container.Save(containerFileName);
            return container;
        }

        private static void FetchMetadata(IQueryProxy queryProxy, ref MetadataStorage container)
        {
            queryProxy.Init(container.Metadatas, container.ConnectionStrings);

            foreach (var cs in container.ConnectionStrings)
            {
                foreach (var database in queryProxy.GetDatabasesName(cs.Id))
                {
                    queryProxy.LoadColumns(cs.Id, database);
                    queryProxy.LoadForeignKeys(cs.Id, database);
                    queryProxy.LoadUniqueKeys(cs.Id, database);
                }
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

        private static MetadataStorage TryLoadContainer(string containerFile, string configHash)
        {
            MetadataStorage container = null;

            if (File.Exists(containerFile))
            {
                using (var fs = new FileStream(containerFile, FileMode.Open))
                using (var br = new BinaryReader(fs, Encoding.UTF8, true))
                {
                    var fileHash = br.ReadString();

                    //Check if container file match with config file version
                    if (fileHash == configHash)
                    {
                        container = new MetadataStorage { ConfigFileHash = fileHash };
                        DeserializeBody(br, container);
                    }
                }
            }
            return container;
        }
    }
}