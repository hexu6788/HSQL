using HSQL.Const;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace HSQL
{
    public static class QueryabelExtensions
    {
        public static Queryabel<TSource> OrderBy<TSource, TKey>(this Queryabel<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            foreach (CustomAttributeData attribute in (keySelector.Body as MemberExpression).Member.CustomAttributes)
            {
                string field = attribute.ConstructorArguments[0].Value as string;
                source.SetOrderBy(KeywordConst.ASC);
                source.SetOrderField(field);

                break;
            }
            return source;
        }

        public static Queryabel<TSource> OrderByDescending<TSource, TKey>(this Queryabel<TSource> source, Expression<Func<TSource, TKey>> keySelector)
        {
            foreach (CustomAttributeData attribute in (keySelector.Body as MemberExpression).Member.CustomAttributes)
            {
                string field = attribute.ConstructorArguments[0].Value as string;
                source.SetOrderBy(KeywordConst.DESC);
                source.SetOrderField(field);

                break;
            }
            return source;
        }
    }
}
