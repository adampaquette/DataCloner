using DataCloner.Configuration;
using DataCloner.Framework;
using DataCloner.Internal;
using DataCloner.Metadata;
using LZ4;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace DataCloner.Archive
{
    public class DataArchive
    {
        private const int SizeOfInt = sizeof(int);
        private const int BufferSize = 32768;

        private MetadataContainer _metadata;
        private ExecutionPlanByServer _executionPlanByServer;
        private ProjectContainer _project;

        public int FormatVersion { get { return 1; } }
        public string Description { get; set; }

        internal DataArchive(MetadataContainer metadataCtn,
            ExecutionPlanByServer executionPlanByServer,
            ProjectContainer project)
        {
            _metadata = metadataCtn;
            _executionPlanByServer = executionPlanByServer;
            _project = project;
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
            _metadata.Serialize(refStream, referenceTracking);

            //Compression
            using (var lzStream = new LZ4Stream(ostream, CompressionMode.Compress))
            using (var bstream = new BinaryWriter(lzStream, Encoding.UTF8, true))
            {
                bstream.Write(FormatVersion);
                bstream.Write(Description ?? "");
                SerializeReferenceTracking(bstream, referenceTracking);
                bstream.Write(_project.SerializeXml());
                refStream.WriteTo(lzStream);
            }
        }

        public static DataArchive Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            int formatVersion;
            string description;
            ProjectContainer project;
            MetadataContainer metadataCtn;
            ExecutionPlanByServer executionPlanByServer;
            FastAccessList<object> referenceTracking;

            using (var istream = new FileStream(path, FileMode.Open))
            using (var lzstream = new LZ4Stream(istream, CompressionMode.Decompress))
            using (var bstream = new BinaryReader(lzstream, Encoding.UTF8, true))
            {
                formatVersion = bstream.ReadInt32();
                description = bstream.ReadString();
                referenceTracking = DeserializeReferenceTracking(bstream);
                project = bstream.ReadString().DeserializeXml<ProjectContainer>();
                executionPlanByServer = ExecutionPlanByServer.Deserialize(bstream, referenceTracking);
                metadataCtn = MetadataContainer.Deserialize(bstream, referenceTracking);
            }

            return new DataArchive(metadataCtn, executionPlanByServer, project)
            {
                Description = description
            };
        }
        private void SerializeReferenceTracking(BinaryWriter output, FastAccessList<object> referenceTracking)
        {
            var bf = SerializationHelper.DefaultFormatter;

            output.Write(referenceTracking.Length);
            for (int i = 0; i < referenceTracking.Length; i++)
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
                    bf.Serialize(output.BaseStream, data);
            }
        }

        private static FastAccessList<object> DeserializeReferenceTracking(BinaryReader input)
        {
            var bf = SerializationHelper.DefaultFormatter;
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
                    referenceTracking.Add(bf.Deserialize(input.BaseStream));
            }

            return referenceTracking;
        }
    } 

    public static class DataArchiveExtension
    {
        public static DataArchive ToDataArchive(this Cloner cloner)
        {
            var project = new ProjectContainer();

            var destinationSrv = (from server in cloner.ExecutionPlanByServer
                                  from insertStep in server.Value.InsertSteps
                                  select insertStep.DestinationTable.ServerId).Distinct();

            foreach (var srv in destinationSrv)
            {
                var cs = cloner.MetadataCtn.ConnectionStrings.First(c => c.Id == srv);
                project.ConnectionStrings.Add(new Connection
                {
                    Id = cs.Id,
                    ConnectionString = cs.ConnectionString,
                    ProviderName = cs.ProviderName
                });
            }

            return new DataArchive(cloner.MetadataCtn, cloner.ExecutionPlanByServer, project);
        }

        public static Cloner ToCloner(this DataArchive dataArchive)
        {
            //TODO: Créer une nouvelle classe permettant de charger un ExecutionPlanByServer et de l'exécuter avec une map.
            //Cloner va s'appeler ExecutionPlanBuilder.
            //Une nouvelle classe s'appelera Cloner et prendra en charge l'enregistrement dans la BD à partir d'un ExecutionPlanByServer.
            //La classe DataArchive permettra de retourner les maps par défaut pour les envoyer au Cloner.
            return null;
        }
    }
}
