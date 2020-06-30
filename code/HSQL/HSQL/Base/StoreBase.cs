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
            if (_propertyInfoListStore.ContainsKey(type))
                return _propertyInfoListStore.GetValueOrDefault(type);

            List<PropertyInfo> propertyInfoList = type.GetProperties().Where(property =>
            {
                int count = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true).Count();
                return count > 0;
            }).ToList();

            _propertyInfoListStore.TryAdd(type, propertyInfoList);
            return propertyInfoList;
        }

        public static string GetTableName(Type type)
        {
            if (_tableNameStore.ContainsKey(type))
                return _tableNameStore.GetValueOrDefault(type);

            string tableName = ((TableAttribute)type.GetCustomAttributes(TypeOfConst.TableAttribute, true)[0]).Name;
            _tableNameStore.TryAdd(type, tableName);
            return tableName;
        }

        internal static List<string> GetColumnNameList(Type type)
        {
            if (_columnNameListStore.ContainsKey(type))
                return _columnNameListStore.GetValueOrDefault(type);

            List<string> columnNameList = new List<string>();
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
            if (_columnAttributeNameStore.ContainsKey(property))
                return _columnAttributeNameStore.GetValueOrDefault(property);

            object[] attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
            string name = ((ColumnAttribute)attributes[0]).Name;
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
