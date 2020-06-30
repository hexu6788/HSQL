using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace HSQL.MySQL
{
    internal class MySQLHelper
    {
        internal static int ExecuteNonQuery(string connectionString, string commandText, params MySqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            int result = 0;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = commandText;
                    command.Parameters.AddRange(parameters);
                    result = command.ExecuteNonQuery();
                    command.Parameters.Clear();
                }
            }
            return result;
        }
        internal static MySqlDataReader ExecuteReader(string connectionString, string commandText, params MySqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");


            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlCommand command = connection.CreateCommand();
            connection.Open();
            command.CommandText = commandText;
            command.Parameters.AddRange(parameters);
            MySqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
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
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand command = connection.CreateCommand())
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
