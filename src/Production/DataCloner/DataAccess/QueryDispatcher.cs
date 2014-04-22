using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DataCloner.Serialization;

namespace DataCloner.DataAccess
{
    class QueryDispatcher : IQueryDispatcher
    {
        private readonly Configuration _config;
        private Dictionary<Int16, IQueryProvider> _conns;

        public QueryDispatcher(Configuration config)
        {
            _config = config;
            _conns = new Dictionary<Int16, IQueryProvider>();

            //Récupération des providers qui seront utilisés pour effectuer les requêtes
            foreach (var conn in _config.ConnectionStrings)
            {
                var t = Type.GetType(conn.ProviderName);
                IQueryProvider provider = Activator.CreateInstance(t, new object[] { conn.ConnectionString }) as IQueryProvider;
                _conns.Add(conn.Id, provider);
            }
        }

        public DataTable GetFK(ITableIdentifier ti)
        {
            return _conns[ti.ServerID].GetFK(ti);
        }

        public long GetLastInsertedPK(Int16 serverId)
        {
            return _conns[serverId].GetLastInsertedPK();
        }

        public DataTable Select(IRowIdentifier ri)
        {
            return _conns[ri.TableIdentifier.ServerID].Select(ri);
        }

        public void Insert(ITableIdentifier ti, DataRow[] rows)
        {
            _conns[ti.ServerID].Insert(ti, rows);
        }

        public void Update(IRowIdentifier ri, DataRow[] rows)
        {
            _conns[ri.TableIdentifier.ServerID].Update(ri, rows);
        }

        public void Delete(IRowIdentifier ri)
        {
            _conns[ri.TableIdentifier.ServerID].Delete(ri);
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
