using System;
using System.Collections.Generic;
using System.Text;

namespace HSQL.Attribute
{
    public class TableAttribute : System.Attribute
    {
        public string Name { get; set; }

        public TableAttribute(string name)
        {
            Name = name;
        }
    }
}
