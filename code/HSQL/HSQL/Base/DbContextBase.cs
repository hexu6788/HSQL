using System;
using System.Threading;
using System.Transactions;

namespace HSQL.Base
{
    public abstract class DbContextBase
    {
        /// <summary>
        /// 是否开启事务
        /// </summary>
        public ThreadLocal<bool> TransactionIsOpen = new ThreadLocal<bool>(() => false);
        protected IDbSQLHelper _dbSQLHelper;

        /// <summary>
        /// 构建连接字符串
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="database">数据库</param>
        /// <param name="userID">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>连接字符串</returns>
        public abstract string BuildConnectionString(string server, string database, string userID, string password);

        /// <summary>
        /// 事务调用
        /// </summary>
        /// <param name="action">方法体</param>
        public void Transaction(Action action)
        {
            TransactionIsOpen.Value = true;
            using (TransactionScope scope = new TransactionScope())
            {
                action();
                scope.Complete();
            }
            TransactionIsOpen.Value = false;
        }
    }


}
