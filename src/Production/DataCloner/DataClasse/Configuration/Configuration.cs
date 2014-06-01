using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataCloner.DataClasse.Configuration
{
    class Configuration
    {
        public List<Connection> ConnectionStrings { get; set; }

        public Configuration()
        {
            StaticTable st = new StaticTable();
           // st.Add()
        }
    }
}
