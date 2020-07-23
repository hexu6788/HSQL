using HSQL.ConnectionPools;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HSQL.MySQL
{
    /// <summary>
    /// 连接池
    /// </summary>
    internal class MySQLConnectionPools
    {
        private static int _size;
        private static readonly object _lockConnector = new object();
        private static List<Connector> _connectorList = new List<Connector>();

        private MySQLConnectionPools()
        {

        }

        internal static void Init(string connectionString, int size = 3)
        {
            _size = size;

            for (var i = 0; i < _size; i++)
            {
                _connectorList.Add(new Connector(new MySqlConnection(connectionString)));
            }
        }

        internal static IConnector GetConnector()
        {
            lock (_lockConnector)
            {
                IConnector connector = _connectorList.Where(x => x.GetState() == ConnectorState.可用).FirstOrDefault();
                if (connector != null)
                {
                    connector.SetState(ConnectorState.占用);
                    return connector;
                }
                else
                {
                    Thread.Sleep(1);
                    return GetConnector();
                }
            }
        }


    }
}
