using HSQL.MySQL;
using HSQL.Test.TestDataBaseModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace HSQL.Test
{
    [TestClass]
    public class UnitTestDataBase
    {
        IDbContext dbContext = new DbContext("127.0.0.1", "test", "root", "123456", 3, true);

        [TestMethod]
        public void TestInsert()
        {
            var result = dbContext.Insert(new Student()
            {
                Name = "zhangsan",
                Age = 18,
                SchoolId = "123"
            });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestUpdate()
        {
            dbContext.Delete<Student>(x => x.Id.Contains("test_'update_list"));

            var insertResult = dbContext.Insert(new Student()
            {
                Id = $"test_'update_list",
                Name = "zhangsan",
                Age = 18,
                SchoolId = "123"
            });
            Assert.AreEqual(true, insertResult);

            var updateResult = dbContext.Update(x => x.Id.Contains("test_'update_list"), new Student() { Age = 19 });
            Assert.AreEqual(true, updateResult);

            var deleteResult = dbContext.Delete<Student>(x => x.Id.Contains("test_'update_list"));
            Assert.AreEqual(true, deleteResult);
        }

        [TestMethod]
        public void TestQuerySingle()
        {
            var s = new Student()
            {
                Id = "test_query_single",
                Name = "zhangsan",
                Age = 19
            };

            dbContext.Delete<Student>(x => x.Id == s.Id);

            var result = dbContext.Insert(s);

            var student = dbContext.Query<Student>(x => x.Id == s.Id).FirstOrDefault();
            Assert.IsNotNull(student);

            Assert.AreEqual(19, student.Age);
            Assert.AreEqual("test_query_single", student.Id);
            Assert.AreEqual("zhangsan", student.Name);
        }

        [TestMethod]
        public void TestQueryOrderBy()
        {
            var list = dbContext.Query<Student>().OrderBy(x => x.Id).ToList();

            dbContext.Query<Student>().OrderBy("id").ToList();
        }

        [TestMethod]
        public void TestQueryAll()
        {

            dbContext.Query<Student>().OrderBy(x => x.Id).ToList();

            dbContext.Delete<Student>(x => !x.Id.Contains(""));


            dbContext.Insert(new School()
            {
                Id = $"test_query_list123",
                Name = "sdf"
            });
            dbContext.Insert(new Student()
            {
                Id = $"test_query_list",
                SchoolId = "test_query_list123",
                Name = "zhangsan",
                Age = 19
            });

            

            var list = dbContext.Query<Student>().ToList();

            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void TestQueryList()
        {
            dbContext.Delete<Student>(x => x.Id.Contains("test_query_list"));


            dbContext.Insert(new Student()
            {
                Id = $"test_query_list",
                Name = "zhangsan",
                Age = 19
            });
            var list = dbContext.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_list")).ConditionAnd(x => x.Name == "zhangsan").ToList();

            list.ForEach(x =>
            {
                Assert.AreEqual(19, x.Age);
                Assert.IsTrue(x.Id.Contains("test_query_list"));
                Assert.AreEqual("zhangsan", x.Name);
            });
            Assert.IsTrue(list.Count == 1);
        }

        [TestMethod]
        public void TestQueryListByIn()
        {
            dbContext.Delete<Student>(x => x.Id.Contains("test_query_in_list"));

            dbContext.Insert(new Student()
            {
                Id = $"test_query_in_list",
                Name = "zhangsan",
                Age = 19
            });
            var studentList = new List<string>();
            var listNo = dbContext.Query<Student>(x => studentList.Contains(x.Id)).ConditionAnd(x => x.Id == "test_query_in_list" && x.Name == "zhangsan").ToList();

            Assert.IsTrue(listNo.Count == 1);
        }

        [TestMethod]
        public void TestQueryPageList()
        {
            
            dbContext.Delete<Student>(x => x.Id.Contains("test_query_page_list"));

            dbContext.Insert(new Student()
            {
                Id = $"test_query_page_list",
                Name = "zhangsan",
                Age = 19
            });
            var list = dbContext.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_list"))
                .ConditionAnd(x => x.Name == "zhangsan")
                .OrderBy(x => x.Age)
                .ToList(1, 10);

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
            
            dbContext.Delete<Student>(x => x.Id.Contains("test_query_page_2_list"));

            dbContext.Insert(new Student()
            {
                Id = $"test_query_page_2_list",
                Name = "zhangsan",
                Age = 19
            });

            var total = 0;
            var totalPage = 0;
            var list = dbContext.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_2_list"))
                .ConditionAnd(x => x.Name == "zhangsan")
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
            
            var result = dbContext.Delete<Student>(x => x.Age > 0);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestQuerySQL()
        {
            
            var list = dbContext.Query("SELECT * FROM t_student;");

            Assert.AreNotEqual(list, null);
        }

        [TestMethod]
        public void TestQuerySQLByParameters()
        {
            var student = dbContext.Query<Student>(x => x.Id.Equals("test_query_sin'gle")).FirstOrDefault();

            var list = dbContext.Query("SELECT t.id,t.name,s.id AS school_id FROM t_student AS t LEFT JOIN t_school AS s ON t.school_id = s.id WHERE t.id = @id AND t.age > @age;", new
            {
                id = "test_query_list",
                age = 1
            });

            Assert.AreNotEqual(list, null);

        }

        [TestMethod]
        public void TestConsolePrintSQL()
        {
            dbContext.SetConsolePrintSql(true);
            var student = dbContext.Query<Student>(x => x.Id.Equals("test_query_single")).FirstOrDefault();
        }
    }
}
