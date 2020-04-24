using System;

namespace HSQL.Exceptions
{
    public class DataIsNullException : Exception
    {
        public DataIsNullException(string message = "异常原因：数据为空！") : base(message)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
