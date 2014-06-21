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
    public class DerivativeTable
    {
        private Dictionary<Int32, Dictionary<string, Dictionary<string, Dictionary<string, TableTo[]>>>> _dic = new Dictionary<Int32, Dictionary<string, Dictionary<string, Dictionary<string, TableTo[]>>>>();

        public bool Contains(Int32 server, string database, string schema, string tableFrom, TableTo tableTo)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            tableFrom = tableFrom.ToLower();
            tableTo.Database = tableTo.Database.ToLower();
            tableTo.Schema = tableTo.Schema.ToLower();
            tableTo.Table = tableTo.Table.ToLower();

            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema) &&
                _dic[server][database][schema].ContainsKey(tableFrom) &&
                _dic[server][database][schema][tableFrom].Contains(tableTo))
                return true;
            return false;
        }

        public void Add(Int32 server, string database, string schema, string tableFrom, TableTo tableTo)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            tableFrom = tableFrom.ToLower();
            tableTo.Database = tableTo.Database.ToLower();
            tableTo.Schema = tableTo.Schema.ToLower();
            tableTo.Table = tableTo.Table.ToLower();

            if (!_dic.ContainsKey(server))
                _dic.Add(server, new Dictionary<string, Dictionary<string, Dictionary<string, TableTo[]>>>());

            if (!_dic[server].ContainsKey(database))
                _dic[server].Add(database, new Dictionary<string, Dictionary<string, TableTo[]>>());

            if (!_dic[server][database].ContainsKey(schema))
                _dic[server][database].Add(schema, new Dictionary<string, TableTo[]>());

            if (!_dic[server][database][schema].ContainsKey(tableFrom))
                _dic[server][database][schema].Add(tableFrom, new TableTo[] { tableTo });
            else
            {
                if (!_dic[server][database][schema][tableFrom].Contains(tableTo))
                {
                    _dic[server][database][schema][tableFrom] = _dic[server][database][schema][tableFrom].Add(tableTo);
                }
            }
        }

        public bool Remove(Int32 server, string database, string schema, string tableFrom, TableTo tableTo)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            tableFrom = tableFrom.ToLower();
            tableTo.Database = tableTo.Database.ToLower();
            tableTo.Schema = tableTo.Schema.ToLower();
            tableTo.Table = tableTo.Table.ToLower();

            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema) &&
                _dic[server][database][schema].ContainsKey(tableFrom))
            {
                _dic[server][database][schema][tableFrom] = _dic[server][database][schema][tableFrom].Remove(tableTo);

                if (_dic[server][database][schema][tableFrom].Length == 0)
                {
                    _dic[server][database][schema].Remove(tableFrom);
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
                }
                return true;
            }
            return false;
        }

        public TableTo[] this[Int32 server, string database, string schema, string tableFrom]
        {
            get
            {
                database = database.ToLower();
                schema = schema.ToLower();
                tableFrom = tableFrom.ToLower();

                if (_dic.ContainsKey(server) &&
                    _dic[server].ContainsKey(database) &&
                    _dic[server][database].ContainsKey(schema) &&
                    _dic[server][database][schema].ContainsKey(tableFrom))
                {
                    return _dic[server][database][schema][tableFrom];
                }
                return null;
            }
            set
            {
                database = database.ToLower();
                schema = schema.ToLower();
                tableFrom = tableFrom.ToLower();
                for (Int32 i = 0; i < value.Length; i++)
                {
                    value[i].Database = value[i].Database.ToLower();
                    value[i].Schema = value[i].Schema.ToLower();
                    value[i].Table = value[i].Table.ToLower();
                }

                if (!_dic.ContainsKey(server))
                    _dic.Add(server, new Dictionary<string, Dictionary<string, Dictionary<string, TableTo[]>>>());

                if (!_dic[server].ContainsKey(database))
                    _dic[server].Add(database, new Dictionary<string, Dictionary<string, TableTo[]>>());

                if (!_dic[server][database].ContainsKey(schema))
                    _dic[server][database].Add(schema, new Dictionary<string, TableTo[]>());

                if (!_dic[server][database][schema].ContainsKey(tableFrom))
                    _dic[server][database][schema].Add(tableFrom, value);
                else
                    _dic[server][database][schema][tableFrom] = value;
            }
        }

        public class TableTo
        {
            public Int32 ServerId { get; set; }
            public string Database { get; set; }
            public string Schema { get; set; }
            public string Table { get; set; }
            public AccessXml Access { get; set; }
            public bool Cascade { get; set; }

            public override bool Equals(object obj)
            {
                TableTo tableToObj = obj as TableTo;
                if (tableToObj == null)
                    return false;
                else
                    return ServerId.Equals(tableToObj.ServerId) &&
                        Database.Equals(tableToObj.Database) &&
                        Schema.Equals(tableToObj.Schema) &&
                        Table.Equals(tableToObj.Table);
            }

            public override int GetHashCode()
            {
                return Table.GetHashCode();
            }
        }

        public void Serialize(Stream stream)
        {
            Serialize(new BinaryWriter(stream));
        }

        public static DerivativeTable Deserialize(Stream stream)
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

        public static DerivativeTable Deserialize(BinaryReader stream)
        {
            var newDic = new DerivativeTable();

            int nbServers = stream.ReadInt32();
            for (int n = 0; n < nbServers; n++)
            {
                int serverId = stream.ReadInt32();
                newDic._dic.Add(serverId, new Dictionary<string, Dictionary<string, Dictionary<string, TableTo[]>>>());

                int nbDatabases = stream.ReadInt32();
                for (int j = 0; j < nbDatabases; j++)
                {
                    string database = stream.ReadString();
                    newDic._dic[serverId].Add(database, new Dictionary<string, Dictionary<string, TableTo[]>>());

                    int nbSchemas = stream.ReadInt32();
                    for (int k = 0; k < nbSchemas; k++)
                    {
                        string schema = stream.ReadString();
                        newDic._dic[serverId][database].Add(schema, new Dictionary<string, TableTo[]>());

                        int nbTablesFrom = stream.ReadInt32();
                        for (int l = 0; l < nbTablesFrom; l++)
                        {
                            string tableFrom = stream.ReadString();
                            var lstTables = new List<TableTo>();

                            int nbTablesTo = stream.ReadInt32();
                            for (int m = 0; m < nbTablesTo; m++)
                            {
                                lstTables.Add(new TableTo()
                                {
                                    ServerId = stream.ReadInt32(),
                                    Database = stream.ReadString(),
                                    Schema = stream.ReadString(),
                                    Table = stream.ReadString(),
                                    Access = (AccessXml)stream.ReadInt32(),
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
