using System;

namespace DataCloner.Generator
{
    public abstract class AbstractSqlWriter : ISqlWriter
    {
        /// <summary>
        /// The delimited identifier or quoted identifier. 
        /// It is formed by enclosing an arbitrary sequence of characters in double-quotes ("). 
        /// A delimited identifier is always an identifier, never a key word. So "select" could be 
        /// used to refer to a column or table named "select", whereas an unquoted select would be 
        /// taken as a key word and would therefore provoke a parse error when used where a table or 
        /// column name is expected.
        /// </summary>
        /// <seealso cref="http://www.postgresql.org/docs/8.2/static/sql-syntax-lexical.html"/>
        public virtual char DelemitedIdentifierCaracter => '"';

        public abstract IInsertWriter GetInsertWriter();
    }
}
