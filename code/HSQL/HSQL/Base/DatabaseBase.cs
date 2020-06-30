using System;
using System.Transactions;

namespace HSQL.Base
{
    public class DatabaseBase
    {
        protected string _connectionString;

        /// <summary>
        /// 构建连接字符串
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="database">数据库</param>
        /// <param name="userID">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="pooling">是否启用连接池</param>
        /// <param name="maximumPoolSize">连接池最大数</param>
        /// <param name="minimumPoolSize">连接池最小数</param>
        /// <returns>连接字符串</returns>
        public virtual string BuildConnectionString(string server, string database, string userID, string password, bool pooling, int maximumPoolSize, int minimumPoolSize)
        {
            throw new Exception("BuildConnectionString Exception");
        }


        /// <summary>
        /// 事务调用
        /// </summary>
        /// <param name="action">方法体</param>
        public void Transaction(Action action)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    action();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    scope.Dispose();
                }
            }
        }
    }


}
