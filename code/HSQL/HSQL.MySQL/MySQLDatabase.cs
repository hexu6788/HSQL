using HSQL.Base;
using HSQL.Exceptions;
using HSQL.Factory;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace HSQL.MySQL
{
    public class MySQLDatabase : DatabaseBase, IDatabase
    {
        /// <summary>
        /// 构建数据库对象
        /// </summary>
        /// <param name="dialect">实例数据类型</param>
        /// <param name="connectionString">连接字符串</param>
        public MySQLDatabase(string connectionString)
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
        public MySQLDatabase(string server, string database, string userId, string password, bool pooling = true, int maximumPoolSize = 100, int minimumPoolSize = 0)
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
            MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder()
            {
                Server = server,
                Database = database,
                UserID = userID,
                Password = password,
                Pooling = pooling,
                MaximumPoolSize = (uint)maximumPoolSize,
                MinimumPoolSize = (uint)minimumPoolSize
            };
            return connectionStringBuilder.ToString();
        }

        public bool Insert<T>(T instance)
        {
            if (instance == null)
                throw new DataIsNullException();

            string sql = StoreBase.BuildInsertSQL(instance);
            MySqlParameter[] parameters = MySQLStore.BuildMySqlParameters(ExpressionFactory.GetColumnList(instance));

            return MySQLHelper.ExecuteNonQuery(_connectionString, sql, parameters) > 0;
        }

        public bool Update<T>(Expression<Func<T, bool>> expression, T instance)
        {
            if (expression == null)
                throw new ExpressionIsNullException();
            if (instance == null)
                throw new DataIsNullException();

            Tuple<string, MySqlParameter[]> result = MySQLStore.BuildUpdateSQLAndParameters(expression, instance);

            return MySQLHelper.ExecuteNonQuery(_connectionString, result.Item1, result.Item2) > 0;
        }

        public bool Delete<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ExpressionIsNullException();

            string sql = StoreBase.BuildDeleteSQL(predicate);

            return MySQLHelper.ExecuteNonQuery(_connectionString, sql) > 0;
        }

        public IQueryabel<T> Query<T>()
        {
            return Query<T>(null);
        }

        public IQueryabel<T> Query<T>(Expression<Func<T, bool>> predicate)
        {
            IQueryabel<T> queryabel = new MySQLQueryabel<T>(_connectionString, predicate);
            return queryabel;
        }

        public List<dynamic> Query(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            IDataReader reader = MySQLHelper.ExecuteReader(_connectionString, sql);
            List<dynamic> list = InstanceFactory.CreateListAndDisposeReader(reader);
            return list;
        }

        public List<dynamic> Query(string sql, dynamic parameter)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            MySqlParameter[] dbParameters = MySQLStore.DynamicToMySqlParameters(parameter);
            IDataReader reader = MySQLHelper.ExecuteReader(_connectionString, sql, dbParameters);
            List<dynamic> list = InstanceFactory.CreateListAndDisposeReader(reader);
            return list;
        }
    }


}
