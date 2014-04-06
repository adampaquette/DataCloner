using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Interface;

namespace DataClasse
{
    public class StaticTableDictionnary : IStaticTableDictionnary
    {
        private IDictionary<ITableIdentifier, bool> _dic = new Dictionary<ITableIdentifier, bool>(new EqualityComparerITableIdentifier());

        #region IDictionary<ITableIdentifier,bool> Membres

        void IDictionary<ITableIdentifier, bool>.Add(ITableIdentifier key, bool value)
        {
            _dic.Add(key, value);
        }

        bool IDictionary<ITableIdentifier, bool>.ContainsKey(ITableIdentifier key)
        {
            return _dic.ContainsKey(key);
        }

        ICollection<ITableIdentifier> IDictionary<ITableIdentifier, bool>.Keys
        {
            get { return _dic.Keys; }
        }

        bool IDictionary<ITableIdentifier, bool>.Remove(ITableIdentifier key)
        {
            return _dic.Remove(key);
        }

        bool IDictionary<ITableIdentifier, bool>.TryGetValue(ITableIdentifier key, out bool value)
        {
            return _dic.TryGetValue(key, out value);
        }

        ICollection<bool> IDictionary<ITableIdentifier, bool>.Values
        {
            get { return _dic.Values; }
        }

        bool IDictionary<ITableIdentifier, bool>.this[ITableIdentifier key]
        {
            get
            {
                return _dic[key];
            }
            set
            {
                _dic[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<ITableIdentifier,bool>> Membres

        void ICollection<KeyValuePair<ITableIdentifier, bool>>.Add(KeyValuePair<ITableIdentifier, bool> item)
        {
            _dic.Add(item);
        }

        void ICollection<KeyValuePair<ITableIdentifier, bool>>.Clear()
        {
            _dic.Clear();
        }

        bool ICollection<KeyValuePair<ITableIdentifier, bool>>.Contains(KeyValuePair<ITableIdentifier, bool> item)
        {
            return _dic.Contains(item);
        }

        void ICollection<KeyValuePair<ITableIdentifier, bool>>.CopyTo(KeyValuePair<ITableIdentifier, bool>[] array, int arrayIndex)
        {
            _dic.CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<ITableIdentifier, bool>>.Count
        {
            get { return _dic.Count; }
        }

        bool ICollection<KeyValuePair<ITableIdentifier, bool>>.IsReadOnly
        {
            get { return _dic.IsReadOnly; }
        }

        bool ICollection<KeyValuePair<ITableIdentifier, bool>>.Remove(KeyValuePair<ITableIdentifier, bool> item)
        {
            return _dic.Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<ITableIdentifier,bool>> Membres

        IEnumerator<KeyValuePair<ITableIdentifier, bool>> IEnumerable<KeyValuePair<ITableIdentifier, bool>>.GetEnumerator()
        {
            return _dic.GetEnumerator();
        }

        #endregion

        #region IEnumerable Membres

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)_dic).GetEnumerator();
        }

        #endregion
    }
}
