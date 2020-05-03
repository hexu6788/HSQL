using System;

namespace HSQL.Exceptions
{
    public class SingleOrDefaultException : Exception
    {
        public SingleOrDefaultException(string message = "异常原因：出现多条实例！") : base(message)
        {

        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
