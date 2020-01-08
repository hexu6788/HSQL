using HSQL.Attribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace HSQL
{
    public class ExpressionToSqlWhere
    {
        public static string Resolve(Expression expression)
        {
            if (expression == null)
                throw new Exception("baocuo");

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
                        return $"{Resolve(binaryExpression.Left)} {symbol} {Resolve(binaryExpression.Right)}";
                    case ExpressionType.Equal:
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
                            else
                                throw new Exception();
                            
                            return $"{left} {symbol} {right}";
                        }
                    default:
                        throw new Exception();
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
            throw new Exception("baocuo");
        }

        private static string ResolveMethodCall(MethodCallExpression expression)
        {
            if (expression.Object.Type.IsGenericType && expression.Method.Name.Equals("Contains"))
            {
                return $"{ResolveMemberName((MemberExpression)expression.Arguments[0])} IN ({ResolveMemberValue((MemberExpression)expression.Object)})";
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
                    throw new Exception();

                switch (expression.Method.Name)
                {
                    case "Equals":
                        return $"{left} = '{right}'";
                    case "Contains":
                        return $"{left} LIKE '%{right}%'";
                    default:
                        throw new Exception("baocuo");
                }
            }
            throw new Exception("baocuo");
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
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.LessThan:
                    return "<";
                default:
                    throw new Exception();
            }
        }

        private static string Eval(MemberExpression expression,bool onlyValue = true)
        {
            if (expression.Type == typeof(int))
                return Expression.Lambda<Func<int>>(Expression.Convert(expression, typeof(int))).Compile().Invoke().ToString();
            else if (expression.Type == typeof(string))
            {
                return onlyValue ?
                    Expression.Lambda<Func<string>>(Expression.Convert(expression, typeof(string))).Compile().Invoke() :
                    $"'{Expression.Lambda<Func<string>>(Expression.Convert(expression, typeof(string))).Compile().Invoke()}'";
            }
            else if (expression.Type == typeof(List<int>))
                return string.Join(",", Expression.Lambda<Func<List<int>>>(Expression.Convert(expression, typeof(List<int>))).Compile().Invoke().Select(x => string.Format("{0}", x)));
            else if (expression.Type == typeof(List<string>))
                return string.Join(",", Expression.Lambda<Func<List<string>>>(Expression.Convert(expression, typeof(List<string>))).Compile().Invoke().Select(x => string.Format("'{0}'", x)));

            throw new Exception("");
        }

    }
}
