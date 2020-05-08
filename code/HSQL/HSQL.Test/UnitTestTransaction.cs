using HSQL.Test.TestDataBaseModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace HSQL.Test
{


    [TestClass]
    public class UnitTestTransaction
    {
        string connnectionString = $"Server=127.0.0.1;Database=test;Uid=root;Pwd=123456;";

        [TestMethod]
        public void TestTransactionInsert()
        {
            var database = new Database(Dialect.MySQL, connnectionString);


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
                //throw new Exception("fsd");
            });
        }

        
    }
}
