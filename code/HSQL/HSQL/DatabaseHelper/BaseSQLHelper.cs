using HSQL.Exceptions;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace HSQL.DatabaseHelper
{
    internal class BaseSQLHelper
    {
        internal static bool ExecuteNonQuery(Dialect dialect, string connectionString, string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            if (dialect == Dialect.MySQL)
            {
                return MySQLHelper.ExecuteNonQuery(connectionString, sql) > 0;
            }
            else if (dialect == Dialect.SQLServer)
            {
                return SQLServerHelper.ExecuteNonQuery(connectionString, sql) > 0;
            }

            throw new NoDialectException();
        }

        internal static bool ExecuteNonQuery(Dialect dialect, string connectionString, string sql, DbParameter[] dbParameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            if (dialect == Dialect.MySQL)
            {
                var mySqlParameterList = dbParameters.Select(x => (MySqlParameter)x).ToList();
                return MySQLHelper.ExecuteNonQuery(connectionString, sql, mySqlParameterList) > 0;
            }
            else if (dialect == Dialect.SQLServer)
            {
                var sqlParameterList = dbParameters.Select(x => (SqlParameter)x).ToList();
                return SQLServerHelper.ExecuteNonQuery(connectionString, sql, sqlParameterList) > 0;
            }

            throw new NoDialectException();
        }

        internal static object ExecuteScalar(Dialect dialect, string connectionString, string commandText)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new EmptySQLException();

            if (dialect == Dialect.MySQL)
            {
                return MySQLHelper.ExecuteScalar(connectionString, commandText);
            }
            else if (dialect == Dialect.SQLServer)
            {
                return SQLServerHelper.ExecuteScalar(connectionString, commandText);
            }

            throw new NoDialectException();
        }
    }
}
