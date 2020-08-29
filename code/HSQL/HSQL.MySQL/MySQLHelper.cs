using HSQL.Base;
using HSQL.ConnectionPools;
using HSQL.Factory;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace HSQL.MySQL
{
    internal class MySQLHelper : DbHelperBase, IDbSQLHelper
    {
        private string _connectionString;
        public MySQLHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int ExecuteNonQuery(bool isNewConnection, bool consolePrintSql, string commandText, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            int result = 0;
            if (isNewConnection == true)
            {
                using (IDbConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    result = ExecuteNonQuery(connection, consolePrintSql, commandText, parameters);
                }
            }
            else
            {
                using (IConnector connector = MySQLConnectionPools.GetConnector())
                {
                    result = ExecuteNonQuery(connector.GetConnection(), consolePrintSql, commandText, parameters);
                }
            }
            return result;
        }

        public object ExecuteScalar(bool consolePrintSql, string commandText, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            object result = null;
            using (IConnector connector = MySQLConnectionPools.GetConnector())
            {
                using (IDbCommand command = connector.GetConnection().CreateCommand())
                {
                    command.CommandText = commandText;
                    foreach (IDbDataParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    result = command.ExecuteScalar();
                }
            }

            PrintSql(consolePrintSql, commandText);
            return result;
        }

        public List<dynamic> ExecuteList(bool consolePrintSql, string commandText, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            List<dynamic> list = new List<dynamic>();
            using (IConnector connector = MySQLConnectionPools.GetConnector())
            {
                IDbCommand command = connector.GetConnection().CreateCommand();
                command.CommandText = commandText;
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                list = InstanceFactory.CreateListAndDisposeReader(reader);
            }

            PrintSql(consolePrintSql, commandText);
            return list;
        }

        public List<T> ExecuteList<T>(bool consolePrintSql, List<PropertyInfo> propertyInfoList, string commandText, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            List<T> list = new List<T>();
            using (IConnector connector = MySQLConnectionPools.GetConnector())
            {
                IDbCommand command = connector.GetConnection().CreateCommand();
                command.CommandText = commandText;
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                list = InstanceFactory.CreateListAndDisposeReader<T>(reader, propertyInfoList);
            }

            PrintSql(consolePrintSql, commandText);
            return list;
        }

    }
}
