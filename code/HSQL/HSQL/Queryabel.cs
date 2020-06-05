using HSQL.Attribute;
using HSQL.Const;
using HSQL.DatabaseHelper;
using HSQL.Exceptions;
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

        public int Count()
        {
            string whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            StringBuilder countStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {Store.GetTableName(typeof(T))}");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                countStringBuilder.Append($" WHERE {whereString}");
            }

            int total = Convert.ToInt32(BaseSQLHelper.ExecuteScalar(_dialect, _connectionString, countStringBuilder.ToString()));
            return total;
        }

        public bool Exists()
        {
            return Count() > 0;
        }

        public List<T> ToList()
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = Store.GetPropertyInfoList(type);
            string tableName = Store.GetTableName(type);
            string columnJoinString = string.Join(",", Store.GetColumnNameList(type));
            string whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
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

            List<T> list = new List<T>();
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
            Type type = typeof(T);
            string tableName = Store.GetTableName(type);
            string columnJoinString = string.Join(",", Store.GetColumnNameList(type));
            List<PropertyInfo> propertyInfoList = Store.GetPropertyInfoList(type);
            int pageStart = (pageIndex - 1) * pageSize;
            string whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
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

            List<T> list = new List<T>();
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
            Type type = typeof(T);
            string tableName = Store.GetTableName(type);
            string columnJoinString = string.Join(",", Store.GetColumnNameList(type));
            List<PropertyInfo> propertyInfoList = Store.GetPropertyInfoList(type);
            int pageStart = (pageIndex - 1) * pageSize;

            string whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {columnJoinString} FROM {tableName}");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableName}");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                sqlStringBuilder.Append($" WHERE {whereString}");
                pageStringBuilder.Append($" WHERE {whereString}");
            }

            pageStringBuilder.Append(";");
            total = Convert.ToInt32(BaseSQLHelper.ExecuteScalar(_dialect, _connectionString, pageStringBuilder.ToString()));


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
            totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

            List<T> list = new List<T>();
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

        public T SingleOrDefault()
        {
            Type type = typeof(T);
            string tableName = Store.GetTableName(type);
            string columnJoinString = string.Join(",", Store.GetColumnNameList(type));
            List<PropertyInfo> propertyInfoList = Store.GetPropertyInfoList(type);

            string whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {columnJoinString} FROM {tableName}");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableName}");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                sqlStringBuilder.Append($" WHERE {whereString}");
                pageStringBuilder.Append($" WHERE {whereString}");
            }

            pageStringBuilder.Append(";");
            int total = Convert.ToInt32(BaseSQLHelper.ExecuteScalar(_dialect, _connectionString, pageStringBuilder.ToString()));
            if (total > 1)
                throw new SingleOrDefaultException();

            IDataReader reader = null;

            if (_dialect == Dialect.MySQL)
            {
                sqlStringBuilder.Append($" LIMIT 0,1;");
                reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
            }
            else if (_dialect == Dialect.SQLServer)
            {
                if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                    sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
                else
                    sqlStringBuilder.Append($" ORDER BY id");

                sqlStringBuilder.Append($" OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY;");
                reader = SQLServerHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
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

        public T FirstOrDefault()
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = Store.GetPropertyInfoList(type);
            string tableName = Store.GetTableName(type);
            string columnJoinString = string.Join(",", Store.GetColumnNameList(type));
            string whereString = ExpressionToWhereSql.ToWhereString(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            IDataReader reader = null;
            
            switch (_dialect)
            {
                case Dialect.MySQL:
                    if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                        sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

                    sqlStringBuilder.Append($" LIMIT 0,1;");
                    reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
                    break;
                case Dialect.SQLServer:
                    if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                        sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
                    else
                        sqlStringBuilder.Append($" ORDER BY id");

                    sqlStringBuilder.Append($" OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;");
                    reader = SQLServerHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());
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
