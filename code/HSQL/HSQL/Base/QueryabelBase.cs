using HSQL.Const;
using HSQL.Model;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HSQL.Base
{
    public class QueryabelBase<T>
    {
        public QueryabelBase()
        {
            OrderInfoList = new List<OrderInfo>();
        }

        protected IDbSQLHelper DbSQLHelper { get; set; }
        protected Expression<Func<T, bool>> Predicate { get; set; }
        public List<OrderInfo> OrderInfoList { get; set; }




        internal void OrderBy(string field)
        {
            OrderInfoList = new List<OrderInfo>()
            {
                new OrderInfo()
                {
                    By = KeywordConst.ASC,
                    Field = field
                }
            };
        }

        internal void OrderByDescending(string field)
        {
            OrderInfoList = new List<OrderInfo>()
            {
                new OrderInfo()
                {
                    By = KeywordConst.DESC,
                    Field = field
                }
            };
        }

        internal void ThenOrderBy(string field)
        {
            OrderInfoList.Add(new OrderInfo()
            {
                By = KeywordConst.ASC,
                Field = field
            });
        }

        internal void ThenOrderByDescending(string field)
        {
            OrderInfoList.Add(new OrderInfo()
            {
                By = KeywordConst.DESC,
                Field = field
            });
        }

    }
}
