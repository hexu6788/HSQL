using System.Data;

namespace HSQL.ConnectionPools
{
    public class Connector : IConnector
    {
        
        private ConnectorState _state;
        private IDbConnection _connection;

        public Connector(IDbConnection connection)
        {
            _state = ConnectorState.可用;
            _connection = connection;
        }

        public ConnectorState GetState()
        {
            return _state;
        }

        public void SetState(ConnectorState state)
        {
            _state = state;
        }

        public void Dispose()
        {
            _state = ConnectorState.可用;
        }

        public IDbConnection GetConnection()
        {
            if (_connection.State == ConnectionState.Closed || _connection.State == ConnectionState.Broken)
                _connection.Open();

            return _connection;
        }
    }

    public enum ConnectorState
    {
        可用 = 1, 占用 = 2
    }
}
