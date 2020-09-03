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
        private static ConcurrentDictionary<Type, List<PropertyInfo>> _propertyInfoListStore = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        private static ConcurrentDictionary<Type, string> _tableNameStore = new ConcurrentDictionary<Type, string>();
        private static ConcurrentDictionary<Type, List<string>> _columnNameListStore = new ConcurrentDictionary<Type, List<string>>();
        private static ConcurrentDictionary<PropertyInfo, string> _columnAttributeNameStore = new ConcurrentDictionary<PropertyInfo, string>();

        public static List<PropertyInfo> GetPropertyInfoList(Type type)
        {
            List<PropertyInfo> propertyInfoList;
            if (_propertyInfoListStore.ContainsKey(type))
            {
                bool tryGetValue = _propertyInfoListStore.TryGetValue(type, out propertyInfoList);
                if (tryGetValue)
                    return propertyInfoList;
            }

            propertyInfoList = type.GetProperties().Where(property =>
            {
                int count = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true).Count();
                return count > 0;
            }).ToList();

            _propertyInfoListStore.TryAdd(type, propertyInfoList);
            return propertyInfoList;
        }

        public static string GetTableName(Type type)
        {
            string tableName = string.Empty;
            if (_tableNameStore.ContainsKey(type))
            {
                bool tryGetValue = _tableNameStore.TryGetValue(type, out tableName);
                if (tryGetValue)
                    return tableName;
            }

            tableName = ((TableAttribute)type.GetCustomAttributes(TypeOfConst.TableAttribute, true)[0]).Name;
            _tableNameStore.TryAdd(type, tableName);
            return tableName;
        }

        internal static List<string> GetColumnNameList(Type type)
        {
            List<string> columnNameList;
            if (_columnNameListStore.ContainsKey(type))
            {
                bool tryGetValue = _columnNameListStore.TryGetValue(type, out columnNameList);
                if (tryGetValue)
                    return columnNameList;
            }

            columnNameList = new List<string>();
            foreach (PropertyInfo property in type.GetProperties())
            {
                foreach (ColumnAttribute attribute in property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true))
                {
                    columnNameList.Add(attribute.Name);
                }
            }
            _columnNameListStore.TryAdd(type, columnNameList);
            return columnNameList;
        }

        public static string GetColumnJoinString(Type type)
        {
            string columnJoinString = string.Join(",", GetColumnNameList(type));
            return columnJoinString;
        }

        internal static string GetPropertyColumnAttributeName(PropertyInfo property)
        {
            string name = string.Empty;
            if (_columnAttributeNameStore.ContainsKey(property))
            {
                bool tryGetValue = _columnAttributeNameStore.TryGetValue(property, out name);
                if (tryGetValue)
                    return name;
            }

            object[] attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
            name = ((ColumnAttribute)attributes[0]).Name;
            _columnAttributeNameStore.TryAdd(property, name);
            return name;
        }

        public static string BuildInsertSQL<T>(T instance)
        {
            Type type = instance.GetType();

            string tableName = GetTableName(type);
            List<string> columnNameList = GetColumnNameList(type);

            return $"INSERT INTO {tableName}({string.Join(",", columnNameList)}) VALUES({string.Join(",", columnNameList.Select(columnName => string.Format("@{0}", columnName)))});";
        }

        public static Sql BuildDeleteSQL<T>(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ExpressionIsNullException();

            string tableName = GetTableName(typeof(T));
            Sql sql = ExpressionFactory.ToWhereSql(predicate);
            sql.CommandText = $"DELETE FROM {tableName} WHERE {sql.CommandText};";
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

            string tableName = GetTableName(instance.GetType());

            string commandText = $"UPDATE {tableName} SET {string.Join(" , ", columnList.Select(x => string.Format("{0} = @{1}", x.Name, x.Name)))} WHERE {sql.CommandText};";

            var parameters = BuildParameters(columnList);
            parameters.AddRange(sql.Parameters);
            return new Tuple<string, List<Parameter>>(commandText, parameters);
        }
    }
}
