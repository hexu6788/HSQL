using System;
using System.Data;

namespace HSQL.Base
{
    public class DbHelperBase
    {
        /// <summary>
        /// 执行增删改操作
        /// </summary>
        /// <param name="connection">连接对象</param>
        /// <param name="consolePrintSql">是否在控制台打印Sql语句</param>
        /// <param name="commandText">执行命令</param>
        /// <param name="parameters">参数</param>
        /// <returns>执行完成后，影响行数</returns>
        public int ExecuteNonQuery(IDbConnection connection, bool consolePrintSql, string commandText, params IDbDataParameter[] parameters)
        {
            int result = 0;
            using (IDbCommand command = connection.CreateCommand())
            {
                command.CommandText = commandText;
                foreach (IDbDataParameter parameter in parameters)
                {
                    command.Parameters.Add(parameter);
                }
                result = command.ExecuteNonQuery();
            }

            PrintSql(consolePrintSql, commandText);
            return result;
        }

        /// <summary>
        /// 在控制台打印Sql语句
        /// </summary>
        /// <param name="consolePrintSql">是否要打印Sql</param>
        /// <param name="commandText">将要打印的Sql语句</param>
        public void PrintSql(bool consolePrintSql, string commandText)
        {
            if (consolePrintSql && !string.IsNullOrWhiteSpace(commandText))
                System.Diagnostics.Trace.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} 执行的Sql语句为：{commandText}");
        }
    }
}
