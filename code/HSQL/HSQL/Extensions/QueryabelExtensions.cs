using HSQL.Base;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace HSQL
{
    public static class QueryabelExtensions
    {
        public static IQueryabel<TSource> OrderBy<TSource, TKey>(this IQueryabel<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            QueryabelBase<TSource> queryabel = (QueryabelBase<TSource>)source;

            foreach (CustomAttributeData attribute in (keySelector.Body as MemberExpression).Member.CustomAttributes)
            {
                string field = attribute.ConstructorArguments[0].Value as string;

                queryabel.OrderBy(field);
                break;
            }
            return (IQueryabel<TSource>)queryabel;
        }

        public static IQueryabel<TSource> OrderByDescending<TSource, TKey>(this IQueryabel<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            QueryabelBase<TSource> queryabel = (QueryabelBase<TSource>)source;

            foreach (CustomAttributeData attribute in (keySelector.Body as MemberExpression).Member.CustomAttributes)
            {
                string field = attribute.ConstructorArguments[0].Value as string;

                queryabel.OrderByDescending(field);
                break;
            }
            return (IQueryabel<TSource>)queryabel;
        }


        public static IQueryabel<TSource> ThenOrderBy<TSource, TKey>(this IQueryabel<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            QueryabelBase<TSource> queryabel = (QueryabelBase<TSource>)source;

            foreach (CustomAttributeData attribute in (keySelector.Body as MemberExpression).Member.CustomAttributes)
            {
                string field = attribute.ConstructorArguments[0].Value as string;

                queryabel.ThenOrderBy(field);
                break;
            }
            return (IQueryabel<TSource>)queryabel;
        }

        public static IQueryabel<TSource> ThenOrderByDescending<TSource, TKey>(this IQueryabel<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            QueryabelBase<TSource> queryabel = (QueryabelBase<TSource>)source;

            foreach (CustomAttributeData attribute in (keySelector.Body as MemberExpression).Member.CustomAttributes)
            {
                string field = attribute.ConstructorArguments[0].Value as string;

                queryabel.ThenOrderByDescending(field);
                break;
            }
            return (IQueryabel<TSource>)queryabel;
        }
    }
}
