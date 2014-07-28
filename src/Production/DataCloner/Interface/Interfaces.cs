using System;
using System.Collections.Generic;
using System.Data;

namespace DataCloner.Interface
{
    public interface ITableIdentifier
    {
        Int16 ServerId { get; set; }
        string Database { get; set; }
        string Schema { get; set; }
        string Table { get; set; }
    }

    public interface IColumnIdentifier : ITableIdentifier
    {
        string Column { get; set; }
    }

    public interface IRowIdentifier
    {
        Int16 ServerId { get; set; }
        string Database { get; set; }
        string Schema { get; set; }
        string Table { get; set; }
        IDictionary<string, object> Columns { get; set; }
    }

    public interface IStaticTableDictionnary : IDictionary<ITableIdentifier, bool>
    {
    }

    public interface ITableCache
    {
        string SelectCommand { get; set; }
        string UpdateCommand { get; set; }
        string DeleteCommand { get; set; }
        string InsertCommand { get; set; }
    }

    public interface ITableCacheDictionnary : IDictionary<ITableIdentifier, ITableCache>
    {
    }

    public interface IInfoShemaTable
    {
        IList<IInfoShemaColumn> Columns { get; set; }
    }

    public interface IInfoShemaColumn
    {
        string ColumnName { get; set; }
        byte DataType { get; set; }
    }

    public interface IQueryDispatcher
    {        
        //DataTable GetFk(ITableIdentifier ti);
        //Int64 GetLastInsertedPk(Int16 serverId);
        //object[] Select(IRowIdentifier ri);
        //void Insert(ITableIdentifier ti, DataRow[] rows);
        //void Update(IRowIdentifier ri, DataRow[] rows);
        //void Delete(IRowIdentifier ri);
    }
}
