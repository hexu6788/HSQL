﻿using HSQL.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HSQL
{
    public class ExpressionToSqlWhere
    {
        public static string ToWhere(Expression expression) 
        {
            var where = Resolve(expression);
            return RemoveBeginEndBracket(where);
        }

        public static string Resolve(Expression expression)
        {
            if (expression == null)
                throw new Exception("未处理异常");

            if (expression is LambdaExpression)
                return Resolve(((LambdaExpression)expression).Body);
            else if (expression is BinaryExpression)
            {
                var binaryExpression = (BinaryExpression)expression;

                var symbol = ExpressionTypeSymbol(expression.NodeType);

                switch (expression.NodeType)
                {
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                        {
                            var left = Resolve(binaryExpression.Left);
                            var right = Resolve(binaryExpression.Right);
                            return Combining(left, symbol, right);
                        }
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.LessThan:
                        {
                            var left = Resolve(binaryExpression.Left);
                            var right = "";

                            if (binaryExpression.Right.NodeType == ExpressionType.MemberAccess)
                                right = ResolveMemberValue((MemberExpression)binaryExpression.Right, false);
                            else if (binaryExpression.Right.NodeType == ExpressionType.Constant)
                                right = ((ConstantExpression)binaryExpression.Right).Value.ToString();
                            else if (binaryExpression.Right.NodeType == ExpressionType.Call)
                                right = ResolveMethodCall((MethodCallExpression)binaryExpression.Right);
                            else
                                throw new Exception("未处理异常");

                            return Combining(left, symbol, right);
                        }
                    default:
                        throw new Exception("未处理异常");
                }
            }
            else if (expression is MethodCallExpression)
            {
                return ResolveMethodCall((MethodCallExpression)expression);
            }
            else if (expression is MemberExpression)
            {
                return ResolveMemberName((MemberExpression)expression);
            }
            else if (expression is ConstantExpression)
            {
                return ResolveConstant((ConstantExpression)expression);
            }
            else if (expression is UnaryExpression)
            {
                return ResolveUnary((UnaryExpression)expression);
            }
            throw new Exception("未处理异常");
        }

        private static string ResolveMethodCall(MethodCallExpression expression)
        {
            if (expression.Object == null)
            {
                return Eval((Expression)expression);
            }
            else if (expression.Object.Type.IsGenericType && expression.Method.Name.Equals("Contains"))
            {
                var left = ResolveMemberName((MemberExpression)expression.Arguments[0]);
                var right = "";
                if (expression.Object.NodeType == ExpressionType.MemberAccess)
                    right = ResolveMemberValue((MemberExpression)expression.Object);
                else if (expression.Object.NodeType == ExpressionType.Call)
                    right = ResolveMethodCall((MethodCallExpression)expression.Object);
                else
                    throw new Exception("未处理异常");

                return Combining(left, "IN", $"({right})");
            }
            else
            {
                var left = ResolveMemberName((MemberExpression)expression.Object);
                var right = "";
                if (expression.Arguments[0] is MemberExpression)
                    right = Eval((MemberExpression)expression.Arguments[0]);
                else if (expression.Arguments[0] is ConstantExpression)
                    right = ResolveConstant((ConstantExpression)expression.Arguments[0]);
                else
                    throw new Exception("未处理异常");

                switch (expression.Method.Name)
                {
                    case "Equals":
                        return Combining(left, "=", $"'{right}'");
                    case "Contains":
                        return Combining(left, "LIKE", $"'%{right}%'");
                    default:
                        throw new Exception("未处理异常");
                }
            }
            throw new Exception("未处理异常");
        }

        private static string ResolveMemberName(MemberExpression expression)
        {
           return ((ColumnAttribute)expression.Member.GetCustomAttributes(typeof(ColumnAttribute), true)[0]).Name;
        }

        private static string ResolveMemberValue(MemberExpression expression, bool onlyValue = true)
        {
            return Eval(expression, onlyValue);
        }

        private static string ResolveConstant(ConstantExpression expression)
        {
            return expression.Value.ToString();
        }

        private static string ResolveUnary(UnaryExpression expression) 
        {
            if (expression.NodeType == ExpressionType.Not)
            {
                var value = ResolveMethodCall((MethodCallExpression)expression.Operand).Replace(" = ", " != ");
                return value;
            }
            throw new Exception("未处理异常");
        }

        private static string ExpressionTypeSymbol(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.OrElse:
                    return "OR";
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
                    throw new Exception("未处理异常");
            }
        }

        private static string Eval(Expression expression,bool onlyValue = true)
        {
            if (expression.Type == typeof(int))
                return Expression.Lambda<Func<int>>(Expression.Convert(expression, typeof(int))).Compile().Invoke().ToString();
            else if (expression.Type == typeof(long))
                return Expression.Lambda<Func<long>>(Expression.Convert(expression, typeof(long))).Compile().Invoke().ToString();
            else if (expression.Type == typeof(decimal))
                return Expression.Lambda<Func<decimal>>(Expression.Convert(expression, typeof(decimal))).Compile().Invoke().ToString();
            else if (expression.Type == typeof(float))
                return Expression.Lambda<Func<float>>(Expression.Convert(expression, typeof(float))).Compile().Invoke().ToString();
            else if (expression.Type == typeof(double))
                return Expression.Lambda<Func<double>>(Expression.Convert(expression, typeof(double))).Compile().Invoke().ToString();
            else if (expression.Type == typeof(string))
            {
                return onlyValue ?
                    Expression.Lambda<Func<string>>(Expression.Convert(expression, typeof(string))).Compile().Invoke() :
                    $"'{Expression.Lambda<Func<string>>(Expression.Convert(expression, typeof(string))).Compile().Invoke()}'";
            }
            else if (expression.Type == typeof(List<int>))
                return string.Join(",", Expression.Lambda<Func<List<int>>>(Expression.Convert(expression, typeof(List<int>))).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == typeof(List<long>))
                return string.Join(",", Expression.Lambda<Func<List<long>>>(Expression.Convert(expression, typeof(List<long>))).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == typeof(List<decimal>))
                return string.Join(",", Expression.Lambda<Func<List<decimal>>>(Expression.Convert(expression, typeof(List<decimal>))).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == typeof(List<float>))
                return string.Join(",", Expression.Lambda<Func<List<float>>>(Expression.Convert(expression, typeof(List<float>))).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == typeof(List<double>))
                return string.Join(",", Expression.Lambda<Func<List<double>>>(Expression.Convert(expression, typeof(List<double>))).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == typeof(List<string>))
                return string.Join(",", Expression.Lambda<Func<List<string>>>(Expression.Convert(expression, typeof(List<string>))).Compile().Invoke().Select(x => string.Format("'{0}'", x)));

            throw new Exception("未处理异常");
        }

        private static string Combining(string left, string symbol, string right)
        {
            if (symbol.Equals(">")
                || symbol.Equals(">=")
                || symbol.Equals("=")
                || symbol.Equals("!=")
                || symbol.Equals("<")
                || symbol.Equals("<=")
                || symbol.Equals("IN")
                || symbol.Equals("LIKE")
                || symbol.Equals("AND"))
            {
                return $"{left} {symbol} {right}";
            }
            else if (symbol.Equals("OR"))
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

            var count = 0;

            count += text.IndexOf(" > ") > -1 ? 1 : 0;
            count += text.IndexOf(" >= ") > -1 ? 1 : 0;
            count += text.IndexOf(" = ") > -1 ? 1 : 0;
            count += text.IndexOf(" != ") > -1 ? 1 : 0;
            count += text.IndexOf(" < ") > -1 ? 1 : 0;
            count += text.IndexOf(" <= ") > -1 ? 1 : 0;
            count += text.IndexOf(" IN ") > -1 ? 1 : 0;
            count += text.IndexOf(" LIKE ") > -1 ? 1 : 0;

            var isNormal = count == 1;
            return isNormal;
        }

        private static string RemoveBeginEndBracket(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (!text.Contains("(") || !text.Contains(")"))
                return text;

            if (text.IndexOf("(") == 0 && text.LastIndexOf(")") == text.Length - 1)
            {
                return text.Substring(1, text.Length - 2);
            }
            return text;
            
        }

    }
}
