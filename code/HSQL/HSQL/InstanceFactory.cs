using HSQL.Attribute;
using HSQL.Const;
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
                var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                if (attributes.Length > 0)
                {
                    var key = ((ColumnAttribute)attributes[0]).Name;
                    property.SetValue(instance, reader[key]);
                }
            }
            return instance;
        }
    }
}
