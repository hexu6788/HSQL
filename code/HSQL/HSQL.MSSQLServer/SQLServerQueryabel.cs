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

namespace HSQL.MSSQLServer
{
    public class SQLServerQueryabel<T> : QueryabelBase<T>, IQueryabel<T>
    {
        public SQLServerQueryabel(IDbSQLHelper dbSQLHelper, bool consolePrintSql, Expression<Func<T, bool>> predicate)
        {
            _dbSQLHelper = dbSQLHelper;
            ConsolePrintSql = consolePrintSql;
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
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            StringBuilder stringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                stringBuilder.Append($" WHERE {sql.CommandText}");
            }

            int total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(ConsolePrintSql, stringBuilder.ToString(), _dbSQLHelper.Convert(sql.Parameters)));
            return total;
        }

        public bool Exists()
        {
            return Count() > 0;
        }

        public List<T> ToList()
        {
            Type type = typeof(T);
            var tableInfo = StoreBase.GetTableInfo(type);
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");

            if (OrderInfoList.Count > 0)
                sqlStringBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));

            List<T> list = _dbSQLHelper.ExecuteList<T>(ConsolePrintSql, sqlStringBuilder.ToString(), _dbSQLHelper.Convert(sql.Parameters));
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize)
        {
            Type type = typeof(T);
            var tableInfo = StoreBase.GetTableInfo(type);
            int pageStart = (pageIndex - 1) * pageSize;
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");

            if (OrderInfoList.Count > 0)
                sqlStringBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            else
                sqlStringBuilder.Append($" ORDER BY {tableInfo.DefaultOrderColumnName}");

            sqlStringBuilder.Append($" OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");

            List<T> list = _dbSQLHelper.ExecuteList<T>(ConsolePrintSql, sqlStringBuilder.ToString(), _dbSQLHelper.Convert(sql.Parameters));
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize, out int total, out int totalPage)
        {
            Type type = typeof(T);
            var tableInfo = StoreBase.GetTableInfo(type);
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            int pageStart = (pageIndex - 1) * pageSize;

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");
                pageStringBuilder.Append($" WHERE {sql.CommandText}");
            }
            pageStringBuilder.Append(";");

            var parameters = _dbSQLHelper.Convert(sql.Parameters);
            total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(ConsolePrintSql, pageStringBuilder.ToString(), parameters));
            totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

            if (OrderInfoList.Count > 0)
                sqlStringBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            else
                sqlStringBuilder.Append($" ORDER BY {tableInfo.DefaultOrderColumnName}");

            sqlStringBuilder.Append($" OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");

            List<T> list = _dbSQLHelper.ExecuteList<T>(ConsolePrintSql, sqlStringBuilder.ToString(), parameters);

            return list;
        }

        public T SingleOrDefault()
        {
            Type type = typeof(T);
            var tableInfo = StoreBase.GetTableInfo(type);
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");
                pageStringBuilder.Append($" WHERE {sql.CommandText}");
            }
            pageStringBuilder.Append(";");

            var parameters = _dbSQLHelper.Convert(sql.Parameters);

            int total = Convert.ToInt32(_dbSQLHelper.ExecuteScalar(ConsolePrintSql, pageStringBuilder.ToString(), parameters));
            if (total > 1)
                throw new SingleOrDefaultException();

            if (OrderInfoList.Count > 0)
                sqlStringBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            else
                sqlStringBuilder.Append($" ORDER BY {tableInfo.DefaultOrderColumnName}");

            sqlStringBuilder.Append($" OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY");

            T instance = _dbSQLHelper.ExecuteList<T>(ConsolePrintSql, sqlStringBuilder.ToString(), parameters).FirstOrDefault();
            return instance;
        }

        public T FirstOrDefault()
        {
            Type type = typeof(T);
            var tableInfo = StoreBase.GetTableInfo(type);
            Sql sql = ExpressionFactory.ToWhereSql(_predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");

            if (OrderInfoList.Count > 0)
                sqlStringBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            else
                sqlStringBuilder.Append($" ORDER BY {tableInfo.DefaultOrderColumnName}");

            sqlStringBuilder.Append($" OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY");

            T instance = _dbSQLHelper.ExecuteList<T>(ConsolePrintSql, sqlStringBuilder.ToString(), _dbSQLHelper.Convert(sql.Parameters)).FirstOrDefault();
            return instance;
        }
    }
}
