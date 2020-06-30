using HSQL.Const;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace HSQL.Base
{
    public class QueryabelBase<T>
    {
        protected string _connectionString;
        protected Expression<Func<T, bool>> _predicate;
        protected string _orderField = string.Empty;
        protected string _orderBy = string.Empty;

        internal void SetOrderField(string field)
        {
            _orderField = field;
        }
        internal void SetOrderBy(string orderBy)
        {
            _orderBy = orderBy;
        }


        
    }
}
