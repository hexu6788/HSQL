using HSQL.Test.TestDataBaseModel;
using HSQL.Test.TestHelper;
using HSQL.Test.TestViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace HSQL.Test
{
    [TestClass]
    public class UnitTestDataBase
    {
        string connnectionString = $"Server=127.0.0.1;Database=test;Uid=root;Pwd=123456;";

        [TestMethod]
        public void TestInsert()
        {
            var student = new Student()
            {
                Name = "zhangsan",
                Age = 18,
                SchoolId = "123"
            };

            var database = new Database(Dialect.MySQL, connnectionString);

            var result = database.Insert<Student>(student);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestBatchInsert()
        {
            var list = new List<Student>();
            for (var i = 0; i < 1000; i++)
            {
                list.Add(new Student()
                {
                    Id = $"{i}",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });
            }
            

            var database = new Database(Dialect.MySQL, connnectionString);

            var result = database.Insert<Student>(list);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestUpdate()
        {
            var database = new Database(Dialect.MySQL, connnectionString);

            database.Delete<Student>(x => x.Id.Contains("test_update_list"));

            var list = new List<Student>();
            for (var i = 0; i < 1000; i++)
            {
                list.Add(new Student()
                {
                    Id = $"test_update_list_{i}",
                    Name = "zhangsan",
                    Age = 18,
                    SchoolId = "123"
                });
            }
            database.Insert<Student>(list);

            var result = database.Update<Student>(x => x.Id.Contains("test_update_list"), new Student() { Age = 19 });
            
            database.Delete<Student>(x => x.Id.Contains("test_update_list"));

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestQuerySingle()
        {
            var database = new Database(Dialect.MySQL, connnectionString);

            database.Delete<Student>(x => x.Id.Equals("test_query_single"));

            var result = database.Insert<Student>(new Student()
            {
                Id = "test_query_single",
                Name = "zhangsan",
                Age = 19
            });

            var student = database.Query<Student>(x => x.Age == 19 && x.Id.Equals("test_query_single")).AddCondition(x => x.Name == "zhangsan").FirstOrDefault();

            Assert.AreEqual(19, student.Age);
            Assert.AreEqual("test_query_single", student.Id);
            Assert.AreEqual("zhangsan", student.Name);
        }

        [TestMethod]
        public void TestQueryList()
        {
            var database = new Database(Dialect.MySQL, connnectionString);

            database.Delete<Student>(x => x.Id.Contains("test_query_list"));

            var addList = new List<Student>();
            for (var i = 1; i <= 1000; i++)
            {
                addList.Add(new Student()
                {
                    Id = $"test_query_list_{i}",
                    Name = "zhangsan",
                    Age = 19
                });
            }
            database.Insert<Student>(addList);
            var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_list")).AddCondition(x => x.Name == "zhangsan").ToList();

            list.ForEach(x =>
            {
                Assert.AreEqual(19, x.Age);
                Assert.IsTrue(x.Id.Contains("test_query_list"));
                Assert.AreEqual("zhangsan", x.Name);
            });
        }

        [TestMethod]
        public void TestQueryPageList()
        {
            var database = new Database(Dialect.MySQL, connnectionString);

            database.Delete<Student>(x => x.Id.Contains("test_query_page_list"));

            var addList = new List<Student>();
            for (var i = 1; i <= 1000; i++)
            {
                addList.Add(new Student()
                {
                    Id = $"test_query_page_list_{i}",
                    Name = "zhangsan",
                    Age = 19
                });
            }
            database.Insert<Student>(addList);
            var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_list"))
                .AddCondition(x => x.Name == "zhangsan")
                .ToList(2, 10);

            list.ForEach(x =>
            {
                Assert.AreEqual(19, x.Age);
                Assert.IsTrue(x.Id.Contains("test_query_page_list"));
                Assert.AreEqual("zhangsan", x.Name);
            });
        }

        [TestMethod]
        public void TestQueryPageList2()
        {
            var database = new Database(Dialect.MySQL, connnectionString);

            database.Delete<Student>(x => x.Id.Contains("test_query_page_2_list"));

            var addList = new List<Student>();
            for (var i = 1; i <= 1000; i++)
            {
                addList.Add(new Student()
                {
                    Id = $"test_query_page_2_list_{i}",
                    Name = "zhangsan",
                    Age = 19
                });
            }
            database.Insert<Student>(addList);

            var total = 0;
            var totalPage = 0;
            var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_2_list"))
                .AddCondition(x => x.Name == "zhangsan")
                .ToList(2, 10, out total, out totalPage);

            list.ForEach(x =>
            {
                Assert.AreEqual(19, x.Age);
                Assert.IsTrue(x.Id.Contains("test_query_page_2_list"));
                Assert.AreEqual("zhangsan", x.Name);
            });
        }

        [TestMethod]
        public void TestDelete()
        {
            var database = new Database(Dialect.MySQL, connnectionString);

            var result = database.Delete<Student>(x => x.Age > 0);

            Assert.AreEqual(true, result);
        }
    }
}
