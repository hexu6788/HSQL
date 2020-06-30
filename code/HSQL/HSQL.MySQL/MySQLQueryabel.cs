using HSQL.Base;
using HSQL.Const;
using HSQL.Exceptions;
using HSQL.Factory;
using HSQL.MySQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HSQL.MySQL
{
    public class MySQLQueryabel<T> : QueryabelBase<T>, IQueryabel<T>
    {
        public MySQLQueryabel(string connectionString, Expression<Func<T, bool>> predicate)
        {
            _connectionString = connectionString;
            _predicate = predicate;
        }

        public IQueryabel<T> ConditionAnd(Expression<Func<T, bool>> condition)
        {
            if (_predicate == null)
                _predicate = condition;
            else
                _predicate = _predicate.AndAlso(condition);

            return this;
        }

        public IQueryabel<T> ConditionOr(Expression<Func<T, bool>> condition)
        {
            if (_predicate == null)
                _predicate = condition;
            else
                _predicate = _predicate.OrElse(condition);

            return this;
        }

        public int Count()
        {
            string whereString = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder stringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {StoreBase.GetTableName(typeof(T))}");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                stringBuilder.Append($" WHERE {whereString}");
            }

            int total = Convert.ToInt32(MySQLHelper.ExecuteScalar(_connectionString, stringBuilder.ToString()));
            return total;
        }

        public bool Exists()
        {
            return Count() > 0;
        }

        public List<T> ToList()
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            string whereString = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

            sqlStringBuilder.Append(";");
            IDataReader reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());

            List<T> list = InstanceFactory.CreateListAndDisposeReader<T>(reader, propertyInfoList);
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize)
        {
            Type type = typeof(T);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            int pageStart = (pageIndex - 1) * pageSize;
            string whereString = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

            sqlStringBuilder.Append($" LIMIT {pageStart},{pageSize};");
            IDataReader reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());

            List<T> list = InstanceFactory.CreateListAndDisposeReader<T>(reader, propertyInfoList);
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize, out int total, out int totalPage)
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            string whereString = ExpressionFactory.ToWhereSql(_predicate);

            int pageStart = (pageIndex - 1) * pageSize;

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {columnJoinString} FROM {tableName}");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableName}");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                sqlStringBuilder.Append($" WHERE {whereString}");
                pageStringBuilder.Append($" WHERE {whereString}");
            }

            pageStringBuilder.Append(";");
            total = Convert.ToInt32(MySQLHelper.ExecuteScalar(_connectionString, pageStringBuilder.ToString()));
            totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

            sqlStringBuilder.Append($" LIMIT {pageStart},{pageSize};");
            IDataReader reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());

            List<T> list = InstanceFactory.CreateListAndDisposeReader<T>(reader, propertyInfoList);
            return list;
        }

        public T SingleOrDefault()
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            string whereString = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {columnJoinString} FROM {tableName}");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableName}");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                sqlStringBuilder.Append($" WHERE {whereString}");
                pageStringBuilder.Append($" WHERE {whereString}");
            }

            pageStringBuilder.Append(";");
            int total = Convert.ToInt32(MySQLHelper.ExecuteScalar(_connectionString, pageStringBuilder.ToString()));
            if (total > 1)
                throw new SingleOrDefaultException();

            sqlStringBuilder.Append($" LIMIT 0,1;");
            IDataReader reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());

            T instance = InstanceFactory.CreateSingleAndDisposeReader<T>(reader, propertyInfoList);
            return instance;
        }

        public T FirstOrDefault()
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            string whereString = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

            sqlStringBuilder.Append($" LIMIT 0,1;");
            IDataReader reader = MySQLHelper.ExecuteReader(_connectionString, sqlStringBuilder.ToString());

            T instance = InstanceFactory.CreateSingleAndDisposeReader<T>(reader, propertyInfoList);
            return instance;
        }
    }
}
