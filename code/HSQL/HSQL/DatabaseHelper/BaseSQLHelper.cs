using HSQL.Exceptions;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace HSQL.DatabaseHelper
{
    internal class BaseSQLHelper
    {

        internal static bool ExecuteNonQuery(Dialect dialect, string connectionString, string sql, params DbParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            if (dialect == Dialect.MySQL)
            {
                return MySQLHelper.ExecuteNonQuery(connectionString, sql, parameters.Select(parameter => (MySqlParameter)parameter).ToArray()) > 0;
            }
            else if (dialect == Dialect.SQLServer)
            {
                return SQLServerHelper.ExecuteNonQuery(connectionString, sql, parameters.Select(parameter => (SqlParameter)parameter).ToArray()) > 0;
            }

            throw new NoDialectException();
        }
        internal static object ExecuteScalar(Dialect dialect, string connectionString, string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            if (dialect == Dialect.MySQL)
            {
                return MySQLHelper.ExecuteScalar(connectionString, sql);
            }
            else if (dialect == Dialect.SQLServer)
            {
                return SQLServerHelper.ExecuteScalar(connectionString, sql);
            }

            throw new NoDialectException();
        }
        internal static IDataReader ExecuteReader(Dialect dialect, string connectionString, string sql, params DbParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            if (dialect == Dialect.MySQL)
            {
                return MySQLHelper.ExecuteReader(connectionString, sql, parameters.Select(parameter => (MySqlParameter)parameter).ToArray());
            }
            else if (dialect == Dialect.SQLServer)
            {
                return SQLServerHelper.ExecuteReader(connectionString, sql, parameters.Select(parameter => (SqlParameter)parameter).ToArray());
            }

            throw new NoDialectException();
        }

        
    }
}
