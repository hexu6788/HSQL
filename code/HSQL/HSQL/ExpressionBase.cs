using HSQL.Attribute;
using HSQL.Const;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace HSQL
{
    internal class ExpressionBase
    {
        internal static List<Column> GetColumnList<T>(T instance)
        {
            var list = new List<Column>();

            foreach (PropertyInfo property in instance.GetType().GetProperties())
            {
                foreach (ColumnAttribute attribute in property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true))
                {
                    var value = property.GetValue(instance, null);

                    if (property.PropertyType == TypeOfConst.String)
                    {
                        list.Add(new Column(attribute.Name, value == null ? string.Empty : value));
                    }
                    else if (property.PropertyType == TypeOfConst.Int
                        || property.PropertyType == TypeOfConst.UInt
                        || property.PropertyType == TypeOfConst.Long
                        || property.PropertyType == TypeOfConst.Float
                        || property.PropertyType == TypeOfConst.Double
                        || property.PropertyType == TypeOfConst.Decimal)
                    {
                        list.Add(new Column(attribute.Name, value == null ? 0 : value));
                    }
                    else if (property.PropertyType == TypeOfConst.ByteArray
                        || property.PropertyType == TypeOfConst.DateTime)
                    {
                        list.Add(new Column(attribute.Name, value));
                    }
                    else {
                        list.Add(new Column(attribute.Name, value));
                    }
                }
            }
            if (list.Count <= 0)
                throw new Exception("缺少列名");
            return list;
        }

        internal static List<Column> GetColumnListWithOutNull<T>(T instance)
        {
            var list = new List<Column>();

            foreach (PropertyInfo property in instance.GetType().GetProperties())
            {
                foreach (ColumnAttribute attribute in property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true))
                {
                    var value = property.GetValue(instance, null);
                    if (value == null)
                        continue;

                    if (property.PropertyType == TypeOfConst.String
                        || property.PropertyType == TypeOfConst.ByteArray
                        || property.PropertyType == TypeOfConst.DateTime)
                    {
                        list.Add(new Column(attribute.Name, value));
                    }
                    else if (property.PropertyType == TypeOfConst.Int
                        || property.PropertyType == TypeOfConst.UInt
                        || property.PropertyType == TypeOfConst.Long
                        || property.PropertyType == TypeOfConst.Float
                        || property.PropertyType == TypeOfConst.Double
                        || property.PropertyType == TypeOfConst.Decimal)
                    {
                        if (Convert.ToInt32(value) != 0)
                            list.Add(new Column(attribute.Name, value));
                    }
                }
            }
            if (list.Count <= 0)
                throw new Exception("缺少列名");
            return list;
        }
    }
}
