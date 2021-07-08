using HSQL.Base;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace HSQL.Factory
{
    public class InstanceFactory
    {
        public static T CreateSingleAndDisposeReader<T>(IDataReader reader)
        {
            T instance = default(T);
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            try
            {
                while (reader.Read())
                {
                    instance = Create<T>(tableInfo, reader);
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

        public static T Create<T>(TableInfo tableInfo, IDataReader reader)
        {
            T instance = Activator.CreateInstance<T>();
            foreach (var column in tableInfo.Columns)
            {
                object value = reader[column.Name];
                if (value is DBNull)
                    continue;

                column.Property.SetValue(instance, value);
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

        public static List<T> CreateListAndDisposeReader<T>(IDataReader reader)
        {
            var tableInfo = StoreBase.GetTableInfo(typeof(T));
            List<T> list = new List<T>();
            try
            {
                while (reader.Read())
                {
                    list.Add(Create<T>(tableInfo, reader));
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
