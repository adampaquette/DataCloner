using System;

namespace DataCloner.Core.Data.Generator
{
    public interface ISqlWriter
    {
        IInsertWriter GetInsertWriter();
    }
}
