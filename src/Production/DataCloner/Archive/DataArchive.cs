using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using DataCloner.Internal;
using DataCloner.Metadata;
using DataCloner.Framework;
using LZ4;

namespace DataCloner.Archive
{
    internal class DataArchive
    {
        private const int SizeOfInt = sizeof(int);
        private const int BufferSize = 32768;

        public string Description { get; set; }
        public List<RowIdentifier> OriginalQueries { get; set; }
        public MetadataContainer Cache { get; set; }
        public List<string> Databases { get; set; }

        public DataArchive()
        {
            OriginalQueries = new List<RowIdentifier>();
            Databases = new List<string>();
        }

        public void Save(string path)
        {
            var tempFile = path + ".tmp";

            //Création d'une archive non compressée
            SaveToBin(tempFile);

            //Compression
            using (var istream = new FileStream(tempFile, FileMode.Open))
            using (var ostream = new FileStream(path, FileMode.Create))
            using (var lzStream = new LZ4Stream(ostream, CompressionMode.Compress))
            {
                istream.CopyTo(lzStream);
            }

            //Clean
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }

        public static DataArchive Load(string path, string decompressedPath)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var tempFile = path + ".tmp";

            //Decompression
            using (var istream = new FileStream(path, FileMode.Open))
            using (var ostream = new FileStream(tempFile, FileMode.Create))
            using (var lzStream = new LZ4Stream(istream, CompressionMode.Decompress))
            {
                lzStream.CopyTo(ostream);
            }

            //Chargement de l'archive
            var output = LoadFromBin(tempFile, decompressedPath);

            //Clean
            if (File.Exists(tempFile))
                File.Delete(tempFile);

            return output;
        }

        private void SaveToBin(string path)
        {
            if (OriginalQueries == null)
                throw new NullReferenceException("OriginalQueries");
            if (Databases == null)
                throw new NullReferenceException("Databases");
            foreach (var file in Databases)
                if (!File.Exists(file))
                    throw new FileNotFoundException(file);

            using (var ostream = File.Create(path))
            using (var bstream = new BinaryWriter(ostream))
            {
                //Archive description
                bstream.Write(Description);

                //Queries
                bstream.Write(OriginalQueries.Count());
                foreach (var ri in OriginalQueries)
                    bstream.Write(ri.SerializeXml());

                //Cache 
                Cache.Serialize(ostream);

                //Databases
                bstream.Write(Databases.Count());
                foreach (var filePath in Databases)
                {
                    var fileName = Path.GetFileName(filePath);
                    var fi = new FileInfo(filePath);

                    bstream.Write(fileName ?? "");
                    bstream.Write(fi.Length);
                    using (var istream = new FileStream(filePath, FileMode.Open))
                    {
                        istream.CopyTo(ostream);
                    }
                }
            }
        }

        private static DataArchive LoadFromBin(string path, string decompressedPath)
        {
            var archiveOut = new DataArchive();

            using (var istream = new FileStream(path, FileMode.Open))
            using (var bstream = new BinaryReader(istream))
            {
                //Archive description
                archiveOut.Description = bstream.ReadString();

                //Queries
                var nbQueries = bstream.ReadInt32();
                for (var i = 0; i < nbQueries; i++)
                    archiveOut.OriginalQueries.Add(bstream.ReadString().DeserializeXml<RowIdentifier>());

                //Cache 
                archiveOut.Cache = MetadataContainer.Deserialize(istream);

                //Databases
                var nbDatabases = bstream.ReadInt32();
                for (var i = 0; i < nbDatabases; i++)
                {
                    var fileName = bstream.ReadString();
                    var filePath = Path.Combine(decompressedPath, fileName);
                    var fileSize = bstream.ReadInt64();
                    if (fileSize > Int32.MaxValue)
                        throw new OverflowException(string.Format("File size for {0} is larger then 32 bit value.", fileName));
                    var fileSize32 = Convert.ToInt32(fileSize);

                    if (!Directory.Exists(decompressedPath))
                        Directory.CreateDirectory(decompressedPath);

                    using (var ostream = new FileStream(filePath, FileMode.Create))
                    {
                        istream.CopyTo(ostream, BufferSize, fileSize32);
                    }
                    archiveOut.Databases.Add(filePath);
                }
            }
            return archiveOut;
        }
    }
}
