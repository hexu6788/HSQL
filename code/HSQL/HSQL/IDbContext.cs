using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HSQL
{
    public interface IDbContext
    {
        /// <summary>
        /// 执行新增操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="instance">要新增的实例</param>
        /// <returns>是否新增成功</returns>
        bool Insert<T>(T instance);

        /// <summary>
        /// 执行更新操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="expression">条件表达式，可理解为 SQL 语句中的 WHERE。如：WHERE age = 16 可写为 x=> x.Age = 16</param>
        /// <param name="instance">目标表达式，可理解为SQL 语句中的 SET。如：SET age = 16 , name = '张三' 可写为 new Student(){ Age = 16 , Name = "张三" }</param>
        /// <returns>是否更新成功</returns>
        bool Update<T>(Expression<Func<T, bool>> expression, T instance);

        /// <summary>
        /// 执行删除操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="predicate">条件表达式</param>
        /// <returns>是否删除成功</returns>
        bool Delete<T>(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 事务调用
        /// </summary>
        /// <param name="action">方法体</param>
        void Transaction(Action action);

        /// <summary>
        /// 无条件查询
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        IQueryabel<T> Query<T>();

        /// <summary>
        /// 无条件查询
        /// </summary>
        /// <param name="predicate">查询条件表达式</param>
        /// <typeparam name="T">类型</typeparam>
        IQueryabel<T> Query<T>(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// 使用SQL语句查询，并得到结果集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        List<dynamic> Query(string sql);

        /// <summary>
        /// 使用SQL语句查询，并得到结果集
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameter">参数</param>
        List<dynamic> Query(string sql, dynamic parameter);
    }
}
