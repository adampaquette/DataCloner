using System;

namespace DataCloner.Generator
{
    public interface ISqlWriter
    {
        IInsertWriter GetInsertWriter();
    }
}
