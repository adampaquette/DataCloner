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
        internal Dictionary<Int32, Dictionary<string, Dictionary<string, TableTo[]>>> _dic = new Dictionary<Int32, Dictionary<string, Dictionary<string, TableTo[]>>>();

        public bool Contains(Int32 server, string database, string schema, TableTo table)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            table.Database = table.Database.ToLower();
            table.Schema = table.Schema.ToLower();
            table.Table = table.Table.ToLower();

            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema) &&
                _dic[server][database][schema].Contains(table))
                return true;
            return false;
        }

        public void Add(Int32 server, string database, string schema, TableTo table)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            table.Database = table.Database.ToLower();
            table.Schema = table.Schema.ToLower();
            table.Table = table.Table.ToLower();

            if (!_dic.ContainsKey(server))
                _dic.Add(server, new Dictionary<string, Dictionary<string, TableTo[]>>());

            if (!_dic[server].ContainsKey(database))
                _dic[server].Add(database, new Dictionary<string, TableTo[]>());

            if (!_dic[server][database].ContainsKey(schema))
                _dic[server][database].Add(schema, new TableTo[] { table });
            else
            {
                if (!_dic[server][database][schema].Contains(table))
                {
                    _dic[server][database][schema] = _dic[server][database][schema].Add(table);
                }
            }
        }

        public bool Remove(Int32 server, string database, string schema, TableTo table)
        {
            database = database.ToLower();
            schema = schema.ToLower();
            table.Database = table.Database.ToLower();
            table.Schema = table.Schema.ToLower();
            table.Table = table.Table.ToLower();

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

        public TableTo[] this[Int32 server, string database, string schema]
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
                {
                    value[i].Database = value[i].Database.ToLower();
                    value[i].Schema = value[i].Schema.ToLower();
                    value[i].Table = value[i].Table.ToLower();
                }

                if (!_dic.ContainsKey(server))
                    _dic.Add(server, new Dictionary<string, Dictionary<string, TableTo[]>>());

                if (!_dic[server].ContainsKey(database))
                    _dic[server].Add(database, new Dictionary<string, TableTo[]>());

                if (!_dic[server][database].ContainsKey(schema))
                    _dic[server][database].Add(schema, value);
                else
                    _dic[server][database][schema] = value;
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
                        bw.Write(schema.Value.Length);
                        foreach (var table in schema.Value)
                        {
                            bw.Write(table.ServerId);
                            bw.Write(table.Database);
                            bw.Write(table.Schema);
                            bw.Write(table.Table);
                            bw.Write((int)table.Access);
                            bw.Write(table.Cascade);
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
                newDic._dic.Add(serverId, new Dictionary<string, Dictionary<string, TableTo[]>>());

                int nbDatabases = br.ReadInt32();
                for (int j = 0; j < nbDatabases; j++)
                {
                    string database = br.ReadString();
                    newDic._dic[serverId].Add(database, new Dictionary<string, TableTo[]>());

                    int nbSchemas = br.ReadInt32();
                    for (int k = 0; k < nbSchemas; k++)
                    {
                        string schema = br.ReadString();
                        var lstTables = new List<TableTo>();

                        int nbTables = br.ReadInt32();
                        for(int l =0; l< nbTables; l++)
                        {
                            lstTables.Add(new TableTo()
                            { 
                                ServerId =  br.ReadInt32(),
                                Database = br.ReadString(),
                                Schema = br.ReadString(), 
                                Table = br.ReadString(),
                                Access = (AccessXml)br.ReadInt32(),
                                Cascade = br.ReadBoolean()
                            });
                        }
                        
                        newDic._dic[serverId][database].Add(schema, lstTables.ToArray());
                    }
                }
            }
            return newDic;
        }
    }
}
