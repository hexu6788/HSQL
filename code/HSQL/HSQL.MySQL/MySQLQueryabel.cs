using HSQL.Base;
using HSQL.Exceptions;
using HSQL.Factory;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace HSQL.MySQL
{
    public class MySQLQueryabel<T> : QueryabelBase<T>, IQueryabel<T>
    {
        public MySQLQueryabel(IDbSQLHelper dbSQLHelper, Expression<Func<T, bool>> predicate)
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
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            var sql = ExpressionFactory.ToWhereSql(Predicate);
            var builder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name}");
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
            var sql = ExpressionFactory.ToWhereSql(Predicate);

            var builder = new StringBuilder();
            builder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name}");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                builder.Append($" WHERE {sql.CommandText}");

            if (OrderInfoList.Count > 0)
                builder.Append(StoreBase.BuildOrderSQL(OrderInfoList));

            var parameters = DbSQLHelper.Convert(sql.Parameters);

            List<T> list = DbSQLHelper.ExecuteList<T>(builder.ToString(), parameters);
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize)
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            var pageStart = (pageIndex - 1) * pageSize;
            var sql = ExpressionFactory.ToWhereSql(Predicate);

            var builder = new StringBuilder();
            builder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name}");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                builder.Append($" WHERE {sql.CommandText}");
            if (OrderInfoList.Count > 0)
                builder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            builder.Append($" LIMIT {pageStart},{pageSize}");

            var parameters = DbSQLHelper.Convert(sql.Parameters);

            List<T> list = DbSQLHelper.ExecuteList<T>(builder.ToString(), parameters);
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize, out int total, out int totalPage)
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            var sql = ExpressionFactory.ToWhereSql(Predicate);
            var pageStart = (pageIndex - 1) * pageSize;

            var sqlBuilder = new StringBuilder($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name}");
            var pageBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name}");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                sqlBuilder.Append($" WHERE {sql.CommandText}");
                pageBuilder.Append($" WHERE {sql.CommandText}");
            }

            var parameters = DbSQLHelper.Convert(sql.Parameters);
            total = Convert.ToInt32(DbSQLHelper.ExecuteScalar(pageBuilder.ToString(), parameters));
            totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

            if (OrderInfoList.Count > 0)
                sqlBuilder.Append(StoreBase.BuildOrderSQL(OrderInfoList));

            sqlBuilder.Append($" LIMIT {pageStart},{pageSize}");

            List<T> list = DbSQLHelper.ExecuteList<T>(sqlBuilder.ToString(), parameters);
            return list;
        }

        public T SingleOrDefault()
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            var sql = ExpressionFactory.ToWhereSql(Predicate);

            var sqlBuilder = new StringBuilder($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name}");
            var pageBuilder = new StringBuilder($"SELECT COUNT(*) FROM {tableInfo.Name}");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
            {
                sqlBuilder.Append($" WHERE {sql.CommandText}");
                pageBuilder.Append($" WHERE {sql.CommandText}");
            }

            var parameters = DbSQLHelper.Convert(sql.Parameters);
            int total = Convert.ToInt32(DbSQLHelper.ExecuteScalar(pageBuilder.ToString(), parameters));
            if (total > 1)
                throw new SingleOrDefaultException();
            sqlBuilder.Append($" LIMIT 0,1");

            T instance = DbSQLHelper.ExecuteList<T>(sqlBuilder.ToString(), parameters).FirstOrDefault();
            return instance;
        }

        public T FirstOrDefault()
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            var sql = ExpressionFactory.ToWhereSql(Predicate);

            var builder = new StringBuilder();
            builder.Append($"SELECT {tableInfo.ColumnsComma} FROM {tableInfo.Name}");
            if (!string.IsNullOrWhiteSpace(sql.CommandText))
                builder.Append($" WHERE {sql.CommandText}");
            if (OrderInfoList.Count > 0)
                builder.Append(StoreBase.BuildOrderSQL(OrderInfoList));
            builder.Append($" LIMIT 0,1");

            var parameters = DbSQLHelper.Convert(sql.Parameters);

            T instance = DbSQLHelper.ExecuteList<T>(builder.ToString(), parameters).FirstOrDefault();
            return instance;
        }
    }
}
