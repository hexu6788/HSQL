using HSQL.Base;
using HSQL.Const;
using HSQL.Exceptions;
using HSQL.Factory;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HSQL.MySQL
{
    public class MySQLQueryabel<T> : QueryabelBase<T>, IQueryabel<T>
    {
        public MySQLQueryabel(IDbSQLHelper dbSQLHelper,bool consolePrintSql, Expression<Func<T, bool>> predicate)
        {
            _dbSQLHelper = dbSQLHelper;
            _consolePrintSql = consolePrintSql;
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
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder stringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {StoreBase.GetTableName(typeof(T))}");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                stringBuilder.Append($" WHERE {sql.CommandText}");
            }

            int total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(_consolePrintSql, stringBuilder.ToString(), _dbSQLHelper.Convert(sql.Parameters)));
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
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

            sqlStringBuilder.Append(";");

            List<T> list = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString(), _dbSQLHelper.Convert(sql.Parameters));
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize)
        {
            Type type = typeof(T);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            int pageStart = (pageIndex - 1) * pageSize;
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

            sqlStringBuilder.Append($" LIMIT {pageStart},{pageSize};");

            List<T> list = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString(), _dbSQLHelper.Convert(sql.Parameters));
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize, out int total, out int totalPage)
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            int pageStart = (pageIndex - 1) * pageSize;

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {columnJoinString} FROM {tableName}");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableName}");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");
                pageStringBuilder.Append($" WHERE {sql.CommandText}");
            }
            pageStringBuilder.Append(";");

            var parameters = _dbSQLHelper.Convert(sql.Parameters);
            total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(_consolePrintSql, pageStringBuilder.ToString(), parameters));
            totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

            sqlStringBuilder.Append($" LIMIT {pageStart},{pageSize};");

            List<T> list = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString(), parameters);
            return list;
        }

        public T SingleOrDefault()
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {columnJoinString} FROM {tableName}");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableName}");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");
                pageStringBuilder.Append($" WHERE {sql.CommandText}");
            }
            pageStringBuilder.Append(";");

            var parameters = _dbSQLHelper.Convert(sql.Parameters);
            int total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(_consolePrintSql, pageStringBuilder.ToString(), parameters));
            if (total > 1)
                throw new SingleOrDefaultException();

            sqlStringBuilder.Append($" LIMIT 0,1;");

            T instance = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString(), parameters).FirstOrDefault();
            return instance;
        }

        public T FirstOrDefault()
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName}");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");

            sqlStringBuilder.Append($" LIMIT 0,1;");

            T instance = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString(), _dbSQLHelper.Convert(sql.Parameters)).FirstOrDefault();
            return instance;
        }

        public IQueryabel<T> OrderBy(string field)
        {
            _orderBy = KeywordConst.ASC;
            _orderField = field;
            return this;
        }

        public IQueryabel<T> OrderByDescending(string field)
        {
            _orderBy = KeywordConst.DESC;
            _orderField = field;
            return this;
        }


        public IQueryabel<T> Order(string by, string field)
        {
            _orderBy = by;
            _orderField = field;
            return this;
        }
    }
}
