﻿using DataCloner.Core.Metadata.Context;
using System.Data;

namespace DataCloner.Core.Data
{
    public class ServerContext
    {
        public IDbConnection Connection { get; }

        public IMetadataProvider MetadataProvider { get; }

        public IQueryProvider QueryProvider { get; }

        public Metadatas Metadatas { get; }

        public ServerContext(IDbConnection connection, IMetadataProvider metadataProvider, IQueryProvider queryProvider, Metadatas metadatas)
        {
            Connection = connection;
            MetadataProvider = metadataProvider;
            QueryProvider = queryProvider;
            Metadatas = metadatas;
        }
    }
}
