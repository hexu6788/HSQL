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
        public SQLServerQueryabel(IDbSQLHelper dbSQLHelper, Expression<Func<T, bool>> predicate)
        {
            DbSQLHelper = dbSQLHelper;
            Predicate = predicate;
        }

        public IQueryabel<T> ConditionAnd(Expression<Func<T, bool>> condition)
        {
            if (Predicate == null)
                Predicate = condition;
            else
                Predicate = Predicate.AndAlso(condition);

            return this;
        }

        public IQueryabel<T> ConditionOr(Expression<Func<T, bool>> condition)
        {
            if (Predicate == null)
                Predicate = condition;
            else
                Predicate = Predicate.OrElse(condition);

            return this;
        }

        public int Count()
        {
            var sql = ExpressionFactory.ToWhereSql(Predicate);
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            var builder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                builder.Append($" WHERE {sql.CommandText}");

            var parameters = DbSQLHelper.Convert(sql.Parameters);

            int total = Convert.ToInt32(DbSQLHelper.ExecuteScalar(builder.ToString(), parameters));
            return total;
        }

        public bool Exists()
        {
            return Count() > 0;
        }

        public List<T> ToList()
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            Sql sql = ExpressionFactory.ToWhereSql(Predicate);

            StringBuilder sqlStringBuilder = new StringBuilder();
            sqlStringBuilder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");

            if (OrderInfoList.Count > 0)
                sqlStringBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));

            List<T> list = DbSQLHelper.ExecuteList<T>(sqlStringBuilder.ToString(), DbSQLHelper.Convert(sql.Parameters));
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize)
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            int pageStart = (pageIndex - 1) * pageSize;
            Sql sql = ExpressionFactory.ToWhereSql(Predicate);

            var builder = new StringBuilder();
            builder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                builder.Append($" WHERE {sql.CommandText}");

            if (OrderInfoList.Count > 0)
                builder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            else
                builder.Append($" ORDER BY {tableInfo.DefaultOrderColumnName}");

            builder.Append($" OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");

            List<T> list = DbSQLHelper.ExecuteList<T>(builder.ToString(), DbSQLHelper.Convert(sql.Parameters));
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize, out int total, out int totalPage)
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            Sql sql = ExpressionFactory.ToWhereSql(Predicate);

            int pageStart = (pageIndex - 1) * pageSize;

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");
                pageStringBuilder.Append($" WHERE {sql.CommandText}");
            }
            pageStringBuilder.Append(";");

            var parameters = DbSQLHelper.Convert(sql.Parameters);
            total = Convert.ToInt32(DbSQLHelper.ExecuteScalar(pageStringBuilder.ToString(), parameters));
            totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

            if (OrderInfoList.Count > 0)
                sqlStringBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            else
                sqlStringBuilder.Append($" ORDER BY {tableInfo.DefaultOrderColumnName}");

            sqlStringBuilder.Append($" OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;");

            List<T> list = DbSQLHelper.ExecuteList<T>(sqlStringBuilder.ToString(), parameters);

            return list;
        }

        public T SingleOrDefault()
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            Sql sql = ExpressionFactory.ToWhereSql(Predicate);

            StringBuilder sqlStringBuilder = new StringBuilder($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            StringBuilder pageStringBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name} WITH(NOLOCK)");

            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                sqlStringBuilder.Append($" WHERE {sql.CommandText}");
                pageStringBuilder.Append($" WHERE {sql.CommandText}");
            }
            pageStringBuilder.Append(";");

            var parameters = DbSQLHelper.Convert(sql.Parameters);

            int total = Convert.ToInt32(DbSQLHelper.ExecuteScalar(pageStringBuilder.ToString(), parameters));
            if (total > 1)
                throw new SingleOrDefaultException();

            if (OrderInfoList.Count > 0)
                sqlStringBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            else
                sqlStringBuilder.Append($" ORDER BY {tableInfo.DefaultOrderColumnName}");

            sqlStringBuilder.Append($" OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY");

            T instance = DbSQLHelper.ExecuteList<T>(sqlStringBuilder.ToString(), parameters).FirstOrDefault();
            return instance;
        }

        public T FirstOrDefault()
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            Sql sql = ExpressionFactory.ToWhereSql(Predicate);

            var builder = new StringBuilder();
            builder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name} WITH(NOLOCK)");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                builder.Append($" WHERE {sql.CommandText}");
            if (OrderInfoList.Count > 0)
                builder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            else
                builder.Append($" ORDER BY {tableInfo.DefaultOrderColumnName}");
            builder.Append($" OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY");

            var parameters = DbSQLHelper.Convert(sql.Parameters);

            T instance = DbSQLHelper.ExecuteList<T>(builder.ToString(), parameters).FirstOrDefault();
            return instance;
        }
    }
}
