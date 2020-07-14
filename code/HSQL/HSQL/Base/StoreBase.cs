using HSQL.Attribute;
using HSQL.Const;
using HSQL.Exceptions;
using HSQL.Factory;
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

        public static string BuildDeleteSQL<T>(Expression<Func<T, bool>> predicate)
        {
            string tableName = GetTableName(typeof(T));

            string where = ExpressionFactory.ToWhereSql(predicate);
            if (string.IsNullOrWhiteSpace(where))
                throw new ExpressionIsNullException();

            return $"DELETE FROM {tableName} WHERE {where};";
        }

        
    }
}
