using HSQL.DatabaseHelper;
using HSQL.Exceptions;
using HSQL.PerformanceOptimization;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

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

            var columnList = ExpressionBase.GetColumnList<T>(t);

            var sql = Store.BuildInsertSQL(ExpressionBase.GetTableName(typeof(T)), columnList);

            return BaseSQLHelper.ExecuteNonQuery(_dialect, _connectionString, sql, Store.BuildDbParameter(_dialect, columnList));
        }

        /// <summary>
        /// 执行新增操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <typeparam name="K">类型</typeparam>
        /// <param name="t">要新增的实例</param>
        /// <param name="k">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T, K>(T t, K k)
        {
            if (t == null || k == null)
                throw new DataIsNullException();

            var typeT = typeof(T);
            var tableNameT = ExpressionBase.GetTableName(typeT);
            var columnListT = ExpressionBase.GetColumnList<T>(t);

            var typeK = typeof(K);
            var tableNameK = ExpressionBase.GetTableName(typeK);
            var columnListK = ExpressionBase.GetColumnList<K>(k);

            var sqlList = new List<string>();
            sqlList.Add(Store.BuildInsertSQL(tableNameT, columnListT));
            sqlList.Add(Store.BuildInsertSQL(tableNameK, columnListK));


            return BaseSQLHelper.ExecuteNonQueryBatch(_dialect, _connectionString, sqlList, new List<DbParameter[]>()
            {
                Store.BuildSqlParameter(columnListT),
                Store.BuildSqlParameter(columnListK)
            });
        }

        /// <summary>
        /// 执行新增操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <typeparam name="K">类型</typeparam>
        /// <param name="t">要新增的实例</param>
        /// <param name="k">要新增的实例</param>
        /// <param name="v">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T, K, V>(T t, K k, V v)
        {
            if (t == null || k == null || v == null)
                throw new Exception("插入数据不能为空！");

            var typeT = typeof(T);
            var tableNameT = ExpressionBase.GetTableName(typeT);
            var columnListT = ExpressionBase.GetColumnList<T>(t);

            var typeK = typeof(K);
            var tableNameK = ExpressionBase.GetTableName(typeK);
            var columnListK = ExpressionBase.GetColumnList<K>(k);

            var typeV = typeof(V);
            var tableNameV = ExpressionBase.GetTableName(typeV);
            var columnListV = ExpressionBase.GetColumnList<V>(v);

            var sqlList = new List<string>();
            sqlList.Add(Store.BuildInsertSQL(tableNameT, columnListT));
            sqlList.Add(Store.BuildInsertSQL(tableNameK, columnListK));
            sqlList.Add(Store.BuildInsertSQL(tableNameV, columnListV));

            var parametersList = new List<DbParameter[]>();
            parametersList.Add(Store.BuildMySqlParameter(columnListT));
            parametersList.Add(Store.BuildMySqlParameter(columnListK));
            parametersList.Add(Store.BuildMySqlParameter(columnListV));

            return BaseSQLHelper.ExecuteNonQueryBatch(_dialect, _connectionString, sqlList, parametersList);
        }

        /// <summary>
        /// 执行新增操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <typeparam name="K">类型</typeparam>
        /// <param name="t">要新增的实例</param>
        /// <param name="kList">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T, K>(T t, List<K> kList)
        {
            if (t == null || kList == null || kList.Count <= 0 || kList.Count(x => x == null) > 0)
                throw new Exception("插入数据不能为空！");

            var typeT = typeof(T);
            var tableNameT = ExpressionBase.GetTableName(typeT);
            var columnListT = ExpressionBase.GetColumnList<T>(t);

            var typeK = typeof(K);
            var tableNameK = ExpressionBase.GetTableName(typeK);
            var columnNameListK = ExpressionBase.GetColumnNameList(typeK);

            var sqlList = new List<string>();
            sqlList.Add(Store.BuildInsertSQL(tableNameT, columnListT));
            var sqlK = Store.BuildInsertSQL(tableNameK, columnNameListK);
            sqlList.AddRange(kList.Select(x => sqlK).ToList());


            if (_dialect == Dialect.MySQL)
            {
                var parametersList = new List<DbParameter[]>();
                parametersList.Add(Store.BuildMySqlParameter(columnListT));
                parametersList.AddRange(kList.Select(x => Store.BuildMySqlParameter(ExpressionBase.GetColumnList<K>(x))).ToList());

                return BaseSQLHelper.ExecuteNonQueryBatch(_dialect, _connectionString, sqlList, parametersList);
            }
            else if (_dialect == Dialect.SQLServer)
            {
                var parametersList = new List<DbParameter[]>();
                parametersList.Add(Store.BuildSqlParameter(columnListT));
                parametersList.AddRange(kList.Select(x => Store.BuildSqlParameter(ExpressionBase.GetColumnList<K>(x))).ToList());

                return BaseSQLHelper.ExecuteNonQueryBatch(_dialect, _connectionString, sqlList, parametersList);
            }
            else
            {
                throw new NoDialectException();
            }
        }


        /// <summary>
        /// 执行新增操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <typeparam name="K">类型</typeparam>
        /// <param name="t">要新增的实例</param>
        /// <param name="k">要新增的实例</param>
        /// <param name="vList">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T, K, V>(T t, K k, List<V> vList)
        {
            if (t == null || k == null || vList == null || vList.Count <= 0 || vList.Count(x => x == null) > 0)
                throw new Exception("插入数据不能为空！");

            var typeT = typeof(T);
            var tableNameT = ExpressionBase.GetTableName(typeT);
            var columnListT = ExpressionBase.GetColumnList<T>(t);

            var typeK = typeof(K);
            var tableNameK = ExpressionBase.GetTableName(typeK);
            var columnListK = ExpressionBase.GetColumnList<K>(k);

            var typeV = typeof(V);
            var tableNameV = ExpressionBase.GetTableName(typeV);
            var columnNameListV = ExpressionBase.GetColumnNameList(typeV);

            var sqlList = new List<string>();
            sqlList.Add(Store.BuildInsertSQL(tableNameT, columnListT));
            sqlList.Add(Store.BuildInsertSQL(tableNameK, columnListK));
            var sqlV = Store.BuildInsertSQL(tableNameV, columnNameListV);
            sqlList.AddRange(vList.Select(x => sqlV).ToList());

            if (_dialect == Dialect.MySQL)
            {
                var parametersList = new List<MySqlParameter[]>();
                parametersList.Add(Store.BuildMySqlParameter(columnListT));
                parametersList.Add(Store.BuildMySqlParameter(columnListK));
                parametersList.AddRange(vList.Select(x => Store.BuildMySqlParameter(ExpressionBase.GetColumnList<V>(x))).ToList());

                return MySQLHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else if (_dialect == Dialect.SQLServer)
            {
                var parametersList = new List<SqlParameter[]>();
                parametersList.Add(Store.BuildSqlParameter(columnListT));
                parametersList.Add(Store.BuildSqlParameter(columnListK));
                parametersList.AddRange(vList.Select(x => Store.BuildSqlParameter(ExpressionBase.GetColumnList<V>(x))).ToList());

                return SQLServerHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else
            {
                throw new Exception("未选择数据库方言！");
            }
        }

        /// <summary>
        /// 执行新增操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <typeparam name="K">类型</typeparam>
        /// <param name="t">要新增的实例</param>
        /// <param name="k">要新增的实例</param>
        /// <param name="vList">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T, K, V, M>(T t, K k, List<V> vList, List<M> mList)
        {
            if (t == null || k == null
                || vList == null || vList.Count <= 0 || vList.Count(x => x == null) > 0
                || mList == null || mList.Count <= 0 || mList.Count(x => x == null) > 0)
                throw new Exception("插入数据不能为空！");

            var typeT = typeof(T);
            var tableNameT = ExpressionBase.GetTableName(typeT);
            var columnListT = ExpressionBase.GetColumnList<T>(t);

            var typeK = typeof(K);
            var tableNameK = ExpressionBase.GetTableName(typeK);
            var columnListK = ExpressionBase.GetColumnList<K>(k);

            var typeV = typeof(V);
            var tableNameV = ExpressionBase.GetTableName(typeV);
            var columnNameListV = ExpressionBase.GetColumnNameList(typeV);

            var typeM = typeof(M);
            var tableNameM = ExpressionBase.GetTableName(typeM);
            var columnNameListM = ExpressionBase.GetColumnNameList(typeM);

            var sqlList = new List<string>();
            sqlList.Add(Store.BuildInsertSQL(tableNameT, columnListT));
            sqlList.Add(Store.BuildInsertSQL(tableNameK, columnListK));
            var sqlV = Store.BuildInsertSQL(tableNameV, columnNameListV);
            var sqlM = Store.BuildInsertSQL(tableNameM, columnNameListM);
            sqlList.AddRange(vList.Select(x => sqlV).ToList());
            sqlList.AddRange(mList.Select(x => sqlM).ToList());

            if (_dialect == Dialect.MySQL)
            {
                var parametersList = new List<MySqlParameter[]>();
                parametersList.Add(Store.BuildMySqlParameter(columnListT));
                parametersList.Add(Store.BuildMySqlParameter(columnListK));
                parametersList.AddRange(vList.Select(x => Store.BuildMySqlParameter(ExpressionBase.GetColumnList<V>(x))).ToList());
                parametersList.AddRange(mList.Select(x => Store.BuildMySqlParameter(ExpressionBase.GetColumnList<M>(x))).ToList());

                return MySQLHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else if (_dialect == Dialect.SQLServer)
            {
                var parametersList = new List<SqlParameter[]>();
                parametersList.Add(Store.BuildSqlParameter(columnListT));
                parametersList.Add(Store.BuildSqlParameter(columnListK));
                parametersList.AddRange(vList.Select(x => Store.BuildSqlParameter(ExpressionBase.GetColumnList<V>(x))).ToList());
                parametersList.AddRange(mList.Select(x => Store.BuildSqlParameter(ExpressionBase.GetColumnList<M>(x))).ToList());

                return SQLServerHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else
            {
                throw new Exception("未选择数据库方言！");
            }
        }

        /// <summary>
        /// 执行新增操作
        /// </summary>
        /// <typeparam name="K">类型</typeparam>
        /// <param name="t">类型</param>
        /// <param name="tList">要新增的实例</param>
        /// <param name="kList">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T, K>(List<T> tList, List<K> kList)
        {
            if (tList == null || tList.Count <= 0 || tList.Count(x => x == null) > 0 || kList == null || kList.Count <= 0 || kList.Count(x => x == null) > 0)
                throw new Exception("插入数据不能为空！");

            var typeT = typeof(T);
            var tableNameT = ExpressionBase.GetTableName(typeT);
            var columnNameListT = ExpressionBase.GetColumnNameList(typeT);

            var typeK = typeof(K);
            var tableNameK = ExpressionBase.GetTableName(typeK);
            var columnNameListK = ExpressionBase.GetColumnNameList(typeK);

            var sqlList = new List<string>();
            var sqlT = Store.BuildInsertSQL(tableNameT, columnNameListT);
            var sqlK = Store.BuildInsertSQL(tableNameK, columnNameListK);
            sqlList.AddRange(tList.Select(x => sqlT).ToList());
            sqlList.AddRange(kList.Select(x => sqlK).ToList());

            if (_dialect == Dialect.MySQL)
            {
                var parametersList = new List<MySqlParameter[]>();
                parametersList.AddRange(tList.Select(x => Store.BuildMySqlParameter(ExpressionBase.GetColumnList<T>(x))).ToList());
                parametersList.AddRange(kList.Select(x => Store.BuildMySqlParameter(ExpressionBase.GetColumnList<K>(x))).ToList());

                return MySQLHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else if (_dialect == Dialect.SQLServer)
            {
                var parametersList = new List<SqlParameter[]>();
                parametersList.AddRange(tList.Select(x => Store.BuildSqlParameter(ExpressionBase.GetColumnList<T>(x))).ToList());
                parametersList.AddRange(kList.Select(x => Store.BuildSqlParameter(ExpressionBase.GetColumnList<K>(x))).ToList());

                return SQLServerHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else
            {
                throw new Exception("未选择数据库方言！");
            }
        }

        /// <summary>
        /// 执行批量新增操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="list">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T>(List<T> list)
        {
            if (list == null || list.Count <= 0 || list.Count(x => x == null) > 0)
                throw new DataIsNullException();

            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);
            var columnNameList = ExpressionBase.GetColumnNameList(type);

            var sql = Store.BuildInsertSQL(tableName, columnNameList);
            var sqls = list.Select(x => sql).ToList();

            var parametersList = list.Select(x => Store.BuildDbParameter(_dialect, ExpressionBase.GetColumnList<T>(x))).ToList();
            return BaseSQLHelper.ExecuteNonQueryBatch(_dialect, _connectionString, sqls, parametersList);
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

            var tableName = ExpressionBase.GetTableName(typeof(T));
            var columnList = ExpressionBase.GetColumnListWithOutNull<T>(instance);
            var where = ExpressionToWhereSql.ToWhereString(expression);

            var sql = Store.BuildUpdateSQL(tableName, columnList, where);

            return BaseSQLHelper.ExecuteNonQuery(_dialect, _connectionString, sql, Store.BuildDbParameter(_dialect, columnList));
        }

        /// <summary>
        /// 执行更新和新增操作
        /// </summary>
        /// <typeparam name="T">修改类型</typeparam>
        /// <typeparam name="K">新增类型</typeparam>
        /// <param name="predicateT">条件表达式，可理解为 SQL 语句中的 WHERE。如：WHERE age = 16 可写为 x=> x.Age = 16</param>
        /// <param name="t">目标表达式，可理解为SQL 语句中的 SET。如：SET age = 16 , name = '张三' 可写为 new Student(){ Age = 16 , Name = "张三" }</param>
        /// <param name="k">新增实例</param>
        /// <returns>是否更新和新增成功</returns>
        public bool UpdateAndInsert<T, K>(Expression<Func<T, bool>> predicateT, T t, K k)
        {
            if (predicateT == null)
                throw new Exception("更新筛选条件不能为空！");
            if (t == null)
                throw new Exception("更新值不能为空！");
            if (k == null)
                throw new Exception("插入数据不能为空！");

            var columnListT = ExpressionBase.GetColumnListWithOutNull<T>(t);
            var columnListK = ExpressionBase.GetColumnList<K>(k);

            var sqlT = Store.BuildUpdateSQL(ExpressionBase.GetTableName(typeof(T)), columnListT, ExpressionToWhereSql.ToWhereString(predicateT));
            var sqlK = Store.BuildInsertSQL(ExpressionBase.GetTableName(typeof(K)), columnListK);

            switch (_dialect)
            {
                case Dialect.MySQL:
                    return MySQLHelper.ExecuteNonQueryBatch(_connectionString, new List<string>() { sqlT, sqlK }, new List<MySqlParameter[]>() { Store.BuildMySqlParameter(columnListT), Store.BuildMySqlParameter(columnListK) }) > 0;
                case Dialect.SQLServer:
                    return SQLServerHelper.ExecuteNonQueryBatch(_connectionString, new List<string>() { sqlT, sqlK }, new List<SqlParameter[]>() { Store.BuildSqlParameter(columnListT), Store.BuildSqlParameter(columnListK) }) > 0;
                default:
                    throw new Exception("未选择数据库方言！");
            }
        }


        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="selectPpredicateT">条件表达式，可理解为 SQL 语句中的 WHERE。如：WHERE age = 16 可写为 x=> x.Age = 16</param>
        /// <param name="instanceT">目标表达式，可理解为SQL 语句中的 SET。如：SET age = 16 , name = '张三' 可写为 new Student(){ Age = 16 , Name = "张三" }</param>
        /// <param name="selectPpredicateK">条件表达式</param>
        /// <param name="instanceK">目标表达式</param>
        /// <returns>是否更新成功</returns>
        public bool Update<T, K>(Expression<Func<T, bool>> selectPpredicateT, T instanceT, Expression<Func<K, bool>> selectPpredicateK, K instanceK)
        {
            if (selectPpredicateT == null || selectPpredicateK == null)
                throw new Exception("更新筛选条件不能为空！");
            if (instanceT == null || instanceK == null)
                throw new Exception("更新值不能为空！");

            var columnListT = ExpressionBase.GetColumnListWithOutNull<T>(instanceT);
            var columnListK = ExpressionBase.GetColumnListWithOutNull<K>(instanceK);

            var sqlT = Store.BuildUpdateSQL(ExpressionBase.GetTableName(typeof(T)), columnListT, ExpressionToWhereSql.ToWhereString(selectPpredicateT));
            var sqlK = Store.BuildUpdateSQL(ExpressionBase.GetTableName(typeof(K)), columnListK, ExpressionToWhereSql.ToWhereString(selectPpredicateK));

            switch (_dialect)
            {
                case Dialect.MySQL:
                    return MySQLHelper.ExecuteNonQueryBatch(_connectionString, new List<string>() { sqlT, sqlK }, new List<MySqlParameter[]>() { Store.BuildMySqlParameter(columnListT), Store.BuildMySqlParameter(columnListK) }) > 0;
                case Dialect.SQLServer:
                    return SQLServerHelper.ExecuteNonQueryBatch(_connectionString, new List<string>() { sqlT, sqlK }, new List<SqlParameter[]>() { Store.BuildSqlParameter(columnListT), Store.BuildSqlParameter(columnListK) }) > 0;
                default:
                    throw new Exception("未选择数据库方言！");
            }
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

            var where = ExpressionToWhereSql.ToWhereString(predicate);
            if (string.IsNullOrWhiteSpace(where))
                throw new ExpressionIsNullException();

            var tableName = ExpressionBase.GetTableName(typeof(T));

            var sql = Store.BuildDeleteSQL(tableName, where);

            return BaseSQLHelper.ExecuteNonQuery(_dialect, _connectionString, sql);
        }

        /// <summary>
        /// 执行删除操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="predicateT">条件表达式</param>
        /// <param name="predicateK">条件表达式</param>
        /// <returns>是否删除成功</returns>
        public bool Delete<T, K>(Expression<Func<T, bool>> predicateT, Expression<Func<K, bool>> predicateK)
        {
            var whereT = ExpressionToWhereSql.ToWhereString(predicateT);
            var whereK = ExpressionToWhereSql.ToWhereString(predicateK);
            if (string.IsNullOrWhiteSpace(whereT) || string.IsNullOrWhiteSpace(whereK))
                throw new ExpressionIsNullException();

            var tableNameT = ExpressionBase.GetTableName(typeof(T));
            var tableNameK = ExpressionBase.GetTableName(typeof(K));

            var sqlT = Store.BuildDeleteSQL(tableNameT, whereT);
            var sqlK = Store.BuildDeleteSQL(tableNameK, whereK);

            return BaseSQLHelper.ExecuteNonQueryBatch(_dialect, _connectionString, new List<string>()
            {
                sqlT,
                sqlK
            });
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
            var queryabel = new Queryabel<T>(_connectionString, _dialect, predicate);
            return queryabel;
        }







    }


}
