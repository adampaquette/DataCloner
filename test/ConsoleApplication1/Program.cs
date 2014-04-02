using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            var ti = new Class.TableIdentifier();
            var ci = new Class.ColumnIdentifier();

            ci.ServerName = "127.0.0.1";
            ci.DatabaseName = "db";
            ci.SchemaName = "dbo";
            ci.TableName = "table";
            ci.ColumnName = "tbl";

            ti = ci;

            Console.WriteLine(SerizlizeXML(ci));
            Console.WriteLine();
            Console.WriteLine(SerizlizeXML(ti));

            Console.ReadKey();
        }

        public static string SerizlizeXML<T>(T obj)
        {
            var xs = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            var sw = new System.IO.StringWriter();
            var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
            ns.Add("", "");

            xs.Serialize(sw, obj, ns);
            return sw.ToString();
        }

    }

}
