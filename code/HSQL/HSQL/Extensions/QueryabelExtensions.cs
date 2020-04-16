using System;
using System.Linq.Expressions;

namespace HSQL
{
    public static class QueryabelExtensions
    {
        public static Queryabel<TSource> OrderBy<TSource, TKey>(this Queryabel<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            foreach (var attribute in (keySelector.Body as MemberExpression).Member.CustomAttributes)
            {
                var field = attribute.ConstructorArguments[0].Value as string;
                source.SetOrderBy("ASC");
                source.SetOrderField(field);

                break;
            }
            return source;
        }

        public static Queryabel<TSource> OrderByDescending<TSource, TKey>(this Queryabel<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            foreach (var attribute in (keySelector.Body as MemberExpression).Member.CustomAttributes)
            {
                var field = attribute.ConstructorArguments[0].Value as string;
                source.SetOrderBy("DESC");
                source.SetOrderField(field);

                break;
            }
            return source;
        }
    }
}
