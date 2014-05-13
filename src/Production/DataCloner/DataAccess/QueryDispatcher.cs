using System;
using System.Collections.Generic;
using System.Data;
using DataCloner.Interface;
using DataCloner.Serialization;

namespace DataCloner.DataAccess
{
    internal class QueryDispatcher : IQueryDispatcher
    {
        private readonly ConfigurationXml _config;
        private readonly Dictionary<Int16, IQueryProvider> _conns;

        public QueryDispatcher(ConfigurationXml config)
        {
            _config = config;
            _conns = new Dictionary<Int16, IQueryProvider>();

            //Récupération des providers qui seront utilisés pour effectuer les requêtes
            foreach (ConnectionXml conn in _config.ConnectionStrings)
            {
                Type t = Type.GetType(conn.ProviderName);
                var provider = Activator.CreateInstance(t, new object[] {conn.ConnectionString}) as IQueryProvider;
                _conns.Add(conn.Id, provider);
            }
        }

        public DataTable GetFk(ITableIdentifier ti)
        {
            return _conns[ti.ServerId].GetFk(ti);
        }

        public long GetLastInsertedPk(Int16 serverId)
        {
            return _conns[serverId].GetLastInsertedPk();
        }

        public DataTable Select(IRowIdentifier ri)
        {
            return _conns[ri.TableIdentifier.ServerId].Select(ri);
        }

        public void Insert(ITableIdentifier ti, DataRow[] rows)
        {
            _conns[ti.ServerId].Insert(ti, rows);
        }

        public void Update(IRowIdentifier ri, DataRow[] rows)
        {
            _conns[ri.TableIdentifier.ServerId].Update(ri, rows);
        }

        public void Delete(IRowIdentifier ri)
        {
            _conns[ri.TableIdentifier.ServerId].Delete(ri);
        }

        public void Dispose()
        {
            //TODO : IDISPOSABLE
            foreach (var conn in _conns)
            {
                conn.Value.Dispose();
            }
        }
    }
}