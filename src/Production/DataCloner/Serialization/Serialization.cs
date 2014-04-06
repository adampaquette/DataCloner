using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Serialization
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
      public Int16 ServerID { get; set; }
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
