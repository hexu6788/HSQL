using System;

namespace HSQL.Exceptions
{
    public class ExpressionIsNullException : Exception
    {
        public ExpressionIsNullException(string message = "异常原因：表达式为空！") : base(message)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
