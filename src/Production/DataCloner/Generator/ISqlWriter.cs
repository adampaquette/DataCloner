using System;

namespace DataCloner.Generator
{
    public interface ISqlWriter
    {
        Char DelemitedIdentifierCaracter { get; }
        IInsertWriter GetInsertWriter();
    }
}
