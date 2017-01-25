using DataCloner.Core.Configuration;
using DataCloner.Core.Framework;
using DataCloner.Core.Internal;
using DataCloner.Core.Metadata;
using DataCloner.Core.Metadata.Context;
using DataCloner.Core.Plan;
//using LZ4;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCloner.Core.Clone
{
    public class Query
    {
        public const int CurrentFormatVersion = 1;

        private readonly ConnectionsContext _connectionsContext;
        private readonly ExecutionPlanByServer _executionPlanByServer;

        public int FormatVersion { get; set; }

        public string Description { get; set; }

        public bool EnforceIntegrity { get; set; }

        public event QueryCommitingEventHandler Commiting;

        internal Query(ConnectionsContext connectionsContext,
            ExecutionPlanByServer executionPlanByServer,
            int formatVersion)
        {
            _connectionsContext = connectionsContext;
            _executionPlanByServer = executionPlanByServer;
            FormatVersion = formatVersion;
        }

        public void ChangeDestination()
        {
            /*TODO : 
            modifier connections
            adapter execution plan
            adapter schéma
            */
        }

        public ResultSet Execute()
        {
            //_dispatcher[riSource].EnforceIntegrityCheck(EnforceIntegrity);
            PlugIn.DataBuilder.ClearBuildersCache();
            ResetExecutionPlan(_executionPlanByServer);
            Parallel.ForEach(_executionPlanByServer, ep =>
            {
                var ctx = _connectionsContext[ep.Key];
                ctx.QueryProvider.QueryCommmiting += Commiting;
                ctx.QueryProvider.Execute(ctx.Connection, _connectionsContext.Metadatas, ep.Value);
                ctx.QueryProvider.QueryCommmiting -= Commiting;
            });

            return new ResultSet(_executionPlanByServer);
        }

        public void ProduceSqlScript(Stream output)
        {
            //_dispatcher[riSource].EnforceIntegrityCheck(EnforceIntegrity);
            PlugIn.DataBuilder.ClearBuildersCache();
            ResetExecutionPlan(_executionPlanByServer);
            Parallel.ForEach(_executionPlanByServer, ep =>
            {
                var ctx = _connectionsContext[ep.Key];
                ctx.QueryProvider.QueryCommmiting += Commiting;
                ctx.QueryProvider.Execute(ctx.Connection, _connectionsContext.Metadatas, ep.Value);
                ctx.QueryProvider.QueryCommmiting -= Commiting;
            });
        }

        public void Save(string path)
        {
            using (var fstream = new FileStream(path, FileMode.Create))
                Save(fstream);
        }

        public void Save(Stream ostream)
        {
            var referenceTracking = new FastAccessList<object>();
            var refStream = new MemoryStream();

            _executionPlanByServer.Serialize(refStream, referenceTracking);
            _connectionsContext.Metadatas.Serialize(refStream, referenceTracking);

            ////Compression
            //using (var lzStream = new LZ4Stream(ostream, LZ4StreamMode.Compress))
            //using (var bstream = new BinaryWriter(lzStream, Encoding.UTF8, true))
            //{
            //    bstream.Write(FormatVersion);
            //    bstream.Write(Description ?? "");
            //    SerializeReferenceTracking(bstream, referenceTracking);
            //    SerializeConnections(bstream, _connections);
            //    refStream.WriteTo(lzStream);
            //}

            //Compression
            using (var bstream = new BinaryWriter(ostream, Encoding.UTF8, true))
            {
                bstream.Write(FormatVersion);
                bstream.Write(Description ?? "");
                SerializeReferenceTracking(bstream, referenceTracking);
                SerializeConnections(bstream, _connectionsContext.Connections.Keys.ToList());
                refStream.WriteTo(ostream);
            }
        }

        public static Query Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            int formatVersion;
            string description;
            HashSet<Connection> connections;
            Metadatas metadata;
            ExecutionPlanByServer executionPlanByServer;
            FastAccessList<object> referenceTracking;

            //using (var istream = new FileStream(path, FileMode.Open))
            //using (var lzstream = new LZ4Stream(istream, CompressionMode.Decompress))
            //using (var bstream = new BinaryReader(lzstream, Encoding.UTF8, true))
            //{
            //    formatVersion = bstream.ReadInt32();
            //    description = bstream.ReadString();
            //    referenceTracking = DeserializeReferenceTracking(bstream);
            //    connections = DeserializeConnections(bstream);
            //    executionPlanByServer = ExecutionPlanByServer.Deserialize(bstream, referenceTracking);
            //    metadata = AppMetadata.Deserialize(bstream, referenceTracking);
            //}

            using (var istream = new FileStream(path, FileMode.Open))
            using (var bstream = new BinaryReader(istream, Encoding.UTF8, true))
            {
                formatVersion = bstream.ReadInt32();
                description = bstream.ReadString();
                referenceTracking = DeserializeReferenceTracking(bstream);
                connections = DeserializeConnections(bstream);
                executionPlanByServer = ExecutionPlanByServer.Deserialize(bstream, referenceTracking);
                metadata = Metadatas.Deserialize(bstream, referenceTracking);
            }

            //Initialize a connection context with values used at the time of the creation of the clone.
            var connectionsContext = new ConnectionsContext();
            connectionsContext.Initialize(connections.ToList(), metadata);

            return new Query(connectionsContext, executionPlanByServer, formatVersion)
            {
                Description = description
            };
        }

        private static void SerializeReferenceTracking(BinaryWriter output, FastAccessList<object> referenceTracking)
        {
            output.Write(referenceTracking.Length);
            for (var i = 0; i < referenceTracking.Length; i++)
            {
                var data = referenceTracking[i];
                var type = data.GetType();
                var tag = SerializationHelper.TypeToTag[type];
                output.Write(tag);

                if (type == typeof(SqlVariable))
                    ((SqlVariable)data).Serialize(output);
                else if (type == typeof(TableMetadata))
                    ((TableMetadata)data).Serialize(output);
                else
                    SerializationHelper.Serialize(output.BaseStream, data);
            }
        }

        private static FastAccessList<object> DeserializeReferenceTracking(BinaryReader input)
        {
            var referenceTracking = new FastAccessList<object>();

            var nbRef = input.ReadInt32();
            for (var i = 0; i < nbRef; i++)
            {
                var tag = input.ReadInt32();
                var type = SerializationHelper.TagToType[tag];

                if (type == typeof(SqlVariable))
                    referenceTracking.Add(SqlVariable.Deserialize(input));
                else if (type == typeof(TableMetadata))
                    referenceTracking.Add(TableMetadata.Deserialize(input));
                else
                    referenceTracking.Add(SerializationHelper.Deserialize<object>(input.BaseStream));
            }

            return referenceTracking;
        }

        private static void SerializeConnections(BinaryWriter output, List<Connection> conns)
        {
            output.Write(conns.Count);
            foreach (var con in conns)
                con.Serialize(output);
        }

        private static HashSet<Connection> DeserializeConnections(BinaryReader input)
        {
            var lstConns = new List<Connection>();
            var nbCons = input.ReadInt32();
            for (var i = 0; i < nbCons; i++)
                lstConns.Add(Connection.Deserialize(input));
            return new HashSet<Connection>(lstConns);
        }

        /// <summary>
        /// We reset the variables for the API to regenerate them.
        /// </summary>
        /// <param name="planByServer"></param>
        private static void ResetExecutionPlan(Dictionary<string, ExecutionPlan> planByServer)
        {
            foreach (var server in planByServer)
                foreach (var sqlVar in server.Value.Variables)
                    sqlVar.Value = null;
        }
    } 
}
