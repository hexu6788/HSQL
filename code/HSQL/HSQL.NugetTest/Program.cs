using HSQL.MySQL;
using System;
using System.Linq;

namespace HSQL.NugetTest
{
    class Program
    {
        static void Main(string[] args)
        {
            IDbContext dbContext = new DbContext("127.0.0.1", "test", "root", "123456");
            var list = dbContext.Query("SELECT * FROM t_student;");

            Console.WriteLine("Hello World!");
        }
    }
}
