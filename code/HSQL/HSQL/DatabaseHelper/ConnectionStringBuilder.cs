using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace HSQL.DatabaseHelper
{
    internal class ConnectionStringBuilder
    {
        internal static string BuildMySqlConnectionString(string server, string database, string userID, string password, bool pooling = true, int maximumPoolSize = 100, int minimumPoolSize = 0)
        {
            MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder()
            {
                Server = server,
                Database = database,
                UserID = userID,
                Password = password,
                Pooling = pooling,
                MaximumPoolSize = (uint)maximumPoolSize,
                MinimumPoolSize = (uint)minimumPoolSize
            };
            return connectionStringBuilder.ToString();
        }

        internal static string BuildSqlConnectionString(string dataSource, string initialCatalog, string userID, string password, bool pooling = true, int maxPoolSize = 100, int minPoolSize = 0)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = dataSource,
                InitialCatalog = initialCatalog,
                UserID = userID,
                Password = password,
                Pooling = pooling,
                MaxPoolSize = maxPoolSize,
                MinPoolSize = minPoolSize
            };
            return connectionStringBuilder.ToString();
        }

    }
}
