using System;
using System.Data;
using DataCloner.Framework;

namespace DataCloner.Data
{
    internal abstract class SqlTypeConverterBase : ISqlTypeConverter
    {
        public DbType ConvertFromSql(SqlType type)
        {
            if (AnsiStringFromSql(type)) return DbType.AnsiString;
            if (AnsiStringFixedLengthFromSql(type)) return DbType.AnsiStringFixedLength;
            if (BinaryFromSql(type)) return DbType.Binary;
            if (BooleanFromSql(type)) return DbType.Boolean;
            if (ByteFromSql(type)) return DbType.Byte;
            if (CurrencyFromSql(type)) return DbType.Currency;
            if (DateFromSql(type)) return DbType.Date;
            if (DateTimeFromSql(type)) return DbType.DateTime;
            if (DateTime2FromSql(type)) return DbType.DateTime2;
            if (DateTimeOffsetFromSql(type)) return DbType.DateTimeOffset;
            if (DecimalFromSql(type)) return DbType.Decimal;
            if (DoubleFromSql(type)) return DbType.Double;
            if (GuidFromSql(type)) return DbType.Guid;
            if (Int16FromSql(type)) return DbType.Int16;
            if (Int32FromSql(type)) return DbType.Int32;
            if (Int64FromSql(type)) return DbType.Int64;
            if (ObjectFromSql(type)) return DbType.Object;
            if (SByteFromSql(type)) return DbType.SByte;
            if (SingleFromSql(type)) return DbType.Single;
            if (StringFromSql(type)) return DbType.String;
            if (StringFixedLengthFromSql(type)) return DbType.StringFixedLength;
            if (TimeFromSql(type)) return DbType.Time;
            if (UInt16FromSql(type)) return DbType.UInt16;
            if (UInt32FromSql(type)) return DbType.UInt32;
            if (UInt64FromSql(type)) return DbType.UInt64;
            if (VarNumericFromSql(type)) return DbType.VarNumeric;
            if (XmlFromSql(type)) return DbType.Xml;
            
            throw new NotSupportedException(type.SerializeXml());
        }

        public SqlType ConvertToSql(DbType type)
        {
            switch (type)
            {
                case DbType.AnsiString: return AnsiStringToSql();
                case DbType.Int32: return Int32ToSql();
            }

            throw new NotImplementedException();
        }

        #region FromSql
		
		protected virtual bool AnsiStringFromSql(SqlType t)
        {
            return false;
        }
        
		protected virtual bool AnsiStringFixedLengthFromSql(SqlType t)
        {
            return false;
        }
		
		protected virtual bool BinaryFromSql(SqlType t)
        {
            if (t.DataType == "binary" ||
                t.DataType == "varbinary" ||
                t.DataType == "timestamp" ||
                t.DataType == "rowversion" ||
                t.DataType == "image")
                return true;
            return false;
        }
        
        protected virtual bool BooleanFromSql(SqlType t)
        {
            if (t.DataType == "bit")
                return true;
            return false;
        }
        
        protected virtual bool ByteFromSql(SqlType t)
        {
            if (t.DataType == "tinyint")
                return true;
            return false;
        }

        protected virtual bool CurrencyFromSql(SqlType t)
        {
            return false;
        }
        
        protected virtual bool DateFromSql(SqlType t)
        {
            if (t.DataType == "date")
                return true;
            return false;
        }
        
        protected virtual bool DateTimeFromSql(SqlType t)
        {
            if (t.DataType == "datetime")
                return true;
            return false;
        }
        
        protected virtual bool DateTime2FromSql(SqlType t)
        {
            if (t.DataType == "datetime2")
                return true;
            return false;
        }
        
        protected virtual bool DateTimeOffsetFromSql(SqlType t)
        {
            if (t.DataType == "datetimeoffset")
                return true;
            return false;
        }
        
        protected virtual bool DecimalFromSql(SqlType t)
        {
            if (t.DataType == "decimal" ||
                t.DataType == "numeric" ||
                t.DataType == "money" ||
                t.DataType == "smallmoney")
                return true;
            return false;
        }
        
        protected virtual bool DoubleFromSql(SqlType t)
        {
            if (t.DataType == "float")
                return true;
            return false;
        }
        
        protected virtual bool GuidFromSql(SqlType t)
        {
            if (t.DataType == "uniqueidentifier")
                return true;
            return false;
        }
        
        protected virtual bool Int16FromSql(SqlType t)
        {
            if (t.DataType == "smallint")
                return true;
            return false;
        }
        
        protected virtual bool Int32FromSql(SqlType t)
        {
            if (t.DataType == "int")
                return true;
            return false;
        }
        
        protected virtual bool Int64FromSql(SqlType t)
        {
            if (t.DataType == "bigint")
                return true;
            return false;
        }
		
        protected virtual bool ObjectFromSql(SqlType t)
        {
            if (t.DataType == "sql_variant")
                return true;
            return false;
        }

        protected virtual bool SByteFromSql(SqlType t)
        {
            return false;
        }
        
        protected virtual bool SingleFromSql(SqlType t)
        {
            if (t.DataType == "real")
                return true;
            return false;
        }
        
        protected virtual bool StringFromSql(SqlType t)
        {
            if (t.DataType == "char" ||
                t.DataType == "varchar" ||
                t.DataType == "nvarchar" ||
                t.DataType == "text" ||
                t.DataType == "ntext")
                return true;
            return false;
        }
        
        protected virtual bool StringFixedLengthFromSql(SqlType t)
        {
            if (t.DataType == "nchar")
                return true;
            return false;
        }
        
        protected virtual bool TimeFromSql(SqlType t)
        {
            if (t.DataType == "time")
                return true;
            return false;
        }

        protected virtual bool UInt16FromSql(SqlType t)
        {
            return false;
        }

        protected virtual bool UInt32FromSql(SqlType t)
        {
            return false;
        }

        protected virtual bool UInt64FromSql(SqlType t)
        {
            return false;
        }

        protected virtual bool VarNumericFromSql(SqlType t)
        {
            return false;
        }
        
        protected virtual bool XmlFromSql(SqlType t)
        {
            if (t.DataType == "xml")
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
                DataType = "integer"
            };
        }

        #endregion
    }
}
