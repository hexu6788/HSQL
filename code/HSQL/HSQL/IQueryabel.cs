using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HSQL
{
    public interface IQueryabel<T>
    {
        /// <summary>
        /// 查询时添加 AND 条件
        /// </summary>
        /// <param name="condition">条件</param>
        IQueryabel<T> ConditionAnd(Expression<Func<T, bool>> condition);

        /// <summary>
        /// 查询时添加 Or 条件
        /// </summary>
        /// <param name="condition">条件</param>
        IQueryabel<T> ConditionOr(Expression<Func<T, bool>> condition);

        /// <summary>
        /// 取得符合查询条件的第一条数据
        /// </summary>
        T FirstOrDefault();

        /// <summary>
        /// 取得符合查询条件的第一条数据，如符合条件的数据不为1条，则抛出异常
        /// </summary>
        T SingleOrDefault();

        /// <summary>
        /// 取得符合查询条件的数据的数量
        /// </summary>
        int Count();

        /// <summary>
        /// 检查符合查询条件的数据是否存在
        /// </summary>
        bool Exists();

        /// <summary>
        /// 获取数据
        /// </summary>
        List<T> ToList();

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageIndex">页号</param>
        /// <param name="pageSize">每页记录条数</param>
        List<T> ToList(int pageIndex, int pageSize);

        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <param name="pageIndex">页号</param>
        /// <param name="pageSize">每页记录条数</param>
        /// <param name="total">总记录条数</param>
        /// <param name="totalPage">总页数</param>
        List<T> ToList(int pageIndex, int pageSize, out int total, out int totalPage);

        /// <summary>
        /// 正排序
        /// </summary>
        /// <param name="field">排序字段</param>
        IQueryabel<T> OrderBy(string field);

        /// <summary>
        /// 倒排序
        /// </summary>
        /// <param name="field">排序字段</param>
        IQueryabel<T> OrderByDescending(string field);
    }
}
