using HSQL.Test.TestDataBaseModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HSQL.Test
{
    [TestClass]
    public class UnitTestPerformance
    {
        [TestMethod]
        public void TestInsert()
        {
            var number = 30000;

            var database = new Database(Dialect.MySQL, "127.0.0.1", "test", "root", "123456");

            database.Delete<Student>(x => x.Age >= 0);
            var list = new List<Student>();
            for (var i = 0; i < number; i++)
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
            database.Transaction(() =>
            {
                list.ForEach(x =>
                {
                    var result = database.Insert<Student>(x);
                });
            });

            stopwatch.Stop();


            var qps = number / (stopwatch.ElapsedMilliseconds / 1000.0);
            var elapsedMilliseconds = $"QPS 为：{qps}";
        }



        [TestMethod]
        public void TestQuerySingle()
        {
            var database = new Database(Dialect.MySQL, "127.0.0.1", "test", "root", "123456");
            //database.Delete<Student>(x => x.Age >= 0);

            int number = 1000;
            //database.Transaction(() =>
            //{
            //    for (var i = 0; i < number; i++)
            //    {
            //        database.Insert(new Student()
            //        {
            //            Id = $"{i}",
            //            Name = "zhangsan",
            //            Age = 18,
            //            SchoolId = "123"
            //        });
            //    }
            //});

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            for (var j = 0; j < number; j++)
            {
                var student = database.Query<Student>(x => x.Age == 18 && x.Id.Equals($"{j}") && x.SchoolId.Equals("123")).SingleOrDefault();
            }

            stopwatch.Stop();
            var elapsedMilliseconds = $"查询十万条次共耗时：{stopwatch.ElapsedMilliseconds}毫秒";
        }

        [TestMethod]
        public void TestQueryAll()
        {
            var number = 2000000;

            var database = new Database(Dialect.MySQL, "127.0.0.1", "test", "root", "123456");

            database.Delete<Student>(x => x.Age >= 0);
            var list = new List<Student>();
            for (var i = 0; i < number; i++)
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
            database.Transaction(() =>
            {
                list.ForEach(x =>
                {
                    var student = database.Query<Student>(x => x.Age == 18).ToList();
                });
            });
            stopwatch.Stop();

            var elapsedMilliseconds = $"数据量为{number}条时，耗时：{stopwatch.ElapsedMilliseconds} ms";
        }
    }
}
