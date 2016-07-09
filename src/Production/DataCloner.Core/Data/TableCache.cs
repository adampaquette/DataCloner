using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCloner.DataClasse
{
   public class TableCache : ITableCache
   {
      public string SelectCommand { get; set; }
      public string UpdateCommand { get; set; }
      public string DeleteCommand { get; set; }
      public string InsertCommand { get; set; }
   }
}
