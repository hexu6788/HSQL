using HSQL.Attribute;
using HSQL.Base;
using HSQL.Const;
using HSQL.Exceptions;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace HSQL.Factory
{
    public class ExpressionFactory
    {
        public static List<Column> GetColumnList<T>(T instance)
        {
            List<Column> list = new List<Column>();

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
                    else
                    {
                        list.Add(new Column(attribute.Name, value));
                    }
                }
            }
            if (list.Count <= 0)
                throw new Exception("缺少列名");
            return list;
        }

        public static List<Column> GetColumnListWithOutNull<T>(T instance)
        {
            List<Column> list = new List<Column>();

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

        public static Sql ToWhereSql(Expression expression)
        {
            if (expression == null)
                return new Sql();

            Sql sql = ResolveWhereSql(expression);
            return sql;
        }


        private static Sql ResolveWhereSql(Expression expression)
        {
            if (expression == null)
                throw new ExpressionException();

            if (expression is LambdaExpression)
                return ResolveWhereSql(((LambdaExpression)expression).Body);
            else if (expression is BinaryExpression)
            {
                BinaryExpression binaryExpression = (BinaryExpression)expression;

                switch (expression.NodeType)
                {
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        {
                            Sql left = ResolveWhereSql(binaryExpression.Left);
                            Sql right = ResolveWhereSql(binaryExpression.Right);

                            if (left == null)
                                return right;
                            if (right == null)
                                return left;

                            Sql sql = new Sql(Combining(left.CommandText, ResolveSymbol(expression.NodeType), right.CommandText));
                            sql.Parameters.AddRange(left.Parameters);
                            sql.Parameters.AddRange(right.Parameters);
                            return sql;
                        }
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.LessThan:
                        {
                            string right = "";

                            if (binaryExpression.Right.NodeType == ExpressionType.MemberAccess)
                                right = ResolveMemberValue((MemberExpression)binaryExpression.Right, false);
                            else if (binaryExpression.Right.NodeType == ExpressionType.Constant)
                            {
                                string value = ResolveConstant((ConstantExpression)binaryExpression.Right);
                                if (binaryExpression.Right.Type == TypeOfConst.Int
                                    || binaryExpression.Right.Type == TypeOfConst.UInt
                                    || binaryExpression.Right.Type == TypeOfConst.Long
                                    || binaryExpression.Right.Type == TypeOfConst.Float
                                    || binaryExpression.Right.Type == TypeOfConst.Double
                                    || binaryExpression.Right.Type == TypeOfConst.Decimal)
                                    right = value;
                                else if (binaryExpression.Right.Type == TypeOfConst.String)
                                    right = value;
                                else
                                    throw new ExpressionException();
                            }
                            else if (binaryExpression.Right.NodeType == ExpressionType.Call)
                                right = ResolveMethodCall((MethodCallExpression)binaryExpression.Right).CommandText;
                            else if (binaryExpression.Right.NodeType == ExpressionType.Convert)
                            {
                                var operand = (binaryExpression.Right as UnaryExpression).Operand as MemberExpression;
                                right = ResolveMemberValue(operand);
                            }
                            else
                                throw new ExpressionException();

                            string memberName = ResolveMemberName((MemberExpression)binaryExpression.Left);
                            Sql sql = new Sql(Combining(memberName, ResolveSymbol(expression.NodeType), $"@{memberName}"));
                            sql.Parameters.Add(new Parameter(memberName, right));
                            return sql;
                        }
                    default:
                        throw new ExpressionException();
                }
            }
            else if (expression is MethodCallExpression)
            {
                return ResolveMethodCall((MethodCallExpression)expression);
            }
            else if (expression is ConstantExpression)
            {
                return new Sql(ResolveConstant((ConstantExpression)expression));
            }
            else if (expression is UnaryExpression)
            {
                return ResolveUnary((UnaryExpression)expression);
            }
            throw new ExpressionException();
        }

        

        private static Sql ResolveMethodCall(MethodCallExpression expression)
        {
            if (expression.Object == null)
                return new Sql(Eval(expression));
            else if (expression.Object.Type.IsGenericType && expression.Method.Name.Equals(KeywordConst.Contains))
                return ResolveMethodCallIn(expression);
            else
                return ResolveMethodCallEqualsOrLike(expression);

            throw new ExpressionException();
        }

        private static Sql ResolveMethodCallIn(MethodCallExpression expression)
        {
            string left = StoreBase.GetColumnName((MemberExpression)expression.Arguments[0]);
            string right = "";
            if (expression.Object.NodeType == ExpressionType.MemberAccess)
                right = ResolveMemberValue((MemberExpression)expression.Object);
            else if (expression.Object.NodeType == ExpressionType.Call)
                right = ResolveMethodCall((MethodCallExpression)expression.Object).CommandText;
            else
                throw new ExpressionException();

            if (string.IsNullOrWhiteSpace(right)) 
                return new Sql();

            return new Sql(Combining(left, KeywordConst.IN, $"({right})"));
        }

        private static Sql ResolveMethodCallEqualsOrLike(MethodCallExpression expression)
        {
            string left = StoreBase.GetColumnName((MemberExpression)expression.Object);
            string right = "";
            if (expression.Arguments[0] is MemberExpression)
                right = Eval((MemberExpression)expression.Arguments[0]);
            else if (expression.Arguments[0] is ConstantExpression)
                right = ResolveConstant((ConstantExpression)expression.Arguments[0]);
            else if (expression.Arguments[0] is MethodCallExpression)
                right = ResolveMethodCall((MethodCallExpression)expression.Arguments[0]).CommandText;
            else
                throw new ExpressionException();

            Sql sql = new Sql();
            switch (expression.Method.Name)
            {
                case KeywordConst.Equals:
                    sql.CommandText = Combining(left, "=", $"@{left}");
                    sql.Parameters.Add(new Parameter(left, right));
                    return sql;
                case KeywordConst.Contains:
                    sql.CommandText = Combining(left, KeywordConst.LIKE, $"@{left}");
                    sql.Parameters.Add(new Parameter(left, $"%{right}%"));
                    return sql;
                default:
                    throw new ExpressionException();
            }
        }

        private static string ResolveMemberName(MemberExpression expression)
        {
            return StoreBase.GetColumnName(expression);
        }

        private static string ResolveMemberValue(MemberExpression expression, bool onlyValue = true)
        {
            return Eval(expression, onlyValue);
        }

        private static string ResolveConvertValue(MemberExpression expression, bool onlyValue = true)
        {
            return Eval(expression, onlyValue);
        }

        private static string ResolveConstant(ConstantExpression expression)
        {
            return expression.Value.ToString();
        }

        private static Sql ResolveUnary(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                Sql sql = ResolveMethodCall((MethodCallExpression)expression.Operand);
                sql.CommandText = sql.CommandText.Replace(" = ", " != ");
                return sql;
            }
            throw new ExpressionException();
        }

        private static string ResolveSymbol(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.AndAlso:
                    return KeywordConst.AND;
                case ExpressionType.OrElse:
                    return KeywordConst.OR;
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.LessThan:
                    return "<";
                default:
                    throw new ExpressionException();
            }
        }

        private static string Eval(Expression expression, bool onlyValue = true)
        {
            if (expression.Type == TypeOfConst.Int)
                return Expression.Lambda<Func<int>>(Expression.Convert(expression, TypeOfConst.Int)).Compile().Invoke().ToString();
            else if (expression.Type == TypeOfConst.Long)
                return Expression.Lambda<Func<long>>(Expression.Convert(expression, TypeOfConst.Long)).Compile().Invoke().ToString();
            else if (expression.Type == TypeOfConst.Decimal)
                return Expression.Lambda<Func<decimal>>(Expression.Convert(expression, TypeOfConst.Decimal)).Compile().Invoke().ToString();
            else if (expression.Type == TypeOfConst.Float)
                return Expression.Lambda<Func<float>>(Expression.Convert(expression, TypeOfConst.Float)).Compile().Invoke().ToString();
            else if (expression.Type == TypeOfConst.Double)
                return Expression.Lambda<Func<double>>(Expression.Convert(expression, TypeOfConst.Double)).Compile().Invoke().ToString();
            if (expression.Type == TypeOfConst.DateTime)
                return $"'{Expression.Lambda<Func<DateTime>>(Expression.Convert(expression, TypeOfConst.DateTime)).Compile().Invoke().ToString()}'";
            else if (expression.Type == TypeOfConst.String)
            {
                return Expression.Lambda<Func<string>>(Expression.Convert(expression, TypeOfConst.String)).Compile().Invoke();
            }
            else if (expression.Type == TypeOfConst.ListInt)
                return string.Join(",", Expression.Lambda<Func<List<int>>>(Expression.Convert(expression, TypeOfConst.ListInt)).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == TypeOfConst.ListLong)
                return string.Join(",", Expression.Lambda<Func<List<long>>>(Expression.Convert(expression, TypeOfConst.ListLong)).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == TypeOfConst.ListDecimal)
                return string.Join(",", Expression.Lambda<Func<List<decimal>>>(Expression.Convert(expression, TypeOfConst.ListDecimal)).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == TypeOfConst.ListFloat)
                return string.Join(",", Expression.Lambda<Func<List<float>>>(Expression.Convert(expression, TypeOfConst.ListFloat)).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == TypeOfConst.ListDouble)
                return string.Join(",", Expression.Lambda<Func<List<double>>>(Expression.Convert(expression, TypeOfConst.ListDouble)).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == TypeOfConst.ListString)
                return string.Join(",", Expression.Lambda<Func<List<string>>>(Expression.Convert(expression, TypeOfConst.ListString)).Compile().Invoke().Select(x => string.Format("'{0}'", x)));
            else
            {
                if (expression.Type.BaseType == typeof(Enum))
                {
                    return Expression.Lambda<Func<int>>(Expression.Convert(expression, TypeOfConst.Int)).Compile().Invoke().ToString();
                }
            }

            throw new ExpressionException();
        }

        private static string Combining(string left, string symbol, string right)
        {
            if (string.IsNullOrWhiteSpace(left) && string.IsNullOrWhiteSpace(right))
                return string.Empty;
            if (string.IsNullOrWhiteSpace(left))
                return right;
            if (string.IsNullOrWhiteSpace(right))
                return left;

            if (symbol.Equals(">")
                || symbol.Equals(">=")
                || symbol.Equals("=")
                || symbol.Equals("!=")
                || symbol.Equals("<")
                || symbol.Equals("<=")
                || symbol.Equals(KeywordConst.IN)
                || symbol.Equals(KeywordConst.LIKE)
                || symbol.Equals(KeywordConst.AND))
            {
                return $"{left} {symbol} {right}";
            }
            else if (symbol.Equals(KeywordConst.OR))
            {
                if (!IsNormalExpression(left))
                    left = $"({left})";
                if (!IsNormalExpression(right))
                    right = $"({right})";

                return $"({left} {symbol} {right})";
            }
            return $"{left} {symbol} {right}";
        }

        private static bool IsNormalExpression(string text)
        {
            if (text.IndexOf("(") == 0 && text.LastIndexOf(")") == text.Length - 1)
                return true;

            int count = 0;

            count += text.IndexOf(" > ") > -1 ? 1 : 0;
            count += text.IndexOf(" >= ") > -1 ? 1 : 0;
            count += text.IndexOf(" = ") > -1 ? 1 : 0;
            count += text.IndexOf(" != ") > -1 ? 1 : 0;
            count += text.IndexOf(" < ") > -1 ? 1 : 0;
            count += text.IndexOf(" <= ") > -1 ? 1 : 0;
            count += text.IndexOf(" IN ") > -1 ? 1 : 0;
            count += text.IndexOf(" LIKE ") > -1 ? 1 : 0;

            bool isNormal = count == 1;
            return isNormal;
        }

    }
}
