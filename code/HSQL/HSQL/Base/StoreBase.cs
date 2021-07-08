using HSQL.Attribute;
using HSQL.Const;
using HSQL.Exceptions;
using HSQL.Factory;
using HSQL.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HSQL.Base
{
    public class StoreBase
    {
        private static ConcurrentDictionary<Type, TableInfo> _tableInfoStore = new ConcurrentDictionary<Type, TableInfo>();
        private static ConcurrentDictionary<MemberInfo, string> _memberAttributeNameStore = new ConcurrentDictionary<MemberInfo, string>();

        public static TableInfo GetTableInfo(Type type)
        {
            TableInfo tableInfo;
            if (_tableInfoStore.ContainsKey(type))
            {
                bool tryGetValue = _tableInfoStore.TryGetValue(type, out tableInfo);
                if (tryGetValue)
                    return tableInfo;
            }

            var columns = new List<ColumnInfo>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                foreach (var attribute in property.GetCustomAttributes(true))
                {
                    if (attribute is ColumnAttribute)
                    {
                        columns.Add(new ColumnInfo()
                        {
                            Identity = attribute is IdentityAttribute ? true : false,
                            Name = ((ColumnAttribute)attribute).Name,
                            Property = property
                        });
                    }
                }
            }

            tableInfo = new TableInfo()
            {
                Name = ((TableAttribute)type.GetCustomAttributes(TypeOfConst.TableAttribute, true)[0]).Name,
                Columns = columns,
                ColumnsComma = string.Join(",", columns.Select(column => column.Name))
            };
            _tableInfoStore.TryAdd(type, tableInfo);
            return tableInfo;
        }



        internal static string GetColumnName(MemberExpression expression)
        {
            string columnName = string.Empty;
            if (_memberAttributeNameStore.ContainsKey(expression.Member))
            {
                bool tryGetValue = _memberAttributeNameStore.TryGetValue(expression.Member, out columnName);
                if (tryGetValue)
                    return columnName;
            }

            columnName = ((ColumnAttribute)expression.Member.GetCustomAttributes(TypeOfConst.ColumnAttribute, true)[0]).Name;
            _memberAttributeNameStore.TryAdd(expression.Member, columnName);
            return columnName;
        }

        public static string BuildInsertSQL<T>(T instance)
        {
            Type type = instance.GetType();

            var tableInfo = GetTableInfo(type);
            var columnNameList = tableInfo.Columns.Where(column => column.Identity == false).Select(column => column.Name).ToList();
            return $"INSERT INTO {tableInfo.Name}({string.Join(",", columnNameList)}) VALUES({string.Join(",", columnNameList.Select(columnName => $"@{columnName}"))});";
        }

        public static Sql BuildDeleteSQL<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ExpressionIsNullException();

            var tableInfo = GetTableInfo(typeof(T));
            Sql sql = ExpressionFactory.ToWhereSql(predicate);
            sql.CommandText = $"DELETE FROM {tableInfo.Name} WHERE {sql.CommandText};";
            return sql;
        }

        public static List<Parameter> BuildParameters(List<Column> columnList)
        {
            return columnList.Select(x => new Parameter(x.Name, x.Value)).ToList();
        }

        public static List<Parameter> DynamicToParameters(object parameters)
        {
            if (parameters == null)
                throw new EmptyParameterException();

            PropertyInfo[] properties = parameters.GetType().GetProperties();

            return properties.Select(property => new Parameter(string.Format("@{0}", property.Name), property.GetValue(parameters, null))).ToList();
        }

        public static Tuple<string, List<Parameter>> BuildUpdateSQLAndParameters<T>(Expression<Func<T, bool>> expression, T instance)
        {
            Sql sql = ExpressionFactory.ToWhereSql(expression);

            List<Column> columnList = ExpressionFactory.GetColumnListWithOutNull(instance);

            var tableInfo = GetTableInfo(instance.GetType());
            string commandText = $"UPDATE {tableInfo.Name} SET {string.Join(" , ", columnList.Select(x => string.Format("{0} = @{1}", x.Name, x.Name)))} WHERE {sql.CommandText};";

            var parameters = BuildParameters(columnList);
            parameters.AddRange(sql.Parameters);
            return new Tuple<string, List<Parameter>>(commandText, parameters);
        }
    }
}
