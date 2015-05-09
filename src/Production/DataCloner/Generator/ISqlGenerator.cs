using System;

namespace DataCloner.Generator
{
    interface ISqlGenerator
    {
        Char DelemitedIdentifierCaracter { get; }
    }
}
