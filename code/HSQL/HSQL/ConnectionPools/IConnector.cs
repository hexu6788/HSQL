using System;
using System.Data;

namespace HSQL.ConnectionPools
{
    public interface IConnector : IDisposable
    {
        ConnectorState GetState();
        void SetState(ConnectorState state);
        IDbConnection GetConnection();
    }
}
