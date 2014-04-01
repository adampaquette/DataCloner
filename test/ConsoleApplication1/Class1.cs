using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Interface
{
   public interface ITableIdentifier
   {
      string ServerName{get; set;}
      string DatabaseName { get; set; }
      string SchemaName { get; set; }
      string TableName { get; set; }
   }

   public interface IColumnIdentifier : ITableIdentifier
   {
      string ColumnName { get; set; }
   }
}

namespace Class
{
   [Serializable]
   public class TableIdentifier : Interface.ITableIdentifier
   {
      [XmlAttribute]
      public string ServerName { get; set; }
      [XmlAttribute]
      public string DatabaseName { get; set; }
      [XmlAttribute]
      public string SchemaName { get; set; }
      [XmlAttribute]
      public string TableName { get; set; }
   }

   [Serializable]
   public class ColumnIdentifier : TableIdentifier, Interface.IColumnIdentifier
   {
      [XmlAttribute]
      public string ColumnName { get; set; }
   }
}
