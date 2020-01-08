using HSQL.Test.TestDataBaseModel;
using HSQL.Test.TestViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace HSQL.Test
{
    [TestClass]
    public class UnitTestExpressionToSQL
    {
        [TestMethod]
        public void TestRoutine1()
        {
            var name = "zhangsan";
            var age = 18;
            var schoolId = "123";

            Expression<Func<Student, bool>> expression = x => (x.Name.Equals(name) && x.Age >= age && x.SchoolId.Equals(schoolId));

            var where = ExpressionToSqlWhere.Resolve(expression);

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

            Expression<Func<Student, bool>> expression = x => (x.Name == student.Name && x.Age >= student.Age && x.SchoolId.Equals(student.SchoolId));

            var where = ExpressionToSqlWhere.Resolve(expression);

            Assert.AreEqual(where, $"name = '{student.Name}' AND age >= {student.Age} AND school_id = '{student.SchoolId}'");
        }

        [TestMethod]
        public void TestRoutine3()
        {
            var loginViewModel = new LoginViewModel()
            {
                Account = "zhangsan",
                Checked = 1
            };

            Expression<Func<Student, bool>> expression = x => (x.Name == loginViewModel.Account && x.Age >= 18 && x.SchoolId.Equals("abc"));

            var where = ExpressionToSqlWhere.Resolve(expression);

            Assert.AreEqual(where, $"name = '{loginViewModel.Account}' AND age >= 18 AND school_id = 'abc'");
        }

        [TestMethod]
        public void TestRoutine4()
        {
            var student = new Student()
            {
                Name = "lisi",
                Age = 60
            };

            Expression<Func<Student, bool>> expression = x => (x.Name == student.Name && x.Age <= student.Age && x.Age > 18);

            var where = ExpressionToSqlWhere.Resolve(expression);

            Assert.AreEqual(where, $"name = '{student.Name}' AND age <= {student.Age} AND age > 18");
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

            var where = ExpressionToSqlWhere.Resolve(expression);

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

            var where = ExpressionToSqlWhere.Resolve(expression);

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

            var where = ExpressionToSqlWhere.Resolve(expression);

            Assert.AreEqual(where, $"age IN (15,16,17,18,19,20) OR name IN ('tony','bryant','kevin')");
        }
    }
}
