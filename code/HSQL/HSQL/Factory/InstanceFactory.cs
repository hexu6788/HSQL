using HSQL.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Reflection;

namespace HSQL.Factory
{
    public class InstanceFactory
    {
        public static T CreateSingleAndDisposeReader<T>(IDataReader reader, List<PropertyInfo> propertyInfoList)
        {
            T instance = default(T);
            try
            {
                while (reader.Read())
                {
                    instance = Create<T>(reader, propertyInfoList);
                }
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        reader.Close();
                    }
                }
            }
            return instance;
        }

        public static T Create<T>(IDataReader reader, List<PropertyInfo> propertyInfoList)
        {
            T instance = Activator.CreateInstance<T>();
            foreach (PropertyInfo property in propertyInfoList)
            {
                string key = StoreBase.GetPropertyColumnAttributeName(property);
                object value = reader[key];
                if (value is DBNull)
                    continue;

                property.SetValue(instance, value);
            }
            return instance;
        }

        public static dynamic Create(IDataReader reader)
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

        public static List<T> CreateListAndDisposeReader<T>(IDataReader reader, List<PropertyInfo> propertyInfoList)
        {
            List<T> list = new List<T>();
            try
            {
                while (reader.Read())
                {
                    list.Add(Create<T>(reader, propertyInfoList));
                }
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        reader.Close();
                    }
                }
            }
            return list;
        }

        public static List<dynamic> CreateListAndDisposeReader(IDataReader reader)
        {
            List<dynamic> list = new List<dynamic>();
            try
            {
                while (reader.Read())
                {
                    dynamic a = Create(reader);
                    list.Add(a);
                }
            }
            finally
            {
                if (reader != null)
                {
                    if (!reader.IsClosed)
                    {
                        reader.Close();
                    }
                }
            }

            return list;
        }
    }
}
