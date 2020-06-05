using System;

namespace HSQL.Exceptions
{
    public class EmptyParameterException : Exception
    {
        public EmptyParameterException(string message = "异常原因：参数为空！") : base(message)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
