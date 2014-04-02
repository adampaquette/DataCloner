using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Interface;

namespace Serialisation
{
   [Serializable]
   public class Config
   {
      [XmlArrayItem("Table")]
      public List<StaticTable> StaticTables { get; set; }
   }

   [Serializable]
   public class StaticTable
   {
      [XmlAttribute]
      public Int16 ConnStringID { get; set; }
      [XmlAttribute]
      public string Database { get; set; }
      [XmlAttribute]
      public string Schema { get; set; }
      [XmlAttribute]
      public string Table { get; set; }
      [XmlAttribute]
      public bool Active { get; set; }
   }
}

namespace Interface
{
   public interface ITableIdentifier
   {
      Int16 ConnStringID { get; set; }
      string DatabaseName { get; set; }
      string SchemaName { get; set; }
      string TableName { get; set; }
   }

   public interface IColumnIdentifier : ITableIdentifier
   {
      string ColumnName { get; set; }
   }

   public interface IStaticTableDictionnary : IDictionary<ITableIdentifier, bool>
   {
   }
}

namespace Class
{
   public class TableIdentifier : ITableIdentifier
   {
      public Int16 ConnStringID { get; set; }
      public string DatabaseName { get; set; }
      public string SchemaName { get; set; }
      public string TableName { get; set; }

      public override string ToString()
      {
         //TODO : EST UTILISÉ PAR GETHASHCODE??
         return ConnStringID + "." + DatabaseName + "." + SchemaName + "." + TableName;
      }
   }

   public class EqualityComparerITableIdentifier : IEqualityComparer<ITableIdentifier>
   {
      #region IEqualityComparer<TableIdentifier> Membres

      bool IEqualityComparer<ITableIdentifier>.Equals(ITableIdentifier x, ITableIdentifier y)
      {
         return x.ConnStringID.Equals(y.ConnStringID) &&
                x.DatabaseName.Equals(y.DatabaseName) &&
                x.SchemaName.Equals(y.SchemaName) &&
                x.TableName.Equals(y.TableName);
      }

      int IEqualityComparer<ITableIdentifier>.GetHashCode(ITableIdentifier obj)
      {
         return (obj.ConnStringID.ToString() + obj.DatabaseName + obj.SchemaName + obj.TableName).GetHashCode();
      }

      #endregion
   }

   public class ColumnIdentifier : TableIdentifier, IColumnIdentifier
   {
      public string ColumnName { get; set; }
   }

   public class EqualityComparerIColumnIdentifier : IEqualityComparer<IColumnIdentifier>
   {
      #region IEqualityComparer<TableIdentifier> Membres

      bool IEqualityComparer<IColumnIdentifier>.Equals(IColumnIdentifier x, IColumnIdentifier y)
      {
         return x.ConnStringID.Equals(y.ConnStringID) &&
                x.DatabaseName.Equals(y.DatabaseName) &&
                x.SchemaName.Equals(y.SchemaName) &&
                x.TableName.Equals(y.TableName) &&
                x.ColumnName.Equals(y.ColumnName);
      }

      int IEqualityComparer<IColumnIdentifier>.GetHashCode(IColumnIdentifier obj)
      {
         return (obj.ConnStringID + obj.DatabaseName + obj.SchemaName + obj.TableName + obj.ColumnName).GetHashCode();
      }

      #endregion
   }

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

   /// ///////////////////////////////////////////////////////////////////////////





   public class MultiDictionary<K1, K2, V>
   {
      private Dictionary<K1, Dictionary<K2, V>> dict =
          new Dictionary<K1, Dictionary<K2, V>>();

      public V this[K1 key1, K2 key2]
      {
         get
         {
            return dict[key1][key2];
         }

         set
         {
            if (!dict.ContainsKey(key1))
            {
               dict[key1] = new Dictionary<K2, V>();
            }
            dict[key1][key2] = value;
         }
      }
   }

   public class MultiKeyDictionary<T1, T2, T3> : Dictionary<T1, Dictionary<T2, T3>>
   {
      new public Dictionary<T2, T3> this[T1 key]
      {
         get
         {
            if (!ContainsKey(key))
               Add(key, new Dictionary<T2, T3>());

            Dictionary<T2, T3> returnObj;
            TryGetValue(key, out returnObj);

            return returnObj;
         }
      }
   }

   //public class MultiKeyDictionary<K1, K2, V> : Dictionary<K1, Dictionary<K2, V>>  {

   //    public V this[K1 key1, K2 key2] {
   //        get {
   //            if (!ContainsKey(key1) || !this[key1].ContainsKey(key2))
   //                throw new ArgumentOutOfRangeException();
   //            return base[key1][key2];
   //        }
   //        set {
   //            if (!ContainsKey(key1))
   //                this[key1] = new Dictionary<K2, V>();
   //            this[key1][key2] = value;
   //        }
   //    }

   //    public void Add(K1 key1, K2 key2, V value) {
   //            if (!ContainsKey(key1))
   //                this[key1] = new Dictionary<K2, V>();
   //            this[key1][key2] = value;
   //    }

   //    public bool ContainsKey(K1 key1, K2 key2) {
   //        return base.ContainsKey(key1) && this[key1].ContainsKey(key2);
   //    }

   //    public new IEnumerable<V> Values {
   //        get {
   //            return from baseDict in base.Values
   //                   from baseKey in baseDict.Keys
   //                   select baseDict[baseKey];
   //        }
   //    } 

   //}



}
