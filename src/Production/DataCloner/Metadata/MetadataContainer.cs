using DataCloner.Configuration;
using DataCloner.Data;
using DataCloner.Framework;
using Murmur;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace DataCloner.Metadata
{
    public sealed class MetadataContainer
    {
        internal delegate void Initialiser(IQueryDispatcher dispatcher, Settings settings, ref MetadataContainer container);

        public string ConfigFileHash { get; set; }
        public Dictionary<ServerIdentifier, ServerIdentifier> ServerMap { get; set; }
        public List<SqlConnection> ConnectionStrings { get; set; }
        public AppMetadata Metadatas { get; set; }

        public MetadataContainer()
        {
            ServerMap = new Dictionary<ServerIdentifier, ServerIdentifier>();
            ConnectionStrings = new List<SqlConnection>();
            Metadatas = new AppMetadata();
        }

        /// <summary>
        /// Verify if the last build of the container's metadata still match with the current cloning settings.
        /// </summary>
        /// <param name="dispatcher">Query dispatcher</param>
        /// <param name="app">Application user config</param>
        /// <param name="mapId">MapId</param>
        /// <param name="behaviourId">BehaviourId</param>
        /// <param name="container">Container</param>
        public static void VerifyIntegrityWithSettings(IQueryDispatcher dispatcher, Settings settings, ref MetadataContainer container)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            if (settings == null) throw new ArgumentNullException("settings");
            if (settings.Project == null) throw new ArgumentNullException("settings.Project");
            if (String.IsNullOrWhiteSpace(settings.Project.Name)) throw new ArgumentException("settings.Project.Name");
            if (settings.Project.ConnectionStrings != null &&
                !settings.Project.ConnectionStrings.Any())
                throw new NullReferenceException("settings.Project.ConnectionStrings");

            var project = settings.Project;
            var containerFileName = project.Name + "_";
            Map map = null;
            Behaviour clonerBehaviour = null;

            //Hash the selected map, connnectionStrings and the cloner 
            //configuration to see if it match the lasted builded container
            var configData = new MemoryStream();
            var bf = SerializationHelper.DefaultFormatter;
            bf.Serialize(configData, project.ConnectionStrings);

            if (settings.MapId.HasValue)
            {
                map = settings.Project.Maps.FirstOrDefault(m => m.Id == settings.MapId);
                if (map == null)
                    throw new Exception(string.Format("Map id '{0}' not found in configuration file for application '{1}'!", settings.MapId, project.Name));
                containerFileName += map.From + "-" + map.To;
                bf.Serialize(configData, map);

                if (map.UsableBehaviours != null && map.UsableBehaviours.Split(',').ToList().Contains(settings.BehaviourId.ToString()))
                {
                    clonerBehaviour = project.Behaviours.FirstOrDefault(c => c.Id == settings.BehaviourId);
                    if (clonerBehaviour == null)
                        throw new KeyNotFoundException(
                            string.Format("There is no behaviour '{0}' in the configuration for the appName name '{1}'.", settings.BehaviourId, project.Name));

                    bf.Serialize(configData, clonerBehaviour);
                    bf.Serialize(configData, project.Templates);
                }
            }
            else
                containerFileName += "defaultMap";

            if (settings.BehaviourId != null)
                containerFileName += "_" + settings.BehaviourId;
            containerFileName += ".cache";

            //Hash user config
            configData.Position = 0;
            var murmur = MurmurHash.Create32(managed: false);
            var configHash = Encoding.Default.GetString(murmur.ComputeHash(configData));

            //If in-memory container is good, we use it
            if (container != null && container.ConfigFileHash == configHash)
                return;

            if (!settings.UseInMemoryCacheOnly)
            {
                //If container on disk is good, we use it
                container = TryLoadContainer(containerFileName, configHash);
                if (container != null)
                {
                    dispatcher.InitProviders(container.Metadatas, container.ConnectionStrings);
                    return;
                }
            }

            //We rebuild the container
            container = BuildMetadataWithSettings(dispatcher, containerFileName, project, clonerBehaviour, map, configHash);

            //Persist container
            if (!settings.UseInMemoryCacheOnly)
                container.Save(containerFileName);
        }

        /// <summary>
        /// Verify if the last build of the container's metadata still match with the sql servers (application's connectionStrings).
        /// </summary>
        /// <param name="dispatcher">Query dispatcher</param>
        /// <param name="proj">Project config</param>
        /// <param name="container">Metadata container</param>
        public static void VerifyIntegrityOfSqlMetadata(IQueryDispatcher dispatcher, ProjectContainer proj, ref MetadataContainer container)
        {
            if (dispatcher == null) throw new ArgumentNullException("dispatcher");
            if (proj == null) throw new ArgumentNullException("app");
            if (proj.ConnectionStrings != null && !proj.ConnectionStrings.Any()) throw new NullReferenceException("ConnectionStrings");

            var containerFileName = proj.Name + ".schema";

            //Hash the connnectionStrings to see if it match the last build
            var configData = new MemoryStream();
            var bf = SerializationHelper.DefaultFormatter;
            bf.Serialize(configData, proj.ConnectionStrings);
            configData.Position = 0;

            HashAlgorithm murmur = MurmurHash.Create32(managed: false);
            var configHash = Encoding.Default.GetString(murmur.ComputeHash(configData));

            //If in-memory container is good, we use it
            if (container != null && container.ConfigFileHash == configHash)
                return;
            //If container on disk is good, we use it
            container = TryLoadContainer(containerFileName, configHash);
            if (container != null)
                dispatcher.InitProviders(container.Metadatas, container.ConnectionStrings);
            //We rebuild the container
            else
                container = BuildMetadata(dispatcher, containerFileName, proj.ConnectionStrings, configHash);
        }

        public void Serialize(Stream output, FastAccessList<object> referenceTracking = null)
        {
            Serialize(new BinaryWriter(output, Encoding.UTF8, true), referenceTracking);
        }        

        public static MetadataContainer Deserialize(Stream input, FastAccessList<object> referenceTracking = null)
        {
            return Deserialize(new BinaryReader(input, Encoding.UTF8, true));
        }

        public void Serialize(BinaryWriter output, FastAccessList<object> referenceTracking = null)
        {
            output.Write(ConfigFileHash);

            if (ServerMap != null)
            {
                output.Write(ServerMap.Count);
                foreach (var sm in ServerMap)
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
            var fs = new FileStream(path, FileMode.Create);
            Serialize(fs);
            fs.Close();
        }

        public static MetadataContainer Deserialize(BinaryReader input, FastAccessList<object> referenceTracking = null)
        {
            var config = new MetadataContainer { ConfigFileHash = input.ReadString() };

            DeserializeBody(input, config, referenceTracking);

            return config;
        }

        /// <summary>
        /// Also merge user configs
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="containerFileName"></param>
        /// <param name="proj"></param>
        /// <param name="clonerBehaviour"></param>
        /// <param name="map"></param>
        /// <param name="configHash"></param>
        /// <returns></returns>
        private static MetadataContainer BuildMetadataWithSettings(IQueryDispatcher dispatcher, string containerFileName,
                                                                   ProjectContainer proj, Behaviour clonerBehaviour,
                                                                   Map map, string configHash)
        {
            var container = new MetadataContainer { ConfigFileHash = configHash, ServerMap = map.ConvertToDictionnary() };

            ////Get servers source
            //var serversSource = map.Roads.Select(r => r.ServerSrc).Distinct().ToList();

            ////Replace variables
            //for (int i = 0; i < serversSource.Count(); i++)
            //{
            //    if (serversSource[i].IsVariable())
            //    {
            //        var configVar = map.Variables.FirstOrDefault(v => v.Name == serversSource[i]);
            //        if (configVar == null)
            //            throw new Exception(string.Format("The variable '{0}' is not found in the map with id='{1}'.", serversSource[i], map.Id));
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

            FetchMetadata(dispatcher, ref container);

            Behaviour behaviour = null;
            if (clonerBehaviour != null)
            {
                List<Variable> vars = null;
                if (map != null)
                    vars = map.Variables;

                behaviour = clonerBehaviour.Build(proj.Templates, vars);
            }
            container.Metadatas.FinalizeMetadata(behaviour);

            return container;
        }

        /// <summary>
        /// Only build the database schema.
        /// </summary>
        /// <param name="dispatcher"></param>
        /// <param name="containerFileName"></param>
        /// <param name="connections"></param>
        /// <param name="configHash"></param>
        /// <returns></returns>
        private static MetadataContainer BuildMetadata(IQueryDispatcher dispatcher, string containerFileName,
                                                       List<Connection> connections, string configHash)
        {
            var container = new MetadataContainer { ConfigFileHash = configHash };

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

            FetchMetadata(dispatcher, ref container);
            container.Metadatas.FinalizeSchema();

            //Save container
            container.Save(containerFileName);
            return container;
        }

        private static void FetchMetadata(IQueryDispatcher dispatcher, ref MetadataContainer container)
        {
            dispatcher.InitProviders(container.Metadatas, container.ConnectionStrings);

            foreach (var cs in container.ConnectionStrings)
            {
                var provider = dispatcher.GetQueryHelper(cs.Id);

                foreach (var database in provider.GetDatabasesName())
                {
                    provider.GetColumns(container.Metadatas.LoadColumns, cs.Id, database);
                    provider.GetForeignKeys(container.Metadatas.LoadForeignKeys, cs.Id, database);
                    provider.GetUniqueKeys(container.Metadatas.LoadUniqueKeys, cs.Id, database);
                }
            }
        }

        private static MetadataContainer DeserializeBody(BinaryReader input, MetadataContainer config, 
            FastAccessList<object> referenceTracking = null)
        {
            var nbServerMap = input.ReadInt32();
            for (var i = 0; i < nbServerMap; i++)
            {
                var src = ServerIdentifier.Deserialize(input);
                var dst = ServerIdentifier.Deserialize(input);
                config.ServerMap.Add(src, dst);
            }
            var nbConnection = input.ReadInt32();
            for (var i = 0; i < nbConnection; i++)
                config.ConnectionStrings.Add(SqlConnection.Deserialize(input));

            config.Metadatas = AppMetadata.Deserialize(input, referenceTracking);

            return config;
        }

        private static MetadataContainer TryLoadContainer(string containerFile, string configHash)
        {
            MetadataContainer container = null;

            if (File.Exists(containerFile))
            {
                using (var fs = new FileStream(containerFile, FileMode.Open))
                using (var br = new BinaryReader(fs, Encoding.UTF8, true))
                {
                    var fileHash = br.ReadString();

                    //Check if container file match with config file version
                    if (fileHash == configHash)
                    {
                        container = new MetadataContainer { ConfigFileHash = fileHash };
                        DeserializeBody(br, container);
                    }
                }
            }
            return container;
        }
    }
}