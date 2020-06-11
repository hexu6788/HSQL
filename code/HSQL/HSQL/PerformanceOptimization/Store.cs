using HSQL.Attribute;
using HSQL.Const;
using HSQL.Exceptions;
using HSQL.Model;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HSQL.PerformanceOptimization
{
    internal class Store
    {
        private static ConcurrentDictionary<Type, List<PropertyInfo>> _propertyInfoListStore = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        private static ConcurrentDictionary<Type, string> _tableNameStore = new ConcurrentDictionary<Type, string>();
        private static ConcurrentDictionary<Type, List<string>> _columnNameListStore = new ConcurrentDictionary<Type, List<string>>();
        private static ConcurrentDictionary<PropertyInfo, string> _columnAttributeNameStore = new ConcurrentDictionary<PropertyInfo, string>();

        internal static List<PropertyInfo> GetPropertyInfoList(Type type)
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

        internal static string GetTableName(Type type)
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

        internal static string GetColumnJoinString(Type type)
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

        internal static string BuildInsertSQL<T>(T instance)
        {
            Type type = instance.GetType();

            string tableName = GetTableName(type);
            List<string> columnNameList = GetColumnNameList(type);

            return $"INSERT INTO {tableName}({string.Join(",", columnNameList)}) VALUES({string.Join(",", columnNameList.Select(columnName => string.Format("@{0}", columnName)))});";
        }

        internal static Tuple<string,DbParameter[]> BuildUpdateSQLAndParameter<T>(Dialect dialect,Expression<Func<T, bool>> expression, T instance)
        {
            string where = ExpressionToSql.ToWhereSql(expression);

            List<Column> columnList = ExpressionBase.GetColumnListWithOutNull(instance);

            string tableName = GetTableName(instance.GetType());

            string sql = $"UPDATE {tableName} SET {string.Join(" , ", columnList.Select(x => string.Format("{0} = @{1}", x.Name, x.Name)))} WHERE {where};";
            
            return new Tuple<string, DbParameter[]>(sql, BuildDbParameter(dialect, columnList));
        }

        internal static string BuildDeleteSQL<T>(Expression<Func<T, bool>> predicate)
        {
            string tableName = GetTableName(typeof(T));

            string where = ExpressionToSql.ToWhereSql(predicate);
            if (string.IsNullOrWhiteSpace(where))
                throw new ExpressionIsNullException();

            return $"DELETE FROM {tableName} WHERE {where};";
        }

        internal static DbParameter[] BuildDbParameter(Dialect _dialect, List<Column> columnList)
        {
            if (_dialect == Dialect.MySQL)
                return columnList.Select(x => (DbParameter)new MySqlParameter(x.Name, x.Value)).ToArray();
            else if (_dialect == Dialect.SQLServer)
                return columnList.Select(x => (DbParameter)new SqlParameter(x.Name, x.Value)).ToArray();

            throw new NoDialectException();
        }

        internal static DbParameter[] DynamicToDbParameters(Dialect dialect, object parameters)
        {
            if (parameters == null)
                throw new EmptyParameterException();

            PropertyInfo[] properties = parameters.GetType().GetProperties();
            if (dialect == Dialect.MySQL)
            {
                return properties.Select(property => (DbParameter)new MySqlParameter(string.Format("@{0}", property.Name), property.GetValue(parameters, null))).ToArray();
            }
            else if (dialect == Dialect.SQLServer)
            {
                return properties.Select(property => (DbParameter)new SqlParameter(string.Format("@{0}", property.Name), property.GetValue(parameters, null))).ToArray();
            }

            throw new NoDialectException();
        }
    }
}
