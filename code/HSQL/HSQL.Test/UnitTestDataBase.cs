using HSQL.MySQL;
using HSQL.Test.TestDataBaseModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HSQL.Test
{


    [TestClass]
    public class UnitTestDataBase
    {
        private IDatabase database = new MySQLDatabase("127.0.0.1", "test", "root", "123456");

        [TestMethod]
        public void TestInsert()
        {
            var student = new Student()
            {
                Name = "zhangsan",
                Age = 18,
                SchoolId = "123"
            };

            var result = database.Insert(student);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestUpdate()
        {
            database.Delete<Student>(x => x.Id.Contains("test_update_list"));

            database.Insert(new Student()
            {
                Id = $"test_update_list",
                Name = "zhangsan",
                Age = 18,
                SchoolId = "123"
            });

            var result = database.Update(x => x.Id.Contains("test_update_list"), new Student() { Age = 19 });

            database.Delete<Student>(x => x.Id.Contains("test_update_list"));

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestQuerySingle()
        {
            database.Delete<Student>(x => x.Id.Equals("test_query_single"));

            var result = database.Insert(new Student()
            {
                Id = "test_query_single",
                Name = "zhangsan",
                Age = 19
            });


            var student = database.Query<Student>(x => x.Id.Equals("test_query_single")).FirstOrDefault();

            Assert.AreEqual(19, student.Age);
            Assert.AreEqual("test_query_single", student.Id);
            Assert.AreEqual("zhangsan", student.Name);
        }

        [TestMethod]
        public void TestQueryOrderBy()
        {
            var list = database.Query<Student>();
        }

        [TestMethod]
        public void TestQueryAll()
        {
            database.Query<Student>().OrderBy(x => x.Id).ToList();

            database.Delete<Student>(x => !x.Id.Contains(""));


            database.Insert(new School()
            {
                Id = $"test_query_list123",
                Name = "sdf"
            });
            database.Insert(new Student()
            {
                Id = $"test_query_list",
                SchoolId = "test_query_list123",
                Name = "zhangsan",
                Age = 19
            });

            

            var list = database.Query<Student>().ToList();

            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod]
        public void TestQueryList()
        {
            database.Delete<Student>(x => x.Id.Contains("test_query_list"));


            database.Insert(new Student()
            {
                Id = $"test_query_list",
                Name = "zhangsan",
                Age = 19
            });
            var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_list")).ConditionAnd(x => x.Name == "zhangsan").ToList();

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
            
            database.Delete<Student>(x => x.Id.Contains("test_query_page_list"));

            database.Insert(new Student()
            {
                Id = $"test_query_page_list",
                Name = "zhangsan",
                Age = 19
            });
            var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_list"))
                .ConditionAnd(x => x.Name == "zhangsan")
                .OrderBy(x => x.Age)
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
            
            database.Delete<Student>(x => x.Id.Contains("test_query_page_2_list"));

            database.Insert(new Student()
            {
                Id = $"test_query_page_2_list",
                Name = "zhangsan",
                Age = 19
            });

            var total = 0;
            var totalPage = 0;
            var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_2_list"))
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
            
            var result = database.Delete<Student>(x => x.Age > 0);

            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void TestQuerySQL()
        {
            
            var list = database.Query("SELECT * FROM t_student;");

            Assert.AreNotEqual(list, null);
        }

        [TestMethod]
        public void TestQuerySQLByParameters()
        {
           
            var list = database.Query("SELECT t.id,t.name,s.id AS school_id FROM t_student AS t LEFT JOIN t_school AS s ON t.school_id = s.id WHERE t.id = @id AND t.age > @age;", new
            {
                id = "test_query_list",
                age = 1
            });

            Assert.AreNotEqual(list, null);

        }


    }
}
