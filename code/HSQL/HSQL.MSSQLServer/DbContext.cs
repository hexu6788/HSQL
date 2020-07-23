using HSQL.Base;
using HSQL.Exceptions;
using HSQL.Factory;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace HSQL.MSSQLServer
{
    public class DbContext : DbContextBase, IDbContext
    {
        /// <summary>
        /// 构建数据库对象
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="database">数据库名称</param>
        /// <param name="userId">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="poolSize">连接池连接数</param>
        public DbContext(string server, string database, string userId, string password, int poolSize = 3)
        {
            if (string.IsNullOrWhiteSpace(server)
                || string.IsNullOrWhiteSpace(database)
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(password))
                throw new ConnectionStringIsEmptyException();

            if (poolSize <= 0)
                throw new ConnectionStringIsEmptyException($"连接数最小为一个！");

            string connectionString = BuildConnectionString(server, database, userId, password);
            SQLServerConnectionPools.Init(connectionString, poolSize);
            _dbSQLHelper = new SQLServerHelper(connectionString);
        }

        public override string BuildConnectionString(string server, string database, string userID, string password)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = server,
                InitialCatalog = database,
                UserID = userID,
                Password = password
            };
            return connectionStringBuilder.ToString();
        }

        public bool Insert<T>(T instance)
        {
            if (instance == null)
                throw new DataIsNullException();

            string sql = StoreBase.BuildInsertSQL(instance);
            SqlParameter[] parameters = SQLServerStore.BuildSqlParameters(ExpressionFactory.GetColumnList(instance));

            bool isNewConnection = TransactionIsOpen.Value;
            return _dbSQLHelper.ExecuteNonQuery(isNewConnection, sql, parameters) > 0;
        }

        public bool Update<T>(Expression<Func<T, bool>> expression, T instance)
        {
            if (expression == null)
                throw new ExpressionIsNullException();
            if (instance == null)
                throw new DataIsNullException();

            Tuple<string, SqlParameter[]> result = SQLServerStore.BuildUpdateSQLAndParameters(expression, instance);

            bool isNewConnection = TransactionIsOpen.Value;
            return _dbSQLHelper.ExecuteNonQuery(isNewConnection, result.Item1, result.Item2) > 0;
        }

        public bool Delete<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ExpressionIsNullException();

            string sql = StoreBase.BuildDeleteSQL(predicate);

            bool isNewConnection = TransactionIsOpen.Value;
            return _dbSQLHelper.ExecuteNonQuery(isNewConnection, sql) > 0;
        }

        public IQueryabel<T> Query<T>()
        {
            return Query<T>(null);
        }

        public IQueryabel<T> Query<T>(Expression<Func<T, bool>> predicate)
        {
            bool isNewConnection = TransactionIsOpen.Value;
            IQueryabel<T> queryabel = new SQLServerQueryabel<T>(_dbSQLHelper, predicate);
            return queryabel;
        }

        public List<dynamic> Query(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            List<dynamic> list = _dbSQLHelper.ExecuteList(sql);
            return list;
        }

        public List<dynamic> Query(string sql, dynamic parameter)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            SqlParameter[] dbParameters = SQLServerStore.DynamicToSqlParameters(parameter);

            List<dynamic> list = _dbSQLHelper.ExecuteList(sql, dbParameters);
            return list;
        }
    }


}
