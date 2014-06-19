﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        private Dictionary<Int32, Dictionary<string, Dictionary<string, TableTo[]>>> _dic = new Dictionary<Int32, Dictionary<string, Dictionary<string, TableTo[]>>>();

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
    }
}
