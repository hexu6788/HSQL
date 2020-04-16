using HSQL.Attribute;
using HSQL.Const;
using HSQL.DatabaseHelper;
using HSQL.PerformanceOptimization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HSQL
{
    public class Queryabel<T>
    {
        private string _connectionString;
        private Dialect _dialect;
        private Expression<Func<T, bool>> _predicate;

        private string _orderField = string.Empty;
        private string _orderBy = string.Empty;

        internal void SetOrderField(string field)
        {
            this._orderField = field;
        }
        internal void SetOrderBy(string orderBy)
        {
            this._orderBy = orderBy;
        }

        public Queryabel(string connectionString, Dialect dialect, Expression<Func<T, bool>> predicate)
        {
            _connectionString = connectionString;
            _dialect = dialect;
            _predicate = predicate;
        }

        public Queryabel<T> AddCondition(Expression<Func<T, bool>> condition)
        {
            if (_predicate == null)
                _predicate = condition;
            else
                _predicate = _predicate.AndAlso(condition);

            return this;
        }
        public Queryabel<T> AndOrCondition(Expression<Func<T, bool>> condition)
        {
            if (_predicate == null)
                _predicate = condition;
            else
                _predicate = _predicate.OrElse(condition);

            return this;
        }

        public List<T> ToList()
        {
            var type = typeof(T);
            var propertyInfoList = Store.GetPropertyInfoList(type);
            var tableName = ExpressionBase.GetTableName(type);
            var columnJoinString = string.Join(",", ExpressionBase.GetColumnNameList(type));
            var whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            var sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");
            
            IDataReader reader = null;
            switch (_dialect)
            {
                case Dialect.MySQL:
                    if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                        sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

                    sqlStringBuilder.Append(";");
                    reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                case Dialect.SQLServer:
                    if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                        sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
                    else
                        sqlStringBuilder.Append($" ORDER BY id");

                    sqlStringBuilder.Append($" OFFSET 1 ROWS FETCH NEXT 9999999 ROWS ONLY;");
                    reader = SQLServerHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                default:
                    throw new Exception("未选择数据库方言！");
            }

            var list = new List<T>();
            try
            {
                while (reader.Read())
                {
                    list.Add(InstanceFactory.CreateInstance<T>(reader, propertyInfoList));
                }
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }

            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize)
        {
            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);
            var columnJoinString = string.Join(",", ExpressionBase.GetColumnNameList(type));
            var propertyInfoList = Store.GetPropertyInfoList(type);
            var pageStart = (pageIndex - 1) * pageSize;
            var whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            var sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            IDataReader reader = null;
            switch (_dialect)
            {
                case Dialect.MySQL:
                    if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                        sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

                    sqlStringBuilder.Append($" LIMIT {pageStart},{pageSize};");
                    reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                case Dialect.SQLServer:
                    if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                        sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
                    else
                        sqlStringBuilder.Append($" ORDER BY id");


                    sqlStringBuilder.Append($" OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");
                    reader = SQLServerHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                default:
                    throw new Exception("未选择数据库方言！");
            }

            var list = new List<T>();
            try
            {
                while (reader.Read())
                {
                    list.Add(InstanceFactory.CreateInstance<T>(reader, propertyInfoList));
                }
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize,out int total,out int totalPage)
        {
            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);
            var columnJoinString = string.Join(",", ExpressionBase.GetColumnNameList(type));
            var propertyInfoList = Store.GetPropertyInfoList(type);
            var pageStart = (pageIndex - 1) * pageSize;

            var whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            var sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            var pageWhereString = $"SELECT COUNT(*) FROM {tableName};";

            IDataReader reader = null;
            switch (_dialect)
            {
                case Dialect.MySQL:
                    total = Convert.ToInt32(MySQLHelper.ExecuteScalar(_connectionString, pageWhereString));
                    if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                        sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

                    sqlStringBuilder.Append($" LIMIT {pageStart},{pageSize};");
                    reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                case Dialect.SQLServer:
                    total = Convert.ToInt32(SQLServerHelper.ExecuteScalar(_connectionString, pageWhereString));
                    if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                        sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
                    else
                        sqlStringBuilder.Append($" ORDER BY id");

                    sqlStringBuilder.Append($" OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");
                    reader = SQLServerHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                default:
                    throw new Exception("未选择数据库方言！");
            }
            totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

            var list = new List<T>();
            try
            {
                while (reader.Read())
                {
                    list.Add(InstanceFactory.CreateInstance<T>(reader, propertyInfoList));
                }
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }

            return list;
        }

        public T FirstOrDefault()
        {
            var type = typeof(T);
            var propertyInfoList = Store.GetPropertyInfoList(type);
            var tableName = ExpressionBase.GetTableName(type);
            var columnJoinString = string.Join(",", ExpressionBase.GetColumnNameList(type));
            var whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            var sqlWhereStringBuilder = new StringBuilder();
            sqlWhereStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlWhereStringBuilder.Append($" WHERE {whereString}");

            IDataReader reader = null;
            
            switch (_dialect)
            {
                case Dialect.MySQL:
                    sqlWhereStringBuilder.Append($" LIMIT 0,1;");
                    reader = MySQLHelper.ExecuteReader(_connectionString, sqlWhereStringBuilder.ToString());
                    break;
                case Dialect.SQLServer:
                    sqlWhereStringBuilder.Append($" ORDER BY id OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;");
                    reader = SQLServerHelper.ExecuteReader(_connectionString, sqlWhereStringBuilder.ToString());
                    break;
                default:
                    throw new Exception("未选择数据库方言！");
            }

            T instance = default(T);
            try
            {
                if (reader.Read())
                    instance = InstanceFactory.CreateInstance<T>(reader, propertyInfoList);
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }
            return instance;
        }
    }
}
