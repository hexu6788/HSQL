using HSQL.Base;
using HSQL.Exceptions;
using HSQL.Factory;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HSQL.MySQL
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
        public DbContext(string server, string database, string userId, string password, int poolSize = 1)
        {
            if (string.IsNullOrWhiteSpace(server)
                || string.IsNullOrWhiteSpace(database)
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(password))
                throw new ConnectionStringIsEmptyException();

            if (poolSize <= 0)
                throw new ConnectionStringIsEmptyException($"连接数最小为一个！");

            string connectionString = BuildConnectionString(server, database, userId, password);
            MySQLConnectionPools.Init(connectionString, poolSize);

            _dbSQLHelper = new MySQLHelper();
        }

        public override string BuildConnectionString(string server, string database, string userID, string password)
        {
            MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder()
            {
                Server = server,
                Database = database,
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
            MySqlParameter[] parameters = MySQLStore.BuildMySqlParameters(ExpressionFactory.GetColumnList(instance));

            return _dbSQLHelper.ExecuteNonQuery(sql, parameters) > 0;
        }

        public bool Update<T>(Expression<Func<T, bool>> expression, T instance)
        {
            if (expression == null)
                throw new ExpressionIsNullException();
            if (instance == null)
                throw new DataIsNullException();

            Tuple<string, MySqlParameter[]> result = MySQLStore.BuildUpdateSQLAndParameters(expression, instance);

            return _dbSQLHelper.ExecuteNonQuery(result.Item1, result.Item2) > 0;
        }

        public bool Delete<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ExpressionIsNullException();

            string sql = StoreBase.BuildDeleteSQL(predicate);

            return _dbSQLHelper.ExecuteNonQuery(sql) > 0;
        }

        public IQueryabel<T> Query<T>()
        {
            return Query<T>(null);
        }

        public IQueryabel<T> Query<T>(Expression<Func<T, bool>> predicate)
        {
            IQueryabel<T> queryabel = new MySQLQueryabel<T>(_dbSQLHelper, predicate);
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

            MySqlParameter[] dbParameters = MySQLStore.DynamicToMySqlParameters(parameter);
            List<dynamic> list = _dbSQLHelper.ExecuteList(sql, dbParameters);
            return list;
        }
    }


}
