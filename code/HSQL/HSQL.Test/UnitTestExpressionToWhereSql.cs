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
    public class UnitTestExpressionToWhereSql
    {
        [TestMethod]
        public void TestRoutine1()
        {
            var name = "zhangsan";
            var age = 18;
            var schoolId = "123";

            Expression<Func<Student, bool>> expression = x => (x.Name.Equals(name) && x.Age >= age && x.SchoolId.Equals(schoolId));

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"name = '{name}' AND age >= {age} AND school_id = '{schoolId}'");
        }

        [TestMethod]
        public void TestRoutine2()
        {
            var student = new Student()
            {
                Name = "zhangsan",
                Age = 18,
                SchoolId = "123"
            };

            Expression<Func<Student, bool>> expression = x => (x.Name == student.Name && x.Name == "zhangsan" && x.Age >= student.Age && x.SchoolId.Equals(student.SchoolId));

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"name = '{student.Name}' AND name = 'zhangsan' AND age >= {student.Age} AND school_id = '{student.SchoolId}'");
        }

        [TestMethod]
        public void TestRoutine3()
        {
            var studentList = new List<Student>()
            {
                new Student() { Id = "1" },
                new Student() { Id = "2" },
                new Student() { Id = "3" },
                new Student() { Id = "4" }
            };

            Expression<Func<Score, bool>> expression = x =>
                studentList.Select(s => s.Id).ToList().Contains(x.StudentId)
                && ((x.Value > 10 && x.Value < 90) || x.StudentId.Equals("zhangsan"))
                && x.Value != 5;

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"student_id IN ('1','2','3','4') AND ((value > 10 AND value < 90) OR student_id = 'zhangsan') AND value != 5");
        }

        [TestMethod]
        public void TestRoutine4()
        {
            var student = new Student()
            {
                Name = "lisi",
                Age = 60
            };

            Expression<Func<Student, bool>> expression = x => (x.Name == student.Name && x.Age <= student.Age && x.Age > 18 && x.Birthday < 99999 && x.Birthday > UnixTime.ToUnixTimeSecond(new DateTime(1990, 1, 1)));

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"name = '{student.Name}' AND age <= {student.Age} AND age > 18 AND birthday < 99999 AND birthday > {UnixTime.ToUnixTimeSecond(new DateTime(1990, 1, 1))}");
        }

        [TestMethod]
        public void TestRoutine5()
        {
            var score = new Score()
            {
                SubjectId = "yuwen"
            };
            decimal value = (decimal)60.0;

            Expression<Func<Score, bool>> expression = x => (x.SubjectId == score.SubjectId && x.Value >= value);

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"subject_id = '{score.SubjectId}' AND value >= {value}");
        }

        [TestMethod]
        public void TestRoutine6()
        {
            var studentList = new List<Student>()
            {
                new Student() { Id = "1" },
                new Student() { Id = "2" },
                new Student() { Id = "3" },
                new Student() { Id = "4" }
            };

            Expression<Func<Score, bool>> expression = x => studentList.Select(s => s.Id).ToList().Contains(x.StudentId);

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"student_id IN ('1','2','3','4')");
        }

        [TestMethod]
        public void TestRoutine7()
        {
            Expression<Func<Score, bool>> expression = x => ((x.Value > 10 && x.Value < 90 && x.Id.Equals("123")) || x.StudentId.Equals("zhangsan") || x.Value < 5) && x.SubjectId.Equals("123");

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"(((value > 10 AND value < 90 AND id = '123') OR student_id = 'zhangsan') OR value < 5) AND subject_id = '123'");
        }

        [TestMethod]
        public void TestNotEquals()
        {
            Expression<Func<Student, bool>> expression = x => !x.Name.Equals("zhangsan");

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"name != 'zhangsan'");
        }

        [TestMethod]
        public void TestHasFunction() 
        {
            var student = new Student()
            {
                Name = "lisi"
            };

            Expression<Func<Student, bool>> expression = x => (x.Name.Contains(student.Name) && x.Birthday >= UnixTime.ToUnixTimeSecond(new DateTime(1990, 1, 1).Date));

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"name LIKE '%{student.Name}%' AND birthday >= {UnixTime.ToUnixTimeSecond(new DateTime(1990, 1, 1).Date)}");
        }

        [TestMethod]
        public void TestObjectProperty()
        {
            var student = new Student()
            {
                Name = "lisi"
            };

            Expression<Func<Student, bool>> expression = x => x.Name.Equals(student.Name);

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"name = '{student.Name}'");
        }

        [TestMethod]
        public void TestDTO()
        {
            var loginViewModel = new LoginViewModel()
            {
                Account = "zhangsan",
                Checked = 1
            };

            Expression<Func<Student, bool>> expression = x => (x.Name == loginViewModel.Account && x.Age >= 18 && x.SchoolId.Equals("abc"));

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"name = '{loginViewModel.Account}' AND age >= 18 AND school_id = 'abc'");
        }

        [TestMethod]
        public void TestLike()
        {
            var student = new Student()
            {
                Name = "lisi",
                Age = 13
            };

            Expression<Func<Student, bool>> expression = x => (x.Name.Contains(student.Name) && x.Age <= student.Age && x.Age > 18);

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"name LIKE '%{student.Name}%' AND age <= {student.Age} AND age > 18");
        }

        [TestMethod]
        public void TestIn()
        {
            var ageList = new List<int>()
            {
                15,16,17,18,19,20
            };

            var nameList = new List<string>()
            {
                "tony","bryant","kevin"
            };

            Expression<Func<Student, bool>> expression = x => (ageList.Contains(x.Age) && nameList.Contains(x.Name));

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"age IN (15,16,17,18,19,20) AND name IN ('tony','bryant','kevin')");
        }

        [TestMethod]
        public void TestOr()
        {
            var ageList = new List<int>()
            {
                15,16,17,18,19,20
            };

            var nameList = new List<string>()
            {
                "tony","bryant","kevin"
            };

            Expression<Func<Student, bool>> expression = x => (ageList.Contains(x.Age) || nameList.Contains(x.Name));

            var where = ExpressionToWhereSql.ToWhereString(expression);

            Assert.AreEqual(where, $"age IN (15,16,17,18,19,20) OR name IN ('tony','bryant','kevin')");
        }
    }
}
