//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.ComponentModel;

//using DataCloner.DataClasse;
//using DataCloner.DataClasse.Cache;
//using DataCloner.DataClasse.Configuration;

//namespace DataCloner.DataClasse.Helper
//{
//    internal sealed class ConfigConverter : TypeConverter
//    {

//        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
//        {
//            if (sourceType == typeof(ConfigurationXml))
//                return true;
//            return false;
//        }

//        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
//        {
//            if (destinationType == typeof(ConfigurationXml))
//                return true;
//            return false;
//        }

//        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
//        {
//            if (value is ConfigurationXml)
//            {
//                ConfigurationXml configXml = (ConfigurationXml)value;
//                Cache.Configuration output = new Cache.Configuration();

//                foreach (var cs in configXml.ConnectionStrings)
//                {
//                    output.ConnectionStrings.Add(new Connection(cs.Id, cs.ProviderName, cs.ConnectionString, cs.SameConfigAsId));
//                }

//                foreach (var s in configXml.DerivativeTableAccess.Servers)
//                {
//                    foreach (var d in s.Databases)
//                    {
//                        foreach (var sc in d.Schemas)
//                        {
//                            foreach (var t in sc.Tables)
//                            { 
//                                output.DerivativeTables.Add(s.Id, d.Name, sc.Name, t.)
//                            }
//                        }
//                    }
//                }
//            }
//            return null;
//        }

//        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
//        {
//            return base.ConvertTo(context, culture, value, destinationType);
//        }


//    }
//}
