using HSQL.Attribute;
using HSQL.Const;
using HSQL.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace HSQL.PerformanceOptimization
{
    internal class Store
    {
        private static Dictionary<Type, List<PropertyInfo>> _propertyInfoListStore = new Dictionary<Type, List<PropertyInfo>>();
        private static Dictionary<Type, string> _tableNameStore = new Dictionary<Type, string>();
        private static Dictionary<Type, List<string>> _columnNameListStore = new Dictionary<Type, List<string>>();
        private static Dictionary<PropertyInfo, string> _columnAttributeNameStore = new Dictionary<PropertyInfo, string>();

        internal static List<PropertyInfo> GetPropertyInfoList(Type type)
        {
            if (_propertyInfoListStore.ContainsKey(type))
                return _propertyInfoListStore.GetValueOrDefault(type);

            var propertyInfoList = type.GetProperties().Where(property =>
            {
                var count = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true).Count();
                return count > 0;
            }).ToList();

            _propertyInfoListStore.Add(type, propertyInfoList);
            return propertyInfoList;
        }

        internal static string GetTableName(Type type)
        {
            if (_tableNameStore.ContainsKey(type))
                return _tableNameStore.GetValueOrDefault(type);

            var tableName = ((TableAttribute)type.GetCustomAttributes(TypeOfConst.TableAttribute, true)[0]).Name;
            _tableNameStore.Add(type, tableName);
            return tableName;
        }

        internal static List<string> GetColumnNameList(Type type)
        {
            if (_columnNameListStore.ContainsKey(type))
                return _columnNameListStore.GetValueOrDefault(type);

            var columnNameList = new List<string>();
            foreach (var property in type.GetProperties())
            {
                foreach (ColumnAttribute attribute in property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true))
                {
                    columnNameList.Add(attribute.Name);
                }
            }
            _columnNameListStore.Add(type, columnNameList);
            return columnNameList;
        }

        internal static string GetPropertyColumnAttributeName(PropertyInfo property)
        {
            if (_columnAttributeNameStore.ContainsKey(property))
                return _columnAttributeNameStore.GetValueOrDefault(property);

            var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
            var name = ((ColumnAttribute)attributes[0]).Name;
            _columnAttributeNameStore.Add(property, name);

            return name;
        }

        internal static string BuildInsertSQL(string tableName, List<Column> columnList)
        {
            return $"INSERT INTO {tableName}({string.Join(",", columnList.Select(x => x.Name))}) VALUES({string.Join(",", columnList.Select(x => string.Format("@{0}", x.Name)))});";
        }

        internal static string BuildInsertSQL(string tableName, List<string> columnNameList)
        {
            return $"INSERT INTO {tableName}({string.Join(",", columnNameList)}) VALUES({string.Join(",", columnNameList.Select(x => $"@{x}"))});";
        }

        internal static string BuildUpdateSQL(string tableName,List<Column> columnList,string where)
        {
            return $"UPDATE {tableName} SET {string.Join(" , ", columnList.Select(x => string.Format("{0} = @{1}", x.Name, x.Name)))} WHERE {where};";
        }

        internal static string BuildDeleteSQL(string tableName,string where) {
            return $"DELETE FROM {tableName} WHERE {where};";
        }

        internal static MySqlParameter[] BuildMySqlParameter(List<Column> columnList)
        {
            return columnList.Select(x => new MySqlParameter(x.Name, x.Value)).ToArray();
        }

        internal static SqlParameter[] BuildSqlParameter(List<Column> columnList)
        {
            return columnList.Select(x => new SqlParameter(x.Name, x.Value)).ToArray();
        }

        internal static DbParameter[] BuildDbParameter(Dialect _dialect, List<Column> columnList)
        {
            if (_dialect == Dialect.MySQL)
                return columnList.Select(x => new MySqlParameter(x.Name, x.Value)).ToArray();
            else if (_dialect == Dialect.SQLServer)
                return columnList.Select(x => new SqlParameter(x.Name, x.Value)).ToArray();
            else
                return new List<DbParameter>().ToArray();
        }
    }
}
