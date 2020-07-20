using System.Data;
using System.Data.SqlClient;

namespace HSQL.MSSQLServer
{
    internal class SQLServerConnector
    {
        private bool _usable;
        private SqlConnection _connection;


        internal SQLServerConnector(string connectionString)
        {
            _usable = true;
            _connection = new SqlConnection(connectionString);
        }

        /// <summary>
        /// 是否可用
        /// </summary>
        internal bool Usable { get { return _usable; } set { _usable = value; } }

        /// <summary>
        /// 获取连接对象
        /// </summary>
        /// <returns></returns>
        internal SqlConnection GetConnection()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
            return _connection;
        }
        
    }
}
