using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HSQL.DatabaseHelper
{
    class MySQLHelper
    {
        internal static int ExecuteNonQuery(string connectionString, string commandText)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            var result = 0;
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = commandText;
                    result = command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
            }
            return result;
        }
        internal static int ExecuteNonQuery(string connectionString, string commandText, List<MySqlParameter> parameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            var result = 0;
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = commandText;
                    command.Parameters.AddRange(parameters.ToArray());
                    result = command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
            }
            return result;
        }
        internal static int ExecuteNonQueryBatch(string connectionString, List<string> commandTextList)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (commandTextList == null || commandTextList.Count <= 0 || commandTextList.Count(x => string.IsNullOrWhiteSpace(x)) > 0)
                throw new ArgumentNullException("执行命令不能为空");

            var result = 0;
            var connection = new MySqlConnection(connectionString);
            var command = connection.CreateCommand();

            try
            {
                connection.Open();
                command.Transaction = connection.BeginTransaction();
                for (var i = 0; i < commandTextList.Count; i++)
                {
                    command.CommandText = commandTextList[i];
                    result += command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                command.Transaction.Commit();
            }
            catch (Exception ex)
            {
                command.Transaction.Rollback();
                throw ex;
            }
            finally
            {
                command.Dispose();
                connection.Dispose();
            }
            return result;
        }
        internal static int ExecuteNonQueryBatch(string connectionString, List<string> commandTextList, List<MySqlParameter[]> parametersList)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (commandTextList == null || commandTextList.Count <= 0 || commandTextList.Count(x => string.IsNullOrWhiteSpace(x)) > 0)
                throw new ArgumentNullException("执行命令不能为空");
            if (parametersList == null || parametersList.Count <= 0 || parametersList.Count(x => x == null) > 0)
                throw new ArgumentNullException("参数不能为空");

            var result = 0;
            var connection = new MySqlConnection(connectionString);
            var command = connection.CreateCommand();

            try
            {
                connection.Open();
                command.Transaction = connection.BeginTransaction();
                for (var i = 0; i < commandTextList.Count; i++)
                {
                    command.CommandText = commandTextList[i];
                    command.Parameters.AddRange(parametersList[i]);
                    result += command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
                command.Transaction.Commit();
            }
            catch (Exception ex)
            {
                command.Transaction.Rollback();
                throw ex;
            }
            finally
            {
                command.Dispose();
                connection.Dispose();
            }
            return result;
        }
        internal static MySqlDataReader ExecuteReader(string connectionString, string commandText)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            var connection = new MySqlConnection(connectionString);
            var command = connection.CreateCommand();
            connection.Open();
            command.CommandText = commandText;
            var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            command.Parameters.Clear();

            return reader;
        }
        internal static object ExecuteScalar(string connectionString, string commandText)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            object result = null;
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = commandText;
                    result = command.ExecuteScalar();
                    command.Parameters.Clear();
                }
            }
            return result;
        }
    }
}
