using System;
using System.Collections.Generic;
using System.Text;

namespace HSQL.Model
{
    internal class Column
    {
        public Column() { }
        public Column(string name, object value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public object Value { get; set; }
    }
}
