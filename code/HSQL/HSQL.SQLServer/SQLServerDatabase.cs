using HSQL.Base;
using HSQL.Exceptions;
using HSQL.Factory;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

namespace HSQL.MSSQLServer
{
    public class SQLServerDatabase : DatabaseBase, IDatabase
    {
        /// <summary>
        /// 构建数据库对象
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        public SQLServerDatabase(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConnectionStringIsEmptyException();

            _connectionString = connectionString;
        }

        /// <summary>
        /// 构建数据库对象
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="database">数据库名称</param>
        /// <param name="userId">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="pooling">是否启用线程池</param>
        /// <param name="maximumPoolSize">最大线程池连接数</param>
        /// <param name="minimumPoolSize">最小线程池连接数</param>
        public SQLServerDatabase(string server, string database, string userId, string password, bool pooling = true, int maximumPoolSize = 100, int minimumPoolSize = 0)
        {
            if (string.IsNullOrWhiteSpace(server)
                || string.IsNullOrWhiteSpace(database)
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(password))
                throw new ConnectionStringIsEmptyException();

            if (maximumPoolSize < 0)
                throw new ConnectionStringIsEmptyException($"连接池最大数不能小于零！");
            if (maximumPoolSize > 100)
                throw new ConnectionStringIsEmptyException($"连接池最大数不能大于一百！");

            if (minimumPoolSize < 0)
                throw new ConnectionStringIsEmptyException($"连接池最小数不能小于零！");
            if (minimumPoolSize > maximumPoolSize)
                throw new ConnectionStringIsEmptyException($"连接池最小数不能大于连接池最大数！");

            _connectionString = BuildConnectionString(server, database, userId, password, pooling, maximumPoolSize, minimumPoolSize);

        }

        public override string BuildConnectionString(string server, string database, string userID, string password, bool pooling, int maximumPoolSize, int minimumPoolSize)
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                DataSource = server,
                InitialCatalog = database,
                UserID = userID,
                Password = password,
                Pooling = pooling,
                MaxPoolSize = maximumPoolSize,
                MinPoolSize = minimumPoolSize
            };
            return connectionStringBuilder.ToString();
        }

        public bool Insert<T>(T instance)
        {
            if (instance == null)
                throw new DataIsNullException();

            string sql = StoreBase.BuildInsertSQL(instance);
            SqlParameter[] parameters = SQLServerStore.BuildSqlParameters(ExpressionFactory.GetColumnList(instance));

            return SQLServerHelper.ExecuteNonQuery(_connectionString, sql, parameters) > 0;
        }

        public bool Update<T>(Expression<Func<T, bool>> expression, T instance)
        {
            if (expression == null)
                throw new ExpressionIsNullException();
            if (instance == null)
                throw new DataIsNullException();

            Tuple<string, SqlParameter[]> result = SQLServerStore.BuildUpdateSQLAndParameters(expression, instance);

            return SQLServerHelper.ExecuteNonQuery(_connectionString, result.Item1, result.Item2) > 0;
        }

        public bool Delete<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ExpressionIsNullException();

            string sql = StoreBase.BuildDeleteSQL(predicate);

            return SQLServerHelper.ExecuteNonQuery(_connectionString, sql) > 0;
        }

        public IQueryabel<T> Query<T>()
        {
            return Query<T>(null);
        }

        public IQueryabel<T> Query<T>(Expression<Func<T, bool>> predicate)
        {
            IQueryabel<T> queryabel = new SQLServerQueryabel<T>(_connectionString, predicate);
            return queryabel;
        }

        public List<dynamic> Query(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            IDataReader reader = SQLServerHelper.ExecuteReader(_connectionString, sql);
            List<dynamic> list = InstanceFactory.CreateListAndDisposeReader(reader);
            return list;
        }

        public List<dynamic> Query(string sql, dynamic parameter)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            SqlParameter[] dbParameters = SQLServerStore.DynamicToSqlParameters(parameter);
            IDataReader reader = SQLServerHelper.ExecuteReader(_connectionString, sql, dbParameters);
            List<dynamic> list = InstanceFactory.CreateListAndDisposeReader(reader);
            return list;
        }
    }


}
