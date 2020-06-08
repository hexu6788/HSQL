using System;

namespace HSQL.Exceptions
{
    public class ConnectionStringIsEmptyException : Exception
    {
        public ConnectionStringIsEmptyException(string message = "异常原因：连接字符串为空或错误！") : base(message)
        { 
        
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
