using System;

namespace HSQL.Attribute
{
    public class ColumnAttribute : System.Attribute
    {
        public string Name { get; set; }

        public ColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
