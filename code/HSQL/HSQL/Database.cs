﻿using HSQL.DatabaseHelper;
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
                throw new Exception("插入数据不能为空！");

            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);
            var columnList = ExpressionBase.GetColumnList<T>(t);

            var sql = $"INSERT INTO {tableName}({string.Join(",", columnList.Select(x => x.Name))}) VALUES({string.Join(",", columnList.Select(x => string.Format("@{0}", x.Name)))});";

            switch (_dialect)
            {
                case Dialect.MySQL:
                    return MySQLHelper.ExecuteNonQuery(_connectionString, sql, columnList.Select(x => new MySqlParameter(x.Name, x.Value)).ToArray()) > 0;
                case Dialect.SQLServer:
                    return SQLServerHelper.ExecuteNonQuery(_connectionString, sql, columnList.Select(x => new SqlParameter(x.Name, x.Value)).ToArray()) > 0;
                default:
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
        /// <returns>是否新增成功</returns>
        public bool Insert<T,K>(T t,K k)
        {
            if (t == null || k == null)
                throw new Exception("插入数据不能为空！");

            var typeT = typeof(T);
            var tableNameT = ExpressionBase.GetTableName(typeT);
            var columnListT = ExpressionBase.GetColumnList<T>(t);

            var typeK = typeof(K);
            var tableNameK = ExpressionBase.GetTableName(typeK);
            var columnListK = ExpressionBase.GetColumnList<K>(k);

            var sqlList = new List<string>();
            sqlList.Add($"INSERT INTO {tableNameT}({string.Join(",", columnListT.Select(x => x.Name))}) VALUES({string.Join(",", columnListT.Select(x => string.Format("@{0}", x.Name)))});");
            sqlList.Add($"INSERT INTO {tableNameK}({string.Join(",", columnListK.Select(x => x.Name))}) VALUES({string.Join(",", columnListK.Select(x => string.Format("@{0}", x.Name)))});");

            if (_dialect == Dialect.MySQL)
            {
                var parametersList = new List<MySqlParameter[]>();
                parametersList.Add(ExpressionBase.GetColumnList<T>(t).Select(x => new MySqlParameter(x.Name, x.Value)).ToArray());
                parametersList.Add(ExpressionBase.GetColumnList<K>(k).Select(x => new MySqlParameter(x.Name, x.Value)).ToArray());

                return MySQLHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else if (_dialect == Dialect.SQLServer)
            {
                var parametersList = new List<SqlParameter[]>();
                parametersList.Add(ExpressionBase.GetColumnList<T>(t).Select(x => new SqlParameter(x.Name, x.Value)).ToArray());
                parametersList.Add(ExpressionBase.GetColumnList<K>(k).Select(x => new SqlParameter(x.Name, x.Value)).ToArray());
                
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
        /// <param name="kList">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        public bool Insert<T,K>(T t,List<K> kList)
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
            sqlList.Add($"INSERT INTO {tableNameT}({string.Join(",", columnListT.Select(x => x.Name))}) VALUES({string.Join(",", columnListT.Select(x => string.Format("@{0}", x.Name)))});");
            var sqlK = $"INSERT INTO {tableNameK}({string.Join(",", columnNameListK)}) VALUES({string.Join(",", columnNameListK.Select(x => $"@{x}"))});";
            sqlList.AddRange(kList.Select(x => sqlK).ToList());

            if (_dialect == Dialect.MySQL)
            {
                var parametersList = new List<MySqlParameter[]>();
                parametersList.Add(ExpressionBase.GetColumnList<T>(t).Select(x => new MySqlParameter(x.Name, x.Value)).ToArray());
                parametersList.AddRange(kList.Select(x => ExpressionBase.GetColumnList<K>(x).Select(x => new MySqlParameter(x.Name, x.Value)).ToArray()).ToList());

                return MySQLHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else if (_dialect == Dialect.SQLServer)
            {
                var parametersList = new List<SqlParameter[]>();
                parametersList.Add(ExpressionBase.GetColumnList<T>(t).Select(x => new SqlParameter(x.Name, x.Value)).ToArray());
                parametersList.AddRange(kList.Select(x => ExpressionBase.GetColumnList<K>(x).Select(x => new SqlParameter(x.Name, x.Value)).ToArray()).ToList());

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
            var sqlT = $"INSERT INTO {tableNameT}({string.Join(",", columnNameListT)}) VALUES({string.Join(",", columnNameListT.Select(x => $"@{x}"))});";
            var sqlK = $"INSERT INTO {tableNameK}({string.Join(",", columnNameListK)}) VALUES({string.Join(",", columnNameListK.Select(x => $"@{x}"))});";
            sqlList.AddRange(tList.Select(x => sqlT).ToList());
            sqlList.AddRange(kList.Select(x => sqlK).ToList());

            if (_dialect == Dialect.MySQL)
            {
                var parametersList = new List<MySqlParameter[]>();
                parametersList.AddRange(tList.Select(x => ExpressionBase.GetColumnList<T>(x).Select(x => new MySqlParameter(x.Name, x.Value)).ToArray()).ToList());
                parametersList.AddRange(kList.Select(x => ExpressionBase.GetColumnList<K>(x).Select(x => new MySqlParameter(x.Name, x.Value)).ToArray()).ToList());

                return MySQLHelper.ExecuteNonQueryBatch(_connectionString, sqlList, parametersList) > 0;
            }
            else if (_dialect == Dialect.SQLServer)
            {
                var parametersList = new List<SqlParameter[]>();
                parametersList.AddRange(tList.Select(x => ExpressionBase.GetColumnList<T>(x).Select(x => new SqlParameter(x.Name, x.Value)).ToArray()).ToList());
                parametersList.AddRange(kList.Select(x => ExpressionBase.GetColumnList<K>(x).Select(x => new SqlParameter(x.Name, x.Value)).ToArray()).ToList());

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
                throw new Exception("插入数据不能为空！");

            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);
            var columnNameList = ExpressionBase.GetColumnNameList(type);

            var sql = $"INSERT INTO {tableName}({string.Join(",", columnNameList)}) VALUES({string.Join(",", columnNameList.Select(x => $"@{x}"))});";
            var sqlList = list.Select(x => sql).ToList();

            switch (_dialect)
            {
                case Dialect.MySQL:
                    return MySQLHelper.ExecuteNonQueryBatch(_connectionString, sqlList, list.Select(x => ExpressionBase.GetColumnList<T>(x).Select(x => new MySqlParameter(x.Name, x.Value)).ToArray()).ToList()) > 0;
                case Dialect.SQLServer:
                    return SQLServerHelper.ExecuteNonQueryBatch(_connectionString, sqlList, list.Select(x => ExpressionBase.GetColumnList<T>(x).Select(x => new SqlParameter(x.Name, x.Value)).ToArray()).ToList()) > 0;
                default:
                    throw new Exception("未选择数据库方言！");
            }
        }

        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="selectPpredicate">条件表达式，可理解为 SQL 语句中的 WHERE。如：WHERE age = 16 可写为 x=> x.Age = 16</param>
        /// <param name="instance">目标表达式，可理解为SQL 语句中的 SET。如：SET age = 16 , name = '张三' 可写为 new Student(){ Age = 16 , Name = "张三" }</param>
        /// <returns>是否更新成功</returns>
        public bool Update<T>(Expression<Func<T, bool>> selectPpredicate, T instance)
        {
            if (selectPpredicate == null)
                throw new Exception("更新筛选条件不能为空！");
            if (instance == null)
                throw new Exception("更新值不能为空！");

            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);
            var columnList = ExpressionBase.GetColumnListWithOutNull<T>(instance);

            var where = ExpressionToWhereSql.ToWhereString(selectPpredicate);
            var sql = $"UPDATE {tableName} SET {string.Join(" , ", columnList.Select(x => string.Format("{0} = @{1}", x.Name, x.Name)))} WHERE {where};";

            switch (_dialect)
            {
                case Dialect.MySQL:
                    return MySQLHelper.ExecuteNonQuery(_connectionString, sql, columnList.Select(x => new MySqlParameter(x.Name, x.Value)).ToArray()) > 0;
                case Dialect.SQLServer:
                    return SQLServerHelper.ExecuteNonQuery(_connectionString, sql, columnList.Select(x => new SqlParameter(x.Name, x.Value)).ToArray()) > 0;
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
            var where = ExpressionToWhereSql.ToWhereString(predicate);
            if (string.IsNullOrWhiteSpace(where))
                throw new Exception("删除时必须包含删除条件！");

            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);

            var sql = $"DELETE FROM {tableName} WHERE {where};";

            switch (_dialect)
            {
                case Dialect.MySQL:
                    return MySQLHelper.ExecuteNonQuery(_connectionString, sql) > 0;
                case Dialect.SQLServer:
                    return SQLServerHelper.ExecuteNonQuery(_connectionString, sql) > 0;
                default:
                    throw new Exception("未选择数据库方言！");
            }
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
