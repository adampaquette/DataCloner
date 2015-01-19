﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataCloner.Enum;
using System.Data;

namespace DataCloner.DataClasse.Cache
{
    public interface ITableDef
    {
        string Name { get; set; }
        bool IsStatic { get; set; }
        string SelectCommand { get; set; }
        string InsertCommand { get; set; }
        IDerivativeTable[] DerivativeTables { get; set; }
        IForeignKey[] ForeignKeys { get; set; }
        ISchemaColumn[] SchemaColumns { get; set; }
    }

    public interface IDerivativeTable
    {
        Int16 ServerId { get; set; }
        string Database { get; set; }
        string Schema { get; set; }
        string Table { get; set; }
        DerivativeTableAccess Access { get; set; }
        bool Cascade { get; set; }
    }

    public interface IForeignKey
    {
        Int16 ServerIdTo { get; set; }
        string DatabaseTo { get; set; }
        string SchemaTo { get; set; }
        string TableTo { get; set; }
        IForeignKeyColumn[] Columns { get; set; }
    }

    public interface IForeignKeyColumn
    {
        string NameFrom { get; set; }
        string NameTo { get; set; }
    }

    public interface ISchemaColumn
    {
        string Name { get; set; }
        DbType Type { get; set; }
        bool IsPrimary { get; set; }
        bool IsForeignKey { get; set; }
        bool IsAutoIncrement { get; set; }
        string BuilderName { get; set; }
    }
}
