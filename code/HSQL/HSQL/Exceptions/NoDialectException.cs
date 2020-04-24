using System;

namespace HSQL.Exceptions
{
    public class NoDialectException : Exception
    {
        public NoDialectException(string message = "异常原因：未选择数据库方言！") : base(message)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
