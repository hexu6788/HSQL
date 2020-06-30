using HSQL.MySQL;
using HSQL.Test.TestDataBaseModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HSQL.Test
{


    [TestClass]
    public class UnitTestTransaction
    {
        IDatabase database = new MySQLDatabase("127.0.0.1", "test", "root", "123456");

        [TestMethod]
        public void TestTransactionInsert()
        {
            database.Transaction(() =>
            {
                var result1 = database.Insert(new Student()
                {
                    Id = "1",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });

                var result2 = database.Insert(new Student()
                {
                    Id = "2",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });
            });
        }


        [TestMethod]
        public void TestTransactionManyTask()
        {
            var database = new MySQLDatabase("127.0.0.1", "test", "root", "123456");

            List<Task> list = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                var task = new Task(() =>
                {
                    database.Transaction(() =>
                    {
                        for (var i = 0; i < 100; i++)
                        {
                            database.Insert(new Student()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "zhangsan",
                                Age = 18,
                                SchoolId = "123"
                            });
                        }
                    });
                });
                list.Add(task);
            }

            list.ForEach(x =>
            {
                x.Start();
            });

            list.ForEach(x =>
            {
                if (!x.IsCompleted)
                    x.Wait();
            });
        }

    }
}
