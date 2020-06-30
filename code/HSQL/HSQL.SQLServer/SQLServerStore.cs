using HSQL.Base;
using HSQL.Exceptions;
using HSQL.Factory;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HSQL.MSSQLServer
{
    class SQLServerStore : StoreBase
    {
        public static SqlParameter[] BuildSqlParameters(List<Column> columnList)
        {
            return columnList.Select(x => new SqlParameter(x.Name, x.Value)).ToArray();
        }

        public static SqlParameter[] DynamicToSqlParameters(object parameters)
        {
            if (parameters == null)
                throw new EmptyParameterException();

            PropertyInfo[] properties = parameters.GetType().GetProperties();
            return properties.Select(property => new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(parameters, null))).ToArray();
        }

        public static Tuple<string, SqlParameter[]> BuildUpdateSQLAndParameters<T>(Expression<Func<T, bool>> expression, T instance)
        {
            string where = ExpressionFactory.ToWhereSql(expression);

            List<Column> columnList = ExpressionFactory.GetColumnListWithOutNull(instance);

            string tableName = GetTableName(instance.GetType());

            string sql = $"UPDATE {tableName} SET {string.Join(" , ", columnList.Select(x => string.Format("{0} = @{1}", x.Name, x.Name)))} WHERE {where};";

            return new Tuple<string, SqlParameter[]>(sql, BuildSqlParameters(columnList));
        }
    }
}
