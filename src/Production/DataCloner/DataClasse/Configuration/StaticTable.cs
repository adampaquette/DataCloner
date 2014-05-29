using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCloner.DataClasse.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>Optimisé pour la lecture et non pour l'écriture!</remarks>
    class StaticTable 
    {
        private Dictionary<string, Dictionary<string, Dictionary<string, string[]>>> _dic = new Dictionary<string, Dictionary<string, Dictionary<string, string[]>>>();

        public void Add(string server, string database, string schema, string table)
        {
            table = table.ToLower();
            
            if (!_dic.ContainsKey(server))
                _dic.Add(server, new Dictionary<string, Dictionary<string, string[]>>());

            if (!_dic[server].ContainsKey(database))
                _dic[server].Add(server, new Dictionary<string, string[]>());

            if (!_dic[server][database].ContainsKey(schema))
                _dic[server][database].Add(schema, new string[] {table});
            else
            {
                if(!_dic[server][database][schema].Contains(table))
                {
                    //TODO : Optimiser!!
                    string[] arrCopy = _dic[server][database][schema];
                    int size = arrCopy.Length;
                    Array.Resize(ref arrCopy, size + 1);
                    arrCopy[size] = table;
                    _dic[server][database][schema] = arrCopy;
                }
            }
        }

        public bool ContainsKey(string server, string database, string schema)
        {
            if (_dic.ContainsKey(server) &&
                _dic[server].ContainsKey(database) &&
                _dic[server][database].ContainsKey(schema))
                return true;
            return false;
        }

        //public bool Remove(string server, string database, string schema)
        //{
        //    if (ContainsKey(server, database, schema))
        //    {

        //        if (!_dic[server][database][schema].Any())
        //        {
        //            _dic[server][database].Remove(schema);
        //            if (!_dic[server][database].Any())
        //            {
        //                _dic[server].Remove(database);
        //                if (!_dic[server].Any())
        //                {
        //                    _dic.Remove(server);
        //                }
        //            }
        //        }
        //        return true;
        //    }
        //    return false;
        //}

        public string this[string server, string database, string schema, string table]
        {
            get
            {
                
            }
            set
            {
                if (!_dic.ContainsKey(server))
                    _dic.Add(server, new Dictionary<string, Dictionary<string, Dictionary<string, string>>>());

                if (!_dic[server].ContainsKey(database))
                    _dic[server].Add(server, new Dictionary<string, Dictionary<string, string>>());

                if (!_dic[server][database].ContainsKey(schema))
                    _dic[server][database].Add(schema, new Dictionary<string, string>());

                if (!_dic[server][database][schema].Contains(table))
                    _dic[server][database][schema].Add(table, value);
            }
        }

        #region ICollection<KeyValuePair<string,IDictionary<string,IDictionary<string,IDictionary<string,int>>>>> Membres

        public void Add(KeyValuePair<string, IDictionary<string, IDictionary<string, IDictionary<string, int>>>> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<string, IDictionary<string, IDictionary<string, IDictionary<string, int>>>> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<string, IDictionary<string, IDictionary<string, IDictionary<string, int>>>>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<string, IDictionary<string, IDictionary<string, IDictionary<string, int>>>> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,IDictionary<string,IDictionary<string,IDictionary<string,int>>>>> Membres

        public IEnumerator<KeyValuePair<string, IDictionary<string, IDictionary<string, IDictionary<string, int>>>>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Membres

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
