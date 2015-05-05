using System;
using System.Data;

namespace DataCloner.DataAccess
{
    internal abstract class AbstractSqlTypeConverter : ISqlTypeConverter
    {
        public DbType ConvertFromSql(SqlType type)
        {
            //fullType = fullType.ToLower();
            //var startPosLength = fullType.IndexOf("(", StringComparison.Ordinal);
            //var endPosLength = fullType.IndexOf(")", StringComparison.Ordinal);
            //var values = fullType.Split(' ');
            //string strType;
            //string[] descriptorValues = null;
            ////int length;
            ////int precision;
            //bool? signedness = null;
            //type = DbType.Object;
            //size = null;

            ////S'il y a une description du type entre ()
            //if (startPosLength > -1 && endPosLength > startPosLength)
            //{
            //    size = fullType.Substring(startPosLength + 1, endPosLength - startPosLength - 1);
            //    descriptorValues = size.Split(',');
            //    strType = values[0].Substring(0, startPosLength);
            //}
            //else
            //{
            //    strType = values[0];
            //}

            //if (values.Length > 1)
            //    signedness = values[1] == "signed";

            ////Parse descriptior
            ////switch (strType)
            ////{
            ////    case "enum":
            ////    case "set":
            ////        type = DbType.Object; //Not supported
            ////        break; 
            ////    default:
            ////        if (descriptorValues != null)
            ////        {
            ////            if (descriptorValues.Length > 1)
            ////                precision = Int32.Parse(descriptorValues[1]);
            ////            length = Int32.Parse(descriptorValues[0]);
            ////        }
            ////        break;
            ////}

            ////From unsigned to CLR data type
            //if (signedness.HasValue && !signedness.Value)
            //{
            //    switch (strType)
            //    {
            //        case "tinyint":
            //        case "smallint":
            //        case "mediumint": //À vérifier
            //            type = DbType.Int32;
            //            break;
            //        case "int":
            //            type = DbType.Int64;
            //            break;
            //        case "bigint":
            //            type = DbType.Decimal;
            //            break;
            //    }
            //}
            //else
            //{
            //    //From signed to CLR data type
            //    switch (strType)
            //    {
            //        case "tinyint":
            //            type = DbType.Byte;
            //            break;
            //        case "smallint":
            //        case "year":
            //            type = DbType.Int16;
            //            break;
            //        case "mediumint":
            //        case "int":
            //            type = DbType.Int32;
            //            break;
            //        case "bigint":
            //        case "bit":
            //            type = DbType.Int64;
            //            break;
            //        case "float":
            //            type = DbType.Single;
            //            break;
            //        case "double":
            //            type = DbType.Double;
            //            break;
            //        case "decimal":
            //            type = DbType.Decimal;
            //            break;
            //        case "char":
            //        case "varchar":
            //        case "tinytext":
            //        case "text":
            //        case "mediumtext":
            //        case "longtext":
            //        case "binary":
            //        case "varbinary":
            //            type = DbType.String;
            //            break;
            //        case "tinyblob":
            //        case "blob":
            //        case "mediumblob":
            //        case "longblob":
            //        case "enum":
            //        case "set":
            //            type = DbType.Binary;
            //            break;
            //        case "date":
            //        case "datetime":
            //            type = DbType.DateTime;
            //            break;
            //        case "time":
            //        case "timestamp":
            //            type = DbType.Time;
            //            break;
            //    }
            //}
            //}

            if (Int32FromSql(type))
                return DbType.Int32;
            if (StringFromSql(type))
                return DbType.String;

            throw new NotSupportedException();
        }

        public SqlType ConvertToSql(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString:
                    return AnsiStringToSql();
                //...
                case DbType.Int32:
                    return Int32ToSql();
            }

            throw new NotImplementedException();
        }

        #region FromSql

        public virtual bool Int32FromSql(SqlType t)
        {
            if (t.DataType == "int" ||
                t.DataType == "integer")
                return true;
            return false;
        }

        public virtual bool StringFromSql(SqlType t)
        {
            if (t.DataType == "varchar" ||
                t.DataType == "tinytext" ||
                t.DataType == "text" ||
                t.DataType == "mediumtext" ||
                t.DataType == "longtext")
                return true;
            return false;
        }

        #endregion

        #region ToSql

        protected abstract SqlType AnsiStringToSql();

        protected virtual SqlType Int32ToSql()
        {
            return new SqlType
            {
                DataType = "integer",
                Signess = true
            };
        }

        #endregion
    }
}
