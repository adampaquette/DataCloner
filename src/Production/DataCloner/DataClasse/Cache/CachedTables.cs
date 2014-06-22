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
        private Dictionary<Int16, Dictionary<string, Dictionary<string, TableDef[]>>> _dic = new Dictionary<Int16, Dictionary<string, Dictionary<string, TableDef[]>>>();

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

        public void Add(Int16 server, string database, string schema, TableDef table)
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

        public bool Remove(Int16 server, string database, string schema, TableDef table)
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

        public TableDef[] this[Int16 server, string database, string schema]
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
                    _dic[server].Add(database, new Dictionary<string, TableDef[]>());

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
            int nbRows;
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
                        nbRows = schema.Value.Length;
                        stream.Write(schema.Key);
                        stream.Write(nbRows);
                        for (int i = 0; i < nbRows; i++)
                            schema.Value[i].Serialize(stream);
                    }
                }
            }
        }

        public static CachedTables Deserialize(BinaryReader stream)
        {
            var cTables = new CachedTables();

            int nbServers = stream.ReadInt32();
            for (int n = 0; n < nbServers; n++)
            {
                Int16 serverId = stream.ReadInt16();
                cTables._dic.Add(serverId, new Dictionary<string, Dictionary<string, TableDef[]>>());

                int nbDatabases = stream.ReadInt32();
                for (int j = 0; j < nbDatabases; j++)
                {
                    string database = stream.ReadString();
                    cTables._dic[serverId].Add(database, new Dictionary<string, TableDef[]>());

                    int nbSchemas = stream.ReadInt32();
                    for (int k = 0; k < nbSchemas; k++)
                    {
                        var lstTables = new List<TableDef>();
                        string schema = stream.ReadString();                        

                        int nbTablesFrom = stream.ReadInt32();
                        for (int l = 0; l < nbTablesFrom; l++)
                            lstTables.Add(TableDef.Deserialize(stream));

                        cTables._dic[serverId][database].Add(schema, lstTables.ToArray());
                    }
                }
            }
            return cTables;
        }
    }
}
