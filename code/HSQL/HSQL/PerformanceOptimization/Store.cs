using HSQL.Attribute;
using HSQL.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HSQL.PerformanceOptimization
{
    internal class Store
    {
        private static Dictionary<Type, List<PropertyInfo>> _propertyInfoListStore = new Dictionary<Type, List<PropertyInfo>>();
        private static Dictionary<Type, string> _tableNameStore = new Dictionary<Type, string>();
        private static Dictionary<Type, List<string>> _columnNameListStore = new Dictionary<Type, List<string>>();

        internal static List<PropertyInfo> GetPropertyInfoList(Type type)
        {
            if (_propertyInfoListStore.ContainsKey(type))
                return _propertyInfoListStore.GetValueOrDefault(type);

            var propertyInfoList = type.GetProperties().ToList();
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
    }
}
