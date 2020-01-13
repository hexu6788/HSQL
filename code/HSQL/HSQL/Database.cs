using HSQL.DatabaseHelper;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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

        public Queryabel<T> Query<T>(Expression<Func<T, bool>> predicate)
        {
            var queryabel = new Queryabel<T>(_connectionString, _dialect, predicate);
            return queryabel;
        }
    }
}
