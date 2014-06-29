using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;

using DataCloner.Framework;
using DataCloner.Enum;

namespace DataCloner.DataClasse.Cache
{
    /// <summary>
    /// Contient les tables statiques de la base de données
    /// </summary>
    /// <remarks>Optimisé pour la lecture et non pour l'écriture!</remarks>
    internal sealed class KeyRelationship
    {
        private Dictionary<Int16, Dictionary<string, Dictionary<string, Dictionary<string, object[]>>>> _dic = new Dictionary<Int16, Dictionary<string, Dictionary<string, Dictionary<string, object[]>>>>();

        public bool Contains(Int32 server, string database, string schema, string tableFrom, DerivativeTable tableTo)
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

        public TableDef GetTable(Int16 server, string database, string schema, string table)
        {
            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema))
                return _dic[server][database][schema].Where(t => t.Name == table).FirstOrDefault();
            return null;
        }

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
    }
}
