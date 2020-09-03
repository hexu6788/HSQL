using System.Collections.Generic;

namespace HSQL.Model
{
    public class Sql 
    {
        public Sql()
        {
            Parameters = new List<Parameter>();
        }
        public Sql(string commandText)
        {
            CommandText = commandText;
            Parameters = new List<Parameter>();
        }

        public string CommandText { get; set; }
        public List<Parameter> Parameters { get; set; }
    }
}
