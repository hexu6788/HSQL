using HSQL.Attribute;
using HSQL.Const;
using HSQL.DatabaseHelper;
using HSQL.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HSQL
{
    public class Queryabel<T>
    {
        private string _connectionString;
        private Dialect _dialect;
        private Expression<Func<T, bool>> _predicate;

        public Queryabel(string connectionString, Dialect dialect, Expression<Func<T, bool>> predicate)
        {
            _connectionString = connectionString;
            _dialect = dialect;
            _predicate = predicate;
        }

        public Queryabel<T> AddCondition(Expression<Func<T, bool>> condition)
        {
            _predicate = _predicate.AndAlso(condition);
            return this;
        }

        public List<T> ToList()
        {
            var type = typeof(T);
            var list = new List<T>();

            switch (_dialect)
            {
                case Dialect.MySQL:
                    {
                        using (var reader = MySQLHelper.ExecuteReader(_connectionString, $"SELECT {string.Join(",", ExpressionBase.GetColumnNameList(type))} FROM {ExpressionBase.GetTableName(type)} WHERE {ExpressionToWhereSql.ToWhereString(_predicate)};"))
                        {
                            while (reader.Read())
                            {
                                T instance = Activator.CreateInstance<T>();
                                foreach (var property in instance.GetType().GetProperties())
                                {
                                    var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                                    if (attributes.Length > 0)
                                    {
                                        var key = ((ColumnAttribute)attributes[0]).Name;
                                        property.SetValue(instance, reader[key]);
                                    }
                                }
                                list.Add(instance);
                            }
                        }
                        break;
                    }
                case Dialect.SQLServer:
                    {
                        using (var reader = SQLServerHelper.ExecuteReader(_connectionString, $"SELECT {string.Join(",", ExpressionBase.GetColumnNameList(type))} FROM {ExpressionBase.GetTableName(type)} WHERE {ExpressionToWhereSql.ToWhereString(_predicate)};"))
                        {
                            while (reader.Read())
                            {
                                T instance = Activator.CreateInstance<T>();
                                foreach (var property in instance.GetType().GetProperties())
                                {
                                    var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                                    if (attributes.Length > 0)
                                    {
                                        var key = ((ColumnAttribute)attributes[0]).Name;
                                        property.SetValue(instance, reader[key]);
                                    }
                                }
                                list.Add(instance);
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("未选择数据库方言！");
            }
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize)
        {
            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);
            var columnJoinString = string.Join(",", ExpressionBase.GetColumnNameList(type));
            var pageStart = (pageIndex - 1) * pageSize;
            var where = ExpressionToWhereSql.ToWhereString(_predicate);

            var list = new List<T>();

            switch (_dialect)
            {
                case Dialect.MySQL:
                    {
                        var total = Convert.ToInt32(MySQLHelper.ExecuteScalar(_connectionString, $"SELECT COUNT(*) FROM {tableName} WHERE {where};"));
                        var totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);
                        
                        using (var reader = MySQLHelper.ExecuteReader(_connectionString, $"SELECT {columnJoinString} FROM {tableName} WHERE {where} LIMIT {pageStart},{pageSize};"))
                        {
                            while (reader.Read())
                            {
                                T instance = Activator.CreateInstance<T>();
                                foreach (var property in instance.GetType().GetProperties())
                                {
                                    var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                                    if (attributes.Length > 0)
                                    {
                                        var key = ((ColumnAttribute)attributes[0]).Name;
                                        property.SetValue(instance, reader[key]);
                                    }
                                }
                                list.Add(instance);
                            }
                        }
                        break;
                    }
                case Dialect.SQLServer:
                    {
                        var total = Convert.ToInt32(SQLServerHelper.ExecuteScalar(_connectionString, $"SELECT COUNT(*) FROM {tableName};"));
                        var totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

                        using (var reader = SQLServerHelper.ExecuteReader(_connectionString, $"SELECT {columnJoinString} FROM {tableName} WHERE {where} ORDER BY id OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;"))
                        {
                            while (reader.Read())
                            {
                                T instance = Activator.CreateInstance<T>();
                                foreach (var property in instance.GetType().GetProperties())
                                {
                                    var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                                    if (attributes.Length > 0)
                                    {
                                        var key = ((ColumnAttribute)attributes[0]).Name;
                                        property.SetValue(instance, reader[key]);
                                    }
                                }
                                list.Add(instance);
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("未选择数据库方言！");
            }
            return list;
        }

        public List<T> ToList(int pageIndex, int pageSize,out int total,out int totalPage)
        {
            var type = typeof(T);
            var tableName = ExpressionBase.GetTableName(type);
            var columnJoinString = string.Join(",", ExpressionBase.GetColumnNameList(type));
            var pageStart = (pageIndex - 1) * pageSize;

            var where = ExpressionToWhereSql.ToWhereString(_predicate);

            var list = new List<T>();

            switch (_dialect)
            {
                case Dialect.MySQL:
                    {
                        total = Convert.ToInt32(MySQLHelper.ExecuteScalar(_connectionString, $"SELECT COUNT(*) FROM {tableName} WHERE {where};"));
                        totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

                        using (var reader = MySQLHelper.ExecuteReader(_connectionString, $"SELECT {columnJoinString} FROM {tableName} WHERE {where} LIMIT {pageStart},{pageSize};"))
                        {
                            while (reader.Read())
                            {
                                T instance = Activator.CreateInstance<T>();
                                foreach (var property in instance.GetType().GetProperties())
                                {
                                    var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                                    if (attributes.Length > 0)
                                    {
                                        var key = ((ColumnAttribute)attributes[0]).Name;
                                        property.SetValue(instance, reader[key]);
                                    }
                                }
                                list.Add(instance);
                            }
                        }
                        break;
                    }
                case Dialect.SQLServer:
                    {
                        total = Convert.ToInt32(SQLServerHelper.ExecuteScalar(_connectionString, $"SELECT COUNT(*) FROM {tableName};"));
                        totalPage = (total % pageSize == 0) ? (total / pageSize) : (total / pageSize + 1);

                        using (var reader = SQLServerHelper.ExecuteReader(_connectionString, $"SELECT {columnJoinString} FROM {tableName} WHERE {where} ORDER BY id OFFSET {pageStart} ROWS FETCH NEXT {pageSize} ROWS ONLY;"))
                        {
                            while (reader.Read())
                            {
                                T instance = Activator.CreateInstance<T>();
                                foreach (var property in instance.GetType().GetProperties())
                                {
                                    var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                                    if (attributes.Length > 0)
                                    {
                                        var key = ((ColumnAttribute)attributes[0]).Name;
                                        property.SetValue(instance, reader[key]);
                                    }
                                }
                                list.Add(instance);
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("未选择数据库方言！");
            }
            return list;
        }

        public T FirstOrDefault()
        {
            var type = typeof(T);
            T instance = default(T);

            switch (_dialect)
            {
                case Dialect.MySQL:
                    {
                        using (var reader = MySQLHelper.ExecuteReader(_connectionString, $"SELECT {string.Join(",", ExpressionBase.GetColumnNameList(type))} FROM {ExpressionBase.GetTableName(type)} WHERE {ExpressionToWhereSql.ToWhereString(_predicate)} LIMIT 0,1;"))
                        {
                            while (reader.Read())
                            {
                                instance = Activator.CreateInstance<T>();
                                foreach (var property in instance.GetType().GetProperties())
                                {
                                    var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                                    if (attributes.Length > 0)
                                    {
                                        var key = ((ColumnAttribute)attributes[0]).Name;
                                        property.SetValue(instance, reader[key]);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case Dialect.SQLServer:
                    {
                        using (var reader = SQLServerHelper.ExecuteReader(_connectionString, $"SELECT TOP 1 {string.Join(",", ExpressionBase.GetColumnNameList(type))} FROM {ExpressionBase.GetTableName(type)} WHERE {ExpressionToWhereSql.ToWhereString(_predicate)};"))
                        {
                            while (reader.Read())
                            {
                                instance = Activator.CreateInstance<T>();
                                foreach (var property in instance.GetType().GetProperties())
                                {
                                    var attributes = property.GetCustomAttributes(TypeOfConst.ColumnAttribute, true);
                                    if (attributes.Length > 0)
                                    {
                                        var key = ((ColumnAttribute)attributes[0]).Name;
                                        property.SetValue(instance, reader[key]);
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("未选择数据库方言！");
            }
            return instance;

        }

        
        
    }
}
