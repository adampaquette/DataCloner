using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using DataCloner.Framework;
using DataCloner.Enum;

namespace DataCloner.DataClasse.Cache
{
    /// <summary>
    /// Contient les tables statiques de la base de données
    /// </summary>
    /// <remarks>Optimisé pour la lecture et non pour l'écriture!</remarks>
    internal sealed class CachedTables
    {
        private Dictionary<Int32, Dictionary<string, Dictionary<string, TableDef[]>>> _dic = new Dictionary<Int32, Dictionary<string, Dictionary<string, TableDef[]>>>();

        //public bool Contains(Int32 server, string database, string schema, string tableFrom, DerivativeTable tableTo)
        //{
        //    database = database.ToLower();
        //    schema = schema.ToLower();
        //    tableFrom = tableFrom.ToLower();
        //    tableTo.Database = tableTo.Database.ToLower();
        //    tableTo.Schema = tableTo.Schema.ToLower();
        //    tableTo.Table = tableTo.Table.ToLower();

        //    if (_dic.ContainsKey(server) &&
        //        _dic[server].ContainsKey(database) &&
        //        _dic[server][database].ContainsKey(schema) &&
        //        _dic[server][database][schema].ContainsKey(tableFrom) &&
        //        _dic[server][database][schema][tableFrom].Contains(tableTo))
        //        return true;
        //    return false;
        //}

        public void Add(Int32 server, string database, string schema, TableDef table)
        {
            database = database.ToLower();
            schema = schema.ToLower();

            if (!_dic.ContainsKey(server))
                _dic.Add(server, new Dictionary<string, Dictionary<string, TableDef[]>>());

            if (!_dic[server].ContainsKey(database))
                _dic[server].Add(database, new Dictionary<string, TableDef[]>());

            if (!_dic[server][database].ContainsKey(schema))
                _dic[server][database].Add(schema, new TableDef[] { table });
            else
            {
                if (!_dic[server][database][schema].Contains(table))
                    _dic[server][database][schema] = _dic[server][database][schema].Add(table);
            }
        }

        public bool Remove(Int32 server, string database, string schema, TableDef table)
        {
            database = database.ToLower();
            schema = schema.ToLower();

            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema) &&
                _dic[server][database][schema].Contains(table))
            {
                _dic[server][database][schema] = _dic[server][database][schema].Remove(table);

                if (_dic[server][database][schema].Length == 0)
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

        public TableDef[] this[Int32 server, string database, string schema]
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

                if (!_dic.ContainsKey(server))
                    _dic.Add(server, new Dictionary<string, Dictionary<string, TableDef[]>>());

                if (!_dic[server].ContainsKey(database))
                    _dic[server].Add(database, new Dictionary<string,  TableDef[]>());

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

        public static CachedTables Deserialize(Stream stream)
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
                        stream.Write(schema.Value.Count);
                        foreach (var tableFrom in schema.Value)
                        {
                            stream.Write(tableFrom.Key);
                            stream.Write(tableFrom.Value.Length);
                            foreach (var tableTo in tableFrom.Value)
                            {
                                stream.Write(tableTo.ServerId);
                                stream.Write(tableTo.Database);
                                stream.Write(tableTo.Schema);
                                stream.Write(tableTo.Table);
                                stream.Write((int)tableTo.Access);
                                stream.Write(tableTo.Cascade);
                            }
                        }
                    }
                }
            }
        }

        public static CachedTables Deserialize(BinaryReader stream)
        {
            var newDic = new CachedTables();

            int nbServers = stream.ReadInt32();
            for (int n = 0; n < nbServers; n++)
            {
                int serverId = stream.ReadInt32();
                newDic._dic.Add(serverId, new Dictionary<string, Dictionary<string, Dictionary<string, DerivativeTable[]>>>());

                int nbDatabases = stream.ReadInt32();
                for (int j = 0; j < nbDatabases; j++)
                {
                    string database = stream.ReadString();
                    newDic._dic[serverId].Add(database, new Dictionary<string, Dictionary<string, DerivativeTable[]>>());

                    int nbSchemas = stream.ReadInt32();
                    for (int k = 0; k < nbSchemas; k++)
                    {
                        string schema = stream.ReadString();
                        newDic._dic[serverId][database].Add(schema, new Dictionary<string, DerivativeTable[]>());

                        int nbTablesFrom = stream.ReadInt32();
                        for (int l = 0; l < nbTablesFrom; l++)
                        {
                            string tableFrom = stream.ReadString();
                            var lstTables = new List<DerivativeTable>();

                            int nbTablesTo = stream.ReadInt32();
                            for (int m = 0; m < nbTablesTo; m++)
                            {
                                lstTables.Add(new DerivativeTable()
                                {
                                    ServerId = stream.ReadInt32(),
                                    Database = stream.ReadString(),
                                    Schema = stream.ReadString(),
                                    Table = stream.ReadString(),
                                    Access = (DerivativeTableAccess)stream.ReadInt32(),
                                    Cascade = stream.ReadBoolean()
                                });
                            }
                            newDic._dic[serverId][database][schema].Add(tableFrom, lstTables.ToArray());
                        }
                    }
                }
            }
            return newDic;
        }
    }
}
