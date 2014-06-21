using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using DataCloner.Framework;

namespace DataCloner.DataClasse.Cache
{
    /// <summary>
    /// Contient les tables statiques de la base de données
    /// </summary>
    /// <remarks>Optimisé pour la lecture et non pour l'écriture!</remarks>
    public class StaticTable
    {
        private Dictionary<Int32, Dictionary<string, Dictionary<string, string[]>>> _dic = new Dictionary<Int32, Dictionary<string, Dictionary<string, string[]>>>();

        public bool Contains(Int32 server, string database, string schema, string table)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            table = table.ToLower();

            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema) &&
                _dic[server][database][schema].Contains(table))
                return true;
            return false;
        }

        public void Add(Int32 server, string database, string schema, string table)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            table = table.ToLower();

            if (!_dic.ContainsKey(server))
                _dic.Add(server, new Dictionary<string, Dictionary<string, string[]>>());

            if (!_dic[server].ContainsKey(database))
                _dic[server].Add(database, new Dictionary<string, string[]>());

            if (!_dic[server][database].ContainsKey(schema))
                _dic[server][database].Add(schema, new string[] { table });
            else
            {
                if (!_dic[server][database][schema].Contains(table))
                {
                    _dic[server][database][schema] = _dic[server][database][schema].Add(table);
                }
            }
        }

        public bool Remove(Int32 server, string database, string schema, string table)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            table = table.ToLower();

            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema))
            {
                _dic[server][database][schema] = _dic[server][database][schema].Remove(table);

                if (!_dic[server][database][schema].Any())
                {
                    _dic[server][database].Remove(schema);
                    if (!_dic[server][database].Any())
                    {
                        _dic[server].Remove(database);
                        if (!_dic[server].Any())
                        {
                            _dic.Remove(server);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public string[] this[Int32 server, string database, string schema]
        {
            get
            {
                database = database.ToLower();
                schema = schema.ToLower();

                if (_dic.ContainsKey(server) &&
                    _dic[server].ContainsKey(database) &&
                    _dic[server][database].ContainsKey(schema))
                {
                    return _dic[server][database][schema];
                }
                return null;
            }
            set
            {
                database = database.ToLower();
                schema = schema.ToLower();
                for (Int32 i = 0; i < value.Length; i++)
                    value[i] = value[i].ToLower();

                if (!_dic.ContainsKey(server))
                    _dic.Add(server, new Dictionary<string, Dictionary<string, string[]>>());

                if (!_dic[server].ContainsKey(database))
                    _dic[server].Add(database, new Dictionary<string, string[]>());

                if (!_dic[server][database].ContainsKey(schema))
                    _dic[server][database].Add(schema, value);
                else
                    _dic[server][database][schema] = value;
            }
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static StaticTable Deserialize(Stream stream)
        {
            return Deserialize(new BinaryReader(stream));
        }

        public void Serialize(BinaryWriter stream)
        {
            stream.Write(_dic.Count);
            foreach (var server in _dic)
            {
                stream.Write(server.Key);
                stream.Write(server.Value.Count);
                foreach (var database in server.Value)
                {
                    stream.Write(database.Key);
                    stream.Write(database.Value.Count);
                    foreach (var schema in database.Value)
                    {
                        stream.Write(schema.Key);
                        stream.Write(schema.Value.Length);
                        for (int i = 0; i < schema.Value.Length; i++)
                        { 
                            stream.Write(schema.Value[i]);                      
                        }       
                    }
                }
            }
        }

        public static StaticTable Deserialize(BinaryReader stream)
        {
            var newDic = new StaticTable();

            int nbServers = stream.ReadInt32();
            for (int n = 0; n < nbServers; n++)
            {
                int serverId = stream.ReadInt32();
                newDic._dic.Add(serverId, new Dictionary<string, Dictionary<string, String[]>>());

                int nbDatabases = stream.ReadInt32();
                for (int j = 0; j < nbDatabases; j++)
                {
                    string database = stream.ReadString();
                    newDic._dic[serverId].Add(database, new Dictionary<string, String[]>());

                    int nbSchemas = stream.ReadInt32();
                    for (int k = 0; k < nbSchemas; k++)
                    {
                        string schema = stream.ReadString();               
                        var lstTables = new List<string>();

                        int nbTables = stream.ReadInt32();
                        for (int l = 0; l < nbTables; l++)
                        {
                            lstTables.Add(stream.ReadString());
                        }
                        newDic._dic[serverId][database].Add(schema, lstTables.ToArray());
                    }
                }
            }
            return newDic;
        }
    }
}
