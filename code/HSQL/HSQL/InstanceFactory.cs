using HSQL.Attribute;
using HSQL.Const;
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
            dynamic expando = new ExpandoObject();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                try
                {
                    ((IDictionary<string, object>)expando).Add(reader.GetName(i), reader.GetValue(i));
                }
                catch
                {
                    ((IDictionary<string, object>)expando).Add(reader.GetName(i), null);
                }
            }
            return expando;
        }
    }
}
