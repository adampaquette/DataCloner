using System;
using System.Collections.Generic;
using System.Data;

namespace DataCloner.DataClasse
{
    public interface IServerIdentifier
    {
        Int16 ServerId { get; set; }
    }

    public interface ITableIdentifier : IServerIdentifier
    {
        string Database { get; set; }
        string Schema { get; set; }
        string Table { get; set; }
    }

    public interface IColumnIdentifier : ITableIdentifier
    {
        string Column { get; set; }
    }

    public interface IRowIdentifier : ITableIdentifier, IEquatable<IRowIdentifier>
    {
        ColumnsWithValue Columns { get; set; }
    }
}
