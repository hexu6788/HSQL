using System;

namespace HSQL.Exceptions
{
    public class EmptySQLException : Exception
    {
        public EmptySQLException(string message = "异常原因：生成SQL语句为空！") : base(message)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
