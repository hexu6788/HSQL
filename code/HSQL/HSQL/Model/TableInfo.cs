using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HSQL.Model
{
    public class TableInfo
    {
        public TableInfo()
        {
            Columns = new List<ColumnInfo>();
        }

        /// <summary>
        /// 表名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 使用逗号将列名进行拼接后得到的字符串
        /// </summary>
        public string ColumnsComma { get; set; }

        /// <summary>
        /// 默认排序列
        /// </summary>
        public string DefaultOrderColumnName { get; set; }
        public List<ColumnInfo> Columns { get; set; }
    }

    public class ColumnInfo
    {
        /// <summary>
        /// 是否为自增列
        /// </summary>
        public bool Identity { get; set; }

        /// <summary>
        /// 列名称
        /// </summary>
        public string Name { get; set; }
        public PropertyInfo Property { get; set; }
    }
}
