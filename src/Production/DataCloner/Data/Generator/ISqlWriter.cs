using System;

namespace DataCloner.Data.Generator
{
    public interface ISqlWriter
    {
        IInsertWriter GetInsertWriter();
    }
}
