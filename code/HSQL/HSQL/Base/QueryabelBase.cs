using System;
using System.Linq.Expressions;

namespace HSQL.Base
{
    public class QueryabelBase<T>
    {
        protected bool _consolePrintSql;
        protected IDbSQLHelper _dbSQLHelper;
        protected Expression<Func<T, bool>> _predicate;
        public string _orderField = string.Empty;
        public string _orderBy = string.Empty;
    }
}
