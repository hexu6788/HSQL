using System;

namespace HSQL.Exceptions
{
    public class ExpressionException : Exception
    {
        public ExpressionException(string message = "异常原因：表达式异常！") : base(message)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
