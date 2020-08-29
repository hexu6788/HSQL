using HSQL.Base;
using HSQL.Exceptions;
using HSQL.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HSQL.MSSQLServer
{
    public class SQLServerQueryabel<T> : QueryabelBase<T>, IQueryabel<T>
    {
        public SQLServerQueryabel(IDbSQLHelper dbSQLHelper, bool consolePrintSql, Expression<Func<T, bool>> predicate)
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
            string whereString = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder stringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {StoreBase.GetTableName(typeof(T))} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                stringBuilder.Append($" WHERE {whereString}");
            }

            int total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(_consolePrintSql, stringBuilder.ToString()));
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
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
            else
                sqlStringBuilder.Append($" ORDER BY id");

            sqlStringBuilder.Append($" OFFSET 0 ROWS FETCH NEXT 9999999 ROWS ONLY;");

            List<T> list = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString());
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
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
            else
                sqlStringBuilder.Append($" ORDER BY id");

            sqlStringBuilder.Append($" OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");

            List<T> list = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString());
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

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {columnJoinString} FROM {tableName} WITH(NOLOCK)");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableName} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                sqlStringBuilder.Append($" WHERE {whereString}");
                pageStringBuilder.Append($" WHERE {whereString}");
            }

            pageStringBuilder.Append(";");
            total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(_consolePrintSql, pageStringBuilder.ToString()));
            totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
            else
                sqlStringBuilder.Append($" ORDER BY id");

            sqlStringBuilder.Append($" OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");

            List<T> list = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString());

            return list;
        }

        public T SingleOrDefault()
        {
            Type type = typeof(T);
            List<PropertyInfo> propertyInfoList = StoreBase.GetPropertyInfoList(type);
            string tableName = StoreBase.GetTableName(type);
            string columnJoinString = StoreBase.GetColumnJoinString(type);
            string whereString = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {columnJoinString} FROM {tableName} WITH(NOLOCK)");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableName} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(whereString))
            {
                sqlStringBuilder.Append($" WHERE {whereString}");
                pageStringBuilder.Append($" WHERE {whereString}");
            }

            pageStringBuilder.Append(";");
            int total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(_consolePrintSql, pageStringBuilder.ToString()));
            if (total > 1)
                throw new SingleOrDefaultException();

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
            else
                sqlStringBuilder.Append($" ORDER BY id");

            sqlStringBuilder.Append($" OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY;");

            T instance = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString()).FirstOrDefault();
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
            sqlStringBuilder.Append($"SELECT {columnJoinString} FROM {tableName} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(whereString))
                sqlStringBuilder.Append($" WHERE {whereString}");

            if (!string.IsNullOrWhiteSpace(_orderField) && !string.IsNullOrWhiteSpace(_orderBy))
                sqlStringBuilder.Append($" ORDER BY {_orderField} {_orderBy}");
            else
                sqlStringBuilder.Append($" ORDER BY id");

            sqlStringBuilder.Append($" OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY;");

            T instance = _dbSQLHelper.ExecuteList<T>(_consolePrintSql, propertyInfoList, sqlStringBuilder.ToString()).FirstOrDefault();
            return instance;
        }
    }
}
