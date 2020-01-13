using HSQL.Attribute;
using HSQL.Const;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HSQL
{
    internal class ExpressionBase
    {
        internal static string GetTableName(Type type)
        {
            return ((TableAttribute)type.GetCustomAttributes(TypeOfConst.TableAttribute, true)[0]).Name;
        }

        internal static List<Column> GetColumnList<T>(T t)
        {
            var list = new List<Column>();

            foreach (var property in t.GetType().GetProperties())
            {
                foreach (ColumnAttribute attribute in property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true))
                {
                    var value = property.GetValue(t, null);
                    if (property.PropertyType == TypeOfConst.String)
                    {
                        list.Add(new Column(attribute.Name, value == null ? "" : value));
                    }
                    else if (property.PropertyType == TypeOfConst.Int
                        || property.PropertyType == TypeOfConst.Long
                        || property.PropertyType == TypeOfConst.Float
                        || property.PropertyType == TypeOfConst.Double
                        || property.PropertyType == TypeOfConst.Decimal)
                    {
                        list.Add(new Column(attribute.Name, value == null ? 0 : value));
                    }
                    else if (property.PropertyType == TypeOfConst.ByteArray)
                    {
                        if (value != null)
                            list.Add(new Column(attribute.Name, value));
                    }
                }
            }
            if (list.Count <= 0)
                throw new Exception("缺少列名");
            return list;
        }

        internal static List<string> GetColumnNameList(Type type)
        {
            var list = new List<string>();
            foreach (var property in type.GetProperties())
            {
                foreach (ColumnAttribute attribute in property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true))
                {
                    list.Add(attribute.Name);
                }
            }
            return list;
        }

        internal static List<Column> GetColumnListWithOutNull<T>(T instance)
        {
            var list = new List<Column>();

            foreach (var property in instance.GetType().GetProperties())
            {
                foreach (ColumnAttribute attribute in property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true))
                {
                    var value = property.GetValue(instance, null);
                    if (property.PropertyType == TypeOfConst.String
                        || property.PropertyType == TypeOfConst.ByteArray)
                    {
                        if (value != null)
                            list.Add(new Column(attribute.Name, value));
                    }
                    else if (property.PropertyType == TypeOfConst.Int)
                    {
                        if (Convert.ToInt32(value) != 0)
                            list.Add(new Column(attribute.Name, value));
                    }
                    else if (property.PropertyType == TypeOfConst.Long)
                    {
                        if (Convert.ToInt64(value) != 0)
                            list.Add(new Column(attribute.Name, value));
                    }
                    else if (property.PropertyType == TypeOfConst.Decimal)
                    {
                        if (Convert.ToDecimal(value) != 0)
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
