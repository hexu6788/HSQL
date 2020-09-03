using HSQL.Base;
using HSQL.Exceptions;
using HSQL.Factory;
using HSQL.Model;
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
        /// <param name="consolePrintSql">是否在控制台输出Sql语句</param>
        public DbContext(string server, string database, string userId, string password, int poolSize = 3, bool consolePrintSql = false)
        {
            if (string.IsNullOrWhiteSpace(server)
                || string.IsNullOrWhiteSpace(database)
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(password))
                throw new ConnectionStringIsEmptyException();

            if (consolePrintSql)
                _consolePrintSql = true;

            if (poolSize <= 0)
                throw new ConnectionStringIsEmptyException($"连接数最小为一个！");

            string connectionString = BuildConnectionString(server, database, userId, password);
            MySQLConnectionPools.Init(connectionString, poolSize);
            _dbSQLHelper = new MySQLHelper(connectionString);
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
            var parameters = StoreBase.BuildParameters(ExpressionFactory.GetColumnList(instance));

            bool isNewConnection = TransactionIsOpen.Value;
            return _dbSQLHelper.ExecuteNonQuery(isNewConnection, _consolePrintSql, sql, _dbSQLHelper.Convert(parameters)) > 0;
        }

        public bool Update<T>(Expression<Func<T, bool>> expression, T instance)
        {
            if (expression == null)
                throw new ExpressionIsNullException();
            if (instance == null)
                throw new DataIsNullException();

            var result = StoreBase.BuildUpdateSQLAndParameters(expression, instance);

            bool isNewConnection = TransactionIsOpen.Value;
            return _dbSQLHelper.ExecuteNonQuery(isNewConnection, _consolePrintSql, result.Item1, _dbSQLHelper.Convert(result.Item2)) > 0;
        }

        public bool Delete<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ExpressionIsNullException();

            Sql sql = StoreBase.BuildDeleteSQL(predicate);

            bool isNewConnection = TransactionIsOpen.Value;
            return _dbSQLHelper.ExecuteNonQuery(isNewConnection, _consolePrintSql, sql.CommandText, _dbSQLHelper.Convert(sql.Parameters)) > 0;
        }

        public IQueryabel<T> Query<T>()
        {
            return Query<T>(null);
        }

        public IQueryabel<T> Query<T>(Expression<Func<T, bool>> predicate)
        {
            bool isNewConnection = TransactionIsOpen.Value;
            IQueryabel<T> queryabel = new MySQLQueryabel<T>(_dbSQLHelper, _consolePrintSql, predicate);
            return queryabel;
        }

        public List<dynamic> Query(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            List<dynamic> list = _dbSQLHelper.ExecuteList(_consolePrintSql, sql);
            return list;
        }

        public List<dynamic> Query(string sql, dynamic parameter)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            var parameters = StoreBase.DynamicToParameters(parameter);
            List<dynamic> list = _dbSQLHelper.ExecuteList(_consolePrintSql, sql, _dbSQLHelper.Convert(parameters));
            return list;
        }




    }


}
