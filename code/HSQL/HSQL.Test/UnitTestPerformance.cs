using HSQL.Test.TestDataBaseModel;
using HSQL.Test.TestHelper;
using HSQL.Test.TestViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HSQL.Test
{
    [TestClass]
    public class UnitTestPerformance
    {
        string connnectionString = $"Server=127.0.0.1;Database=test;Uid=root;Pwd=123456;";

        [TestMethod]
        public void TestInsert()
        {
            var database = new Database(Dialect.MySQL, connnectionString);
            database.Delete<Student>(x => x.Age >= 0);
            var list = new List<Student>();
            for (var i = 0; i < 100000; i++)
            {
                list.Add(new Student()
                {
                    Id = $"{i}",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            list.ForEach(x =>
            {
                var result = database.Insert<Student>(x);
            });
            stopwatch.Stop();
            var elapsedMilliseconds = $"插入十万条次共耗时：{stopwatch.ElapsedMilliseconds}毫秒";
        }

        [TestMethod]
        public void TestBatchInsert()
        {
            var database = new Database(Dialect.MySQL, connnectionString);
            database.Delete<Student>(x => x.Age >= 0);
            var list = new List<Student>();
            for (var i = 0; i < 100000; i++)
            {
                list.Add(new Student()
                {
                    Id = $"{i}",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = database.Insert<Student>(list);
            stopwatch.Stop();
            var elapsedMilliseconds = $"插入十万条次共耗时：{stopwatch.ElapsedMilliseconds}毫秒";
        }



        [TestMethod]
        public void TestQuerySingle()
        {
            var database = new Database(Dialect.MySQL, connnectionString);
            database.Delete<Student>(x => x.Age >= 0);
            var list = new List<Student>();
            for (var i = 0; i < 100000; i++)
            {
                list.Add(new Student()
                {
                    Id = $"{i}",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });
            }
            var result = database.Insert<Student>(list);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (var i = 0; i < 100000; i++)
            {
                var student = database.Query<Student>(x => x.Age == 18 && x.Id.Equals($"{i}") && x.SchoolId.Equals("123")).FirstOrDefault();
            }
            stopwatch.Stop();
            var elapsedMilliseconds = $"查询十万条次共耗时：{stopwatch.ElapsedMilliseconds}毫秒";
        }
    }
}
