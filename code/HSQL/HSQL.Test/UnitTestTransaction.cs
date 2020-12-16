using HSQL.MySQL;
using HSQL.Test.TestDataBaseModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HSQL.Test
{


    [TestClass]
    public class UnitTestTransaction
    {
        //MYSQL
        IDbContext dbContext = new DbContext("127.0.0.1", "test", "root", "123456", 3, true);

        //SQLSERVER
        //IDbContext dbContext = new DbContext("127.0.0.1", "test", "sa", "123");

        [TestMethod]
        public void TestTransactionInsert()
        {
            dbContext.Delete<Student>(x => x.Name.Contains("transaction_"));

            dbContext.Transaction(() =>
            {
                var result1 = dbContext.Insert(new Student()
                {
                    Id = "1",
                    Name = "transaction_1",
                    Age = 18,
                    SchoolId = "123"
                });
                var result2 = dbContext.Insert(new Student()
                {
                    Id = "2",
                    Name = "transaction_2",
                    Age = 18,
                    SchoolId = "123"
                });
            });

            var countYes = dbContext.Query<Student>(x => x.Name == "transaction_1" || x.Name == "transaction_2").ToList().Count;
            Assert.IsTrue(countYes == 2);

            try
            {
                dbContext.Transaction(() =>
                {
                    var result1 = dbContext.Insert(new Student()
                    {
                        Id = "3",
                        Name = "transaction_3",
                        Age = 18,
                        SchoolId = "123"
                    });

                    throw new Exception("asdf");

                    var result2 = dbContext.Insert(new Student()
                    {
                        Id = "4",
                        Name = "transaction_4",
                        Age = 18,
                        SchoolId = "123"
                    });
                });
            }
            catch (Exception ex)
            { 
            
            }

            var countNo = dbContext.Query<Student>(x => x.Name == "transaction_1" || x.Name == "transaction_2" || x.Name == "transaction_3" || x.Name == "transaction_4").ToList().Count;
            Assert.IsTrue(countNo == 2);
        }


        [TestMethod]
        public void TestTransactionManyTask()
        {

            dbContext.Delete<Student>(x => x.Name.Contains("TransactionManyTask_"));

            List<Task> list = new List<Task>();
            for (var i = 0; i < 50; i++)
            {
                var taskIndex = i;
                var task = new Task(() =>
                {
                    dbContext.Transaction(() =>
                    {
                        for (var j = 0; j < 10; j++)
                        {
                            dbContext.Insert(new Student()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = $"TransactionManyTask_{taskIndex}_{j}",
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

            Task.WaitAll(list.ToArray());
            var count = dbContext.Query<Student>(x => x.Name.Contains("TransactionManyTask_")).ToList().Count;
            Assert.IsTrue(500 == count);
        }

    }
}
