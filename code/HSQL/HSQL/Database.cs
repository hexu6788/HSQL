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
