using HSQL.DatabaseHelper;
using HSQL.Exceptions;
using HSQL.PerformanceOptimization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Transactions;

namespace HSQL
{
    public class Database
    {
        private string _connectionString;
        private Dialect _dialect;

        /// <summary>
        /// 构建数据库对象
        /// </summary>
        /// <param name="dialect">实例数据类型</param>
        /// <param name="server">服务器地址</param>
        /// <param name="database">数据库名称</param>
        /// <param name="userId">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="pooling">是否启用线程池</param>
        /// <param name="maximumPoolSize">最大线程池连接数</param>
        /// <param name="minimumPoolSize">最小线程池连接数</param>
        public Database(Dialect dialect, string server, string database, string userId, string password, bool pooling = true, int maximumPoolSize = 100, int minimumPoolSize = 0)
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

            _dialect = dialect;

            switch (dialect)
            {
                case Dialect.MySQL:
                    _connectionString = ConnectionStringBuilder.BuildMySqlConnectionString(server, database, userId, password, pooling, maximumPoolSize, minimumPoolSize);
                    break;
                case Dialect.SQLServer:
                    _connectionString = ConnectionStringBuilder.BuildSqlConnectionString(server, database, userId, password, pooling, maximumPoolSize, minimumPoolSize);
                    break;
            }
        }

        /// <summary>
        /// 构建数据库对象
        /// </summary>
        /// <param name="dialect">实例数据类型</param>
        /// <param name="connectionString">连接字符串</param>
        public Database(Dialect dialect, string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConnectionStringIsEmptyException();

            _dialect = dialect;
            _connectionString = connectionString;
        }


        /// <summary>
        /// 执行新增操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="instance">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T>(T instance)
        {
            if (instance == null)
                throw new DataIsNullException();

            string sql = Store.BuildInsertSQL(instance);
            DbParameter[] parameters = Store.BuildDbParameter(_dialect, ExpressionBase.GetColumnList(instance));

            return BaseSQLHelper.ExecuteNonQuery(_dialect, _connectionString, sql, parameters);
        }

        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="expression">条件表达式，可理解为 SQL 语句中的 WHERE。如：WHERE age = 16 可写为 x=> x.Age = 16</param>
        /// <param name="instance">目标表达式，可理解为SQL 语句中的 SET。如：SET age = 16 , name = '张三' 可写为 new Student(){ Age = 16 , Name = "张三" }</param>
        /// <returns>是否更新成功</returns>
        public bool Update<T>(Expression<Func<T, bool>> expression, T instance)
        {
            if (expression == null)
                throw new ExpressionIsNullException();
            if (instance == null)
                throw new DataIsNullException();

            Tuple<string, DbParameter[]> result = Store.BuildUpdateSQLAndParameter(_dialect, expression, instance);

            return BaseSQLHelper.ExecuteNonQuery(_dialect, _connectionString, result.Item1, result.Item2);
        }

        /// <summary>
        /// 执行删除操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="predicate">条件表达式</param>
        /// <returns>是否删除成功</returns>
        public bool Delete<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ExpressionIsNullException();

            string sql = Store.BuildDeleteSQL(predicate);

            return BaseSQLHelper.ExecuteNonQuery(_dialect, _connectionString, sql);
        }

        /// <summary>
        /// 无条件查询
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        public Queryabel<T> Query<T>()
        {
            return Query<T>(null);
        }

        /// <summary>
        /// 无条件查询
        /// </summary>
        /// <param name="predicate">查询条件表达式</param>
        /// <typeparam name="T">类型</typeparam>
        public Queryabel<T> Query<T>(Expression<Func<T, bool>> predicate)
        {
            Queryabel<T> queryabel = new Queryabel<T>(_connectionString, _dialect, predicate);
            return queryabel;
        }

        /// <summary>
        /// 使用SQL语句查询，并得到结果集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        public List<dynamic> Query(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            IDataReader reader = BaseSQLHelper.ExecuteReader(_dialect, _connectionString, sql);

            List<dynamic> list = new List<dynamic>();
            try
            {
                while (reader.Read())
                {
                    list.Add(InstanceFactory.CreateInstance(reader));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }

            return list;
        }

        /// <summary>
        /// 使用SQL语句查询，并得到结果集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameter">参数</param>
        public List<dynamic> Query(string sql, dynamic parameter)
        {
            if (string.IsNullOrWhiteSpace(sql))
                throw new EmptySQLException();

            DbParameter[] dbParameters = Store.DynamicToDbParameters(_dialect, parameter);

            IDataReader reader = BaseSQLHelper.ExecuteReader(_dialect, _connectionString, sql, dbParameters);

            List<dynamic> list = new List<dynamic>();
            try
            {
                while (reader.Read())
                {
                    list.Add(InstanceFactory.CreateInstance(reader));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }

            return list;
        }

        /// <summary>
        /// 事务调用
        /// </summary>
        /// <param name="action"></param>
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
