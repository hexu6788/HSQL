using System;
using System.Data;
using System.Data.SqlClient;

namespace HSQL.DatabaseHelper
{
    class SQLServerHelper
    {
        internal static int ExecuteNonQuery(string connectionString, string commandText, params SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");


            int result = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
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
        internal static SqlDataReader ExecuteReader(string connectionString, string commandText, params SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException("连接字符串不能为空！");
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            SqlConnection connection = new SqlConnection(connectionString);
            SqlCommand command = connection.CreateCommand();
            connection.Open();
            command.CommandText = commandText;
            command.Parameters.AddRange(parameters);
            SqlDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
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
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = connection.CreateCommand())
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
