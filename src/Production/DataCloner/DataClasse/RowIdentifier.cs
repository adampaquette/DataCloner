using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCloner.DataClasse
{
    public class RowIdentifier : IRowIdentifier
    {
        ITableIdentifier _ti = new TableIdentifier();
        IDictionary<string, object> _columns = new Dictionary<string, object>();

        public ITableIdentifier TableIdentifier 
        {
            get { return _ti; }
            set { _ti = value; }
        }     
   
        public IDictionary<string, object> Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }


        //private IDictionary<IColumnIdentifier, object> _dic  = new Dictionary<IColumnIdentifier, object>();

        //void IDictionary<IColumnIdentifier, object>.Add(IColumnIdentifier key, object value)
        //{
        //    _dic.Add(key, value);
        //}

        //bool IDictionary<IColumnIdentifier, object>.ContainsKey(IColumnIdentifier key)
        //{
        //    return _dic.ContainsKey(key);
        //}

        //ICollection<IColumnIdentifier> IDictionary<IColumnIdentifier, object>.Keys
        //{
        //    get { return _dic.Keys; }
        //}

        //bool IDictionary<IColumnIdentifier, object>.Remove(IColumnIdentifier key)
        //{
        //    return _dic.Remove(key);
        //}

        //bool IDictionary<IColumnIdentifier, object>.TryGetValue(IColumnIdentifier key, out object value)
        //{
        //    return _dic.TryGetValue(key, out value);
        //}

        //ICollection<object> IDictionary<IColumnIdentifier, object>.Values
        //{
        //    get { return _dic.Values; }
        //}

        //object IDictionary<IColumnIdentifier, object>.this[IColumnIdentifier key]
        //{
        //    get
        //    {
        //        return _dic[key];
        //    }
        //    set
        //    {
        //        _dic[key] = value;
        //    }
        //}

        //void ICollection<KeyValuePair<IColumnIdentifier, object>>.Add(KeyValuePair<IColumnIdentifier, object> item)
        //{
        //    _dic.Add(item);
        //}

        //void ICollection<KeyValuePair<IColumnIdentifier, object>>.Clear()
        //{
        //    _dic.Clear();
        //}

        //bool ICollection<KeyValuePair<IColumnIdentifier, object>>.Contains(KeyValuePair<IColumnIdentifier, object> item)
        //{
        //   return _dic.Contains(item);
        //}

        //void ICollection<KeyValuePair<IColumnIdentifier, object>>.CopyTo(KeyValuePair<IColumnIdentifier, object>[] array, int arrayIndex)
        //{
        //    _dic.CopyTo(array, arrayIndex);
        //}

        //int ICollection<KeyValuePair<IColumnIdentifier, object>>.Count
        //{
        //    get { return _dic.Count; }
        //}

        //bool ICollection<KeyValuePair<IColumnIdentifier, object>>.IsReadOnly
        //{
        //    get { return _dic.IsReadOnly; }
        //}

        //bool ICollection<KeyValuePair<IColumnIdentifier, object>>.Remove(KeyValuePair<IColumnIdentifier, object> item)
        //{
        //    return _dic.Remove(item);
        //}

        //IEnumerator<KeyValuePair<IColumnIdentifier, object>> IEnumerable<KeyValuePair<IColumnIdentifier, object>>.GetEnumerator()
        //{
        //    return _dic.GetEnumerator();
        //}

        //System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //{
        //    return ((System.Collections.IEnumerable)_dic).GetEnumerator();
        //}
    }
}
