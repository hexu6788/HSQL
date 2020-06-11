using HSQL.Test.TestDataBaseModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace HSQL.Test
{


    [TestClass]
    public class UnitTestTransaction
    {
        [TestMethod]
        public void TestTransactionInsert()
        {
            var database = new Database(Dialect.MySQL, "127.0.0.1", "test", "root", "123456");


            database.Transaction(() =>
            {
                var result1 = database.Insert<Student>(new Student()
                {
                    Id = "1",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });

                var result2 = database.Insert<Student>(new Student()
                {
                    Id = "2",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });
            });
        }

        public void a() {
            string value = "";

        }
    }
}
