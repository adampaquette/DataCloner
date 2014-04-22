using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCloner.DataClasse
{
   public class TableCacheDictionnary : ITableCacheDictionnary
   {
      private IDictionary<ITableIdentifier, ITableCache> _dic = new Dictionary<ITableIdentifier, ITableCache>(new EqualityComparerITableIdentifier());

      #region IDictionary<ITableIdentifier,ITableCache> Membres

      public void Add(ITableIdentifier key, ITableCache value)
      {
         _dic.Add(key, value);
      }

      public bool ContainsKey(ITableIdentifier key)
      {
         return _dic.ContainsKey(key);
      }

      public ICollection<ITableIdentifier> Keys
      {
         get { return _dic.Keys; }
      }

      public bool Remove(ITableIdentifier key)
      {
         return _dic.Remove(key);
      }

      public bool TryGetValue(ITableIdentifier key, out ITableCache value)
      {
         return _dic.TryGetValue(key, out value);
      }

      public ICollection<ITableCache> Values
      {
         get { return _dic.Values; }
      }

      public ITableCache this[ITableIdentifier key]
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

      #region ICollection<KeyValuePair<ITableIdentifier,ITableCache>> Membres

      public void Add(KeyValuePair<ITableIdentifier, ITableCache> item)
      {
         _dic.Add(item);
      }

      public void Clear()
      {
         _dic.Clear();
      }

      public bool Contains(KeyValuePair<ITableIdentifier, ITableCache> item)
      {
         return _dic.Contains(item);
      }

      public void CopyTo(KeyValuePair<ITableIdentifier, ITableCache>[] array, int arrayIndex)
      {
         _dic.CopyTo(array, arrayIndex);
      }

      public int Count
      {
         get { return _dic.Count; }
      }

      public bool IsReadOnly
      {
         get { return _dic.IsReadOnly; }
      }

      public bool Remove(KeyValuePair<ITableIdentifier, ITableCache> item)
      {
         return _dic.Remove(item);
      }

      #endregion

      #region IEnumerable<KeyValuePair<ITableIdentifier,ITableCache>> Membres

      public IEnumerator<KeyValuePair<ITableIdentifier, ITableCache>> GetEnumerator()
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
