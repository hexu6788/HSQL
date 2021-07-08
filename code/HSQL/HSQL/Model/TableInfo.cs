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

        public string Name { get; set; }
        public string ColumnsComma { get; set; }
        public List<ColumnInfo> Columns { get; set; }
    }

    public class ColumnInfo
    {

        public bool Identity { get; set; }
        public string Name { get; set; }
        public PropertyInfo Property { get; set; }
    }
}
