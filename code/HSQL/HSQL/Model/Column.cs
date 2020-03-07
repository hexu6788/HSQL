using System;
using System.Collections.Generic;
using System.Text;

namespace HSQL.Model
{
    internal class Column
    {
        internal Column() { }
        internal Column(string name, object value)
        {
            Name = name;
            Value = value;
        }
        internal string Name { get; set; }
        internal object Value { get; set; }
    }
}
