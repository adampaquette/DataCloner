using DataCloner.Configuration;
using DataCloner.Framework;
using DataCloner.Internal;
using DataCloner.Metadata;
using LZ4;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace DataCloner.Archive
{
    public class DataArchive
    {
        private const int SizeOfInt = sizeof(int);
        private const int BufferSize = 32768;

        private MetadataContainer _metadata;
        private ExecutionPlanByServer _executionPlanByServer;
        private ProjectContainer _project;

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
            var referenceValue = new DecompresibleList();

            var epStream = new MemoryStream();
            _executionPlanByServer.Serialize(epStream, referenceValue);

            var ms = new MemoryStream();
            var bf = new BinaryFormatter();
            referenceValue.Trim();
            bf.Serialize(ms, referenceValue);

            //Compression
            using (var lzStream = new LZ4Stream(ostream, CompressionMode.Compress))
            using (var bstream = new BinaryWriter(lzStream))
            {
                
                bstream.Write(Description ?? "");
                bstream.Write(_project.SerializeXml());
                _metadata.Serialize(lzStream);
                epStream.CopyTo(lzStream);
            }
        }

        public static DataArchive Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            
            string description;
            ProjectContainer project;
            MetadataContainer metadataCtn;
            ExecutionPlanByServer executionPlanByServer;

            using (var istream = new FileStream(path, FileMode.Open))
            using (var lzstream = new LZ4Stream(istream, CompressionMode.Decompress))
            using (var bstream = new BinaryReader(lzstream))
            {
                description = bstream.ReadString();
                project = bstream.ReadString().DeserializeXml<ProjectContainer>();

                
                metadataCtn = MetadataContainer.Deserialize(bstream);
            }

            return new DataArchive(metadataCtn, null, project);
        }

        private void SaveToBin(string path)
        {


            //using (var ostream = File.Create(path))
            //using (var bstream = new BinaryWriter(ostream))
            //{
            //    //Archive description
            //    bstream.Write(Description);

            //    //Queries
            //    bstream.Write(OriginalQueries.Count());
            //    foreach (var ri in OriginalQueries)
            //        bstream.Write(ri.SerializeXml());

            //    //Cache 
            //    Cache.Serialize(ostream);

            //    //Databases
            //    bstream.Write(Databases.Count());
            //    foreach (var filePath in Databases)
            //    {
            //        var fileName = Path.GetFileName(filePath);
            //        var fi = new FileInfo(filePath);

            //        bstream.Write(fileName ?? "");
            //        bstream.Write(fi.Length);
            //        using (var istream = new FileStream(filePath, FileMode.Open))
            //        {
            //            istream.CopyTo(ostream);
            //        }
            //    }
            //}
        }

        private static DataArchive LoadFromBin(string path, string decompressedPath)
        {
            //var archiveOut = new DataArchive();

            //using (var istream = new FileStream(path, FileMode.Open))
            //using (var bstream = new BinaryReader(istream))
            //{
            //    //Archive description
            //    archiveOut.Description = bstream.ReadString();

            //    //Queries
            //    var nbQueries = bstream.ReadInt32();
            //    for (var i = 0; i < nbQueries; i++)
            //        archiveOut.OriginalQueries.Add(bstream.ReadString().DeserializeXml<RowIdentifier>());

            //    //Cache 
            //    archiveOut.Cache = MetadataContainer.Deserialize(istream);

            //    //Databases
            //    var nbDatabases = bstream.ReadInt32();
            //    for (var i = 0; i < nbDatabases; i++)
            //    {
            //        var fileName = bstream.ReadString();
            //        var filePath = Path.Combine(decompressedPath, fileName);
            //        var fileSize = bstream.ReadInt64();
            //        if (fileSize > Int32.MaxValue)
            //            throw new OverflowException(string.Format("File size for {0} is larger then 32 bit value.", fileName));
            //        var fileSize32 = Convert.ToInt32(fileSize);

            //        if (!Directory.Exists(decompressedPath))
            //            Directory.CreateDirectory(decompressedPath);

            //        using (var ostream = new FileStream(filePath, FileMode.Create))
            //        {
            //            istream.CopyTo(ostream, BufferSize, fileSize32);
            //        }
            //        archiveOut.Databases.Add(filePath);
            //    }
            //}
            return null;//archiveOut;
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
    }
}
