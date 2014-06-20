using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using DataCloner.Framework;
using DataCloner.Enum;

namespace DataCloner.DataClasse.Configuration
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
            BinaryWriter bw = new BinaryWriter(stream);

            bw.Write(_dic.Count);
            foreach (var server in _dic)
            {
                bw.Write(server.Key);
                bw.Write(server.Value.Count);
                foreach (var database in server.Value)
                {
                    bw.Write(database.Key);
                    bw.Write(database.Value.Count);
                    foreach (var schema in database.Value)
                    {
                        bw.Write(schema.Key);
                        bw.Write(schema.Value.Count);
                        foreach (var tableFrom in schema.Value)
                        {
                            bw.Write(tableFrom.Key);
                            bw.Write(tableFrom.Value.Length);
                            foreach (var tableTo in tableFrom.Value)
                            {
                                bw.Write(tableTo.ServerId);
                                bw.Write(tableTo.Database);
                                bw.Write(tableTo.Schema);
                                bw.Write(tableTo.Table);
                                bw.Write((int)tableTo.Access);
                                bw.Write(tableTo.Cascade);
                            }
                        }
                    }
                }
            }
            bw.Flush();
        }

        public static DerivativeTable Deserialize(Stream stream)
        {
            BinaryReader br = new BinaryReader(stream);
            var newDic = new DerivativeTable();

            int nbServers = br.ReadInt32();
            for (int n = 0; n < nbServers; n++)
            {
                int serverId = br.ReadInt32();
                newDic._dic.Add(serverId, new Dictionary<string, Dictionary<string, Dictionary<string, TableTo[]>>>());

                int nbDatabases = br.ReadInt32();
                for (int j = 0; j < nbDatabases; j++)
                {
                    string database = br.ReadString();
                    newDic._dic[serverId].Add(database, new Dictionary<string, Dictionary<string, TableTo[]>>());

                    int nbSchemas = br.ReadInt32();
                    for (int k = 0; k < nbSchemas; k++)
                    {
                        string schema = br.ReadString();
                        newDic._dic[serverId][database].Add(schema, new Dictionary<string, TableTo[]>());

                        int nbTablesFrom = br.ReadInt32();
                        for (int l = 0; l < nbTablesFrom; l++)
                        {
                            string tableFrom = br.ReadString();
                            var lstTables = new List<TableTo>();

                            int nbTablesTo = br.ReadInt32();
                            for (int m = 0; m < nbTablesTo; m++)
                            {
                                lstTables.Add(new TableTo()
                                {
                                    ServerId = br.ReadInt32(),
                                    Database = br.ReadString(),
                                    Schema = br.ReadString(),
                                    Table = br.ReadString(),
                                    Access = (AccessXml)br.ReadInt32(),
                                    Cascade = br.ReadBoolean()
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
