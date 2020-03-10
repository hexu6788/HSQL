using HSQL.Attribute;
using HSQL.Const;
using HSQL.PerformanceOptimization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace HSQL
{
    internal class InstanceFactory
    {
        internal static T CreateInstance<T>(IDataReader reader, List<PropertyInfo> propertyInfoList)
        {
            T instance = Activator.CreateInstance<T>();
            foreach (var property in propertyInfoList)
            {
                var key = Store.GetPropertyColumnAttributeName(property);
                property.SetValue(instance, reader[key]);
            }
            return instance;
        }
    }
}
