using HSQL.Attribute;
using HSQL.Const;
using HSQL.DatabaseHelper;
using HSQL.Extensions;
using HSQL.PerformanceOptimization;
using System;
using System.Collections.Generic;
using System.Data;
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
            sqlStringBuilder.Append(";");

            IDataReader reader = null;
            switch (_dialect)
            {
                case Dialect.MySQL:
                    reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                case Dialect.SQLServer:
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
                    list.Add(CreateInstance<T>(reader, propertyInfoList));
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
                    sqlStringBuilder.Append($" LIMIT {pageStart},{pageSize};");
                    reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                case Dialect.SQLServer:
                    sqlStringBuilder.Append($" ORDER BY id OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");
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
                    list.Add(CreateInstance<T>(reader, propertyInfoList));
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

            var sqlWhereStringBuilder = new StringBuilder();
            sqlWhereStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlWhereStringBuilder.Append($" WHERE {whereString}");

            var pageWhereString = $"SELECT COUNT(*) FROM {tableName};";

            IDataReader reader = null;
            switch (_dialect)
            {
                case Dialect.MySQL:
                    total = Convert.ToInt32(MySQLHelper.ExecuteScalar(_connectionString, pageWhereString));
                    sqlWhereStringBuilder.Append($" LIMIT {pageStart},{pageSize};");
                    reader = MySQLHelper.ExecuteReader(_connectionString, sqlWhereStringBuilder.ToString());
                    break;
                case Dialect.SQLServer:
                    total = Convert.ToInt32(SQLServerHelper.ExecuteScalar(_connectionString, pageWhereString));
                    sqlWhereStringBuilder.Append($" ORDER BY id OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");
                    reader = SQLServerHelper.ExecuteReader(_connectionString, sqlWhereStringBuilder.ToString());
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
                    list.Add(CreateInstance<T>(reader, propertyInfoList));
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
                    instance = CreateInstance<T>(reader, propertyInfoList);
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }
            return instance;
        }


        private T CreateInstance<T>(IDataReader reader, List<PropertyInfo> propertyInfoList) 
        {
            T instance = Activator.CreateInstance<T>();
            foreach (var property in propertyInfoList)
            {
                var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                if (attributes.Length > 0)
                {
                    var key = ((ColumnAttribute)attributes[0]).Name;
                    property.SetValue(instance, reader[key]);
                }
            }
            return instance;
        }
    }
}
