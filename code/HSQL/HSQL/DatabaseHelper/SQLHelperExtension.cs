using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace HSQL.DatabaseHelper
{
    internal class SQLHelperExtension
    {
        internal static List<MySqlParameter> ConverToMySqlParameterList(Dialect dialect, DbParameter[] parametersList)
        {
            return parametersList.Select(x => (MySqlParameter)x).ToList();
        }

        internal static List<MySqlParameter[]> ConverToMySqlParameterList(Dialect dialect, List<DbParameter[]> parametersList)
        {
            return parametersList.Select(x => x.Select(y => (MySqlParameter)y).ToArray()).ToList();
        }

        internal static List<SqlParameter> ConverToSqlParameterList(Dialect dialect, DbParameter[] parametersList)
        {
            return parametersList.Select(x => (SqlParameter)x).ToList();
        }

        internal static List<SqlParameter[]> ConverToSqlParameterList(Dialect dialect, List<DbParameter[]> parametersList)
        {
            return parametersList.Select(x => x.Select(y => (SqlParameter)y).ToArray()).ToList();
        }
    }
}
