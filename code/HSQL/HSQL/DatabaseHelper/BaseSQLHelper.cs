using HSQL.Exceptions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

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
                var mySqlParameter = SQLHelperExtension.ConverToMySqlParameterList(dialect, dbParameters);
                return MySQLHelper.ExecuteNonQuery(connectionString, sql, mySqlParameter) > 0;
            }
            else if (dialect == Dialect.SQLServer)
            {
                var sqlParameter = SQLHelperExtension.ConverToSqlParameterList(dialect, dbParameters);
                return SQLServerHelper.ExecuteNonQuery(connectionString, sql, sqlParameter) > 0;
            }

            throw new NoDialectException();
        }

        internal static bool ExecuteNonQueryBatch(Dialect dialect, string connectionString, List<string> sqls)
        {
            if (dialect == Dialect.MySQL)
            {
                return MySQLHelper.ExecuteNonQueryBatch(connectionString, sqls) > 0;
            }
            else if (dialect == Dialect.SQLServer)
            {
                return SQLServerHelper.ExecuteNonQueryBatch(connectionString, sqls) > 0;
            }

            throw new NoDialectException();
        }

        internal static bool ExecuteNonQueryBatch(Dialect dialect, string connectionString, List<string> sqls, List<DbParameter[]> parametersList)
        {
            if (dialect == Dialect.MySQL)
            {
                var parameter = SQLHelperExtension.ConverToMySqlParameterList(dialect, parametersList);
                return MySQLHelper.ExecuteNonQueryBatch(connectionString, sqls, parameter) > 0;
            }
            else if (dialect == Dialect.SQLServer)
            {
                var parameter = SQLHelperExtension.ConverToSqlParameterList(dialect, parametersList);
                return SQLServerHelper.ExecuteNonQueryBatch(connectionString, sqls, parameter) > 0;
            }

            throw new NoDialectException();
        }

    }
}
