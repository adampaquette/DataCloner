﻿using System;
using System.Data;

using DataCloner.DataClasse.Cache;

namespace DataCloner.Interface
{
    public interface IQueryProvider : IDisposable
    {
        IDbConnection Connection { get; }
        bool IsReadOnly { get; }
        string[] GetDatabasesName();
        void FillForeignKeys(CachedTables tables);        
        DataTable GetFk(ITableIdentifier ti);
        Int64 GetLastInsertedPk();
        DataTable Select(IRowIdentifier ri);
        void Insert(ITableIdentifier ti, DataRow[] rows);
        void Update(IRowIdentifier ri, DataRow[] rows);
        void Delete(IRowIdentifier ri);
    }
}
