using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataCloner.Framework;

namespace DataCloner.DataClasse.Configuration
{
    /// <summary>
    /// Contient les tables qui sont le résultat des relations plusieurs à plusieurs.
    /// </summary>
    /// <remarks>Optimisé pour la lecture et non pour l'écriture!</remarks>
    public class ManyToManyRelationshipsTable
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
    }
}
