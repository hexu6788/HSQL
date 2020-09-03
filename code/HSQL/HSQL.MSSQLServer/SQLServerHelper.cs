using HSQL.Base;
using HSQL.ConnectionPools;
using HSQL.Factory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace HSQL.MSSQLServer
{
    internal class SQLServerHelper : DbHelperBase, IDbSQLHelper
    {
        
        private string _connectionString;
        public SQLServerHelper(string connectionString)
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
                using (IDbConnection connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    result = ExecuteNonQuery(connection, consolePrintSql, commandText, parameters);
                }
            }
            else
            {
                using (IConnector connector = SQLServerConnectionPools.GetConnector())
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
            using (IConnector connector = SQLServerConnectionPools.GetConnector())
            {
                using (IDbCommand command = connector.GetConnection().CreateCommand())
                {
                    command.CommandText = commandText;
                    foreach (IDbDataParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    if (consolePrintSql)
                        PrintSql(commandText);
                    result = command.ExecuteScalar();
                }
            }
            return result;
        }

        public List<dynamic> ExecuteList(bool consolePrintSql, string commandText, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            List<dynamic> list = new List<dynamic>();
            using (IConnector connector = SQLServerConnectionPools.GetConnector())
            {
                IDbCommand command = connector.GetConnection().CreateCommand();
                command.CommandText = commandText;
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                if (consolePrintSql)
                    PrintSql(commandText);
                IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                list = InstanceFactory.CreateListAndDisposeReader(reader);
            }
            return list;
        }

        public List<T> ExecuteList<T>(bool consolePrintSql, List<PropertyInfo> propertyInfoList, string commandText, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            List<T> list = new List<T>();
            using (IConnector connector = SQLServerConnectionPools.GetConnector())
            {
                IDbCommand command = connector.GetConnection().CreateCommand();
                command.CommandText = commandText;
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                if (consolePrintSql)
                    PrintSql(commandText);
                IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                list = InstanceFactory.CreateListAndDisposeReader<T>(reader, propertyInfoList);
            }
            return list;
        }

        
    }
}
