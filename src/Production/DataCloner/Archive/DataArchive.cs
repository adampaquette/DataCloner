using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using DataCloner.DataClasse;
using DataCloner.DataClasse.Cache;
using DataCloner.Framework;

namespace DataCloner.Archive
{
    internal class DataArchive
    {
        private const int SIZE_OF_INT = sizeof(int);
        private const int BUFFER_SIZE = 32768;

        public string Description { get; set; }
        public List<RowIdentifier> OriginalQueries { get; set; }
        public Cache Cache { get; set; }
        public List<string> Databases { get; set; }

        public DataArchive()
        {
            OriginalQueries = new List<RowIdentifier>();
            Databases = new List<string>();
        }

        public void Save(string path)
        {
            string tempFile = path + ".tmp";
            
            //Création d'une archive non compressée
            SaveToBin(tempFile);

            //Compression
            using (var istream = new FileStream(tempFile, FileMode.Open))
            using (var ostream = new FileStream(path, FileMode.Create))
            using (var lzStream = new LZ4.LZ4Stream(ostream, System.IO.Compression.CompressionMode.Compress))
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

            string tempFile = path + ".tmp";
            DataArchive output;

            //Decompression
            using (var istream = new FileStream(path, FileMode.Open))
            using (var ostream = new FileStream(tempFile, FileMode.Create))
            using (var lzStream = new LZ4.LZ4Stream(istream, System.IO.Compression.CompressionMode.Decompress))
            {
                lzStream.CopyTo(ostream);
            }

            //Chargement de l'archive
            output = LoadFromBin(tempFile, decompressedPath);
            
            //Clean
            if (File.Exists(tempFile))
                File.Delete(tempFile);

            return output;
        }

        private void SaveToBin(string path)
        {
            if (OriginalQueries == null)
                throw new ArgumentNullException("OriginalQueries");
            if (Databases == null)
                throw new ArgumentNullException("Databases");
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
                    string fileName = Path.GetFileName(filePath);
                    FileInfo fi = new FileInfo(filePath);

                    bstream.Write(fileName);
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
                int nbQueries = bstream.ReadInt32();
                for (int i = 0; i<nbQueries; i++)
                    archiveOut.OriginalQueries.Add(bstream.ReadString().DeserializeXml<RowIdentifier>());

                //Cache 
                archiveOut.Cache = Cache.Deserialize(istream);

                //Databases
                int nbDatabases = bstream.ReadInt32();
                for (int i = 0; i < nbDatabases; i++ )
                {
                    string fileName = bstream.ReadString();
                    string filePath = Path.Combine(decompressedPath, fileName);
                    Int64 fileSize = bstream.ReadInt64();
                    if (fileSize > Int32.MaxValue)
                        throw new OverflowException(String.Format("File size for {0} is larger then 32 bit value.", fileName));
                    Int32 fileSize32 = Convert.ToInt32(fileSize);

                    if(!Directory.Exists(decompressedPath))
                        Directory.CreateDirectory(decompressedPath);

                    using (var ostream = new FileStream(filePath, FileMode.Create))
                    {
                        istream.CopyTo(ostream, BUFFER_SIZE, fileSize32);
                    }
                    archiveOut.Databases.Add(filePath);
                }
            }
            return archiveOut;
        }
    }
}
