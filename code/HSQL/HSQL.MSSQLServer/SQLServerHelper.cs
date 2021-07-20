using HSQL.Base;
using HSQL.ConnectionPools;
using HSQL.Factory;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace HSQL.MSSQLServer
{
    internal class SQLServerHelper : DbHelperBase, IDbSQLHelper
    {
        
        private string ConnectionString { get; set; }
        public SQLServerHelper(string connectionString, bool consolePrintSql)
        {
            ConnectionString = connectionString;
            ConsolePrintSql = consolePrintSql;
        }

        public int ExecuteNonQuery(bool isNewConnection, string commandText, params IDbDataParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandText))
                throw new ArgumentNullException("执行命令不能为空");

            int result = 0;
            if (isNewConnection == true)
            {
                using (IDbConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    result = ExecuteNonQuery(connection, commandText, parameters);
                }
            }
            else
            {
                using (IConnector connector = SQLServerConnectionPools.GetConnector())
                {
                    result = ExecuteNonQuery(connector.GetConnection(), commandText, parameters);
                }
            }
            return result;
        }

        public object ExecuteScalar(string commandText, params IDbDataParameter[] parameters)
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
                    PrintSql(commandText);
                    result = command.ExecuteScalar();
                }
            }
            return result;
        }

        public List<dynamic> ExecuteList(string commandText, params IDbDataParameter[] parameters)
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
                PrintSql(commandText);
                IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                list = InstanceFactory.CreateListAndDisposeReader(reader);
            }
            return list;
        }

        public List<T> ExecuteList<T>(string commandText, params IDbDataParameter[] parameters)
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
                PrintSql(commandText);
                IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
                list = InstanceFactory.CreateListAndDisposeReader<T>(reader);
            }
            return list;
        }

        public IDbDataParameter[] Convert(List<Parameter> parameters)
        {
            return parameters.Select(x => new SqlParameter(x.ParameterName, x.Value)).ToArray();
        }
    }
}
