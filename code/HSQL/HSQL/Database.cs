using HSQL.DatabaseHelper;
using HSQL.Exceptions;
using HSQL.Model;
using HSQL.PerformanceOptimization;
using System;
using System.Collections.Generic;
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
        /// <param name="t">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T>(T t)
        {
            if (t == null)
                throw new DataIsNullException();

            List<Column> columnList = ExpressionBase.GetColumnList<T>(t);

            string sql = Store.BuildInsertSQL(ExpressionBase.GetTableName(typeof(T)), columnList);

            return BaseSQLHelper.ExecuteNonQuery(_dialect, _connectionString, sql, Store.BuildDbParameter(_dialect, columnList));
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

            string tableName = ExpressionBase.GetTableName(typeof(T));
            List<Column> columnList = ExpressionBase.GetColumnListWithOutNull<T>(instance);
            string where = ExpressionToWhereSql.ToWhereString(expression);

            string sql = Store.BuildUpdateSQL(tableName, columnList, where);

            return BaseSQLHelper.ExecuteNonQuery(_dialect, _connectionString, sql, Store.BuildDbParameter(_dialect, columnList));
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

            string where = ExpressionToWhereSql.ToWhereString(predicate);
            if (string.IsNullOrWhiteSpace(where))
                throw new ExpressionIsNullException();

            string sql = Store.BuildDeleteSQL(ExpressionBase.GetTableName(typeof(T)), where);

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
        /// 事务调用
        /// </summary>
        /// <param name="action"></param>
        public void Transaction(Action action)
        {
            using (var scope = new TransactionScope())
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
