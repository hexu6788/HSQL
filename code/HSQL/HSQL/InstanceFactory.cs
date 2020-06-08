using HSQL.PerformanceOptimization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Reflection;

namespace HSQL
{
    internal class InstanceFactory
    {
        internal static T CreateInstance<T>(IDataReader reader, List<PropertyInfo> propertyInfoList)
        {
            T instance = Activator.CreateInstance<T>();
            foreach (PropertyInfo property in propertyInfoList)
            {
                string key = Store.GetPropertyColumnAttributeName(property);
                object value = reader[key];
                if (value is DBNull)
                    continue;

                property.SetValue(instance, value);
            }
            return instance;
        }

        internal static dynamic CreateInstance(IDataReader reader)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);

                if (expando.ContainsKey(columnName))
                    throw new Exception($"查询的列名{columnName}重复！");

                if (reader.IsDBNull(i))
                    expando.Add(columnName, null);
                else
                    expando.Add(columnName, reader.GetValue(i));

            }
            return expando;
        }
    }
}
