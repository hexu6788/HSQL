﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

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
