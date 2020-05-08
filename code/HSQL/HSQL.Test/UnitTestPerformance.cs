using HSQL.Test.TestDataBaseModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace HSQL.Test
{
    [TestClass]
    public class UnitTestPerformance
    {
        string connectionString = $"Server=127.0.0.1;Database=test;Uid=root;Pwd=123456;";

        [TestMethod]
        public void TestInsert()
        {
            var number = 30000;

            var database = new Database(Dialect.MySQL, connectionString);
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
        public void TestAdoNetInsert()
        {
            var number = 300000;

            var database = new Database(Dialect.MySQL, connectionString);
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
            list.ForEach(x =>
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        connection.Open();
                        command.CommandText = "INSERT INTO t_student(id,name,age,school_id,birthday) VALUES(@id,@name,@age,@school_id,@birthday);";
                        command.Parameters.AddRange(new MySqlParameter[] {
                            new MySqlParameter("@id",x.Id),
                            new MySqlParameter("@name",x.Name),
                            new MySqlParameter("@age",x.Age),
                            new MySqlParameter("@school_id",x.SchoolId),
                            new MySqlParameter("@birthday",x.Birthday)
                        });
                        command.ExecuteNonQuery();
                        command.Parameters.Clear();
                    }
                }
            });
            stopwatch.Stop();

            var qps = number / (stopwatch.ElapsedMilliseconds / 1000.0);
            var elapsedMilliseconds = $"QPS 为：{qps}";
        }

        [TestMethod]
        public void TestQuerySingle()
        {
            var database = new Database(Dialect.MySQL, connectionString);
            database.Delete<Student>(x => x.Age >= 0);
            var list = new List<Student>();
            for (var i = 0; i < 100000; i++)
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
                for (var i = 0; i < 100000; i++)
                {
                    var student = database.Query<Student>(x => x.Age == 18 && x.Id.Equals($"{i}") && x.SchoolId.Equals("123")).FirstOrDefault();
                }
            });
            
            stopwatch.Stop();
            var elapsedMilliseconds = $"查询十万条次共耗时：{stopwatch.ElapsedMilliseconds}毫秒";
        }

        [TestMethod]
        public void TestQueryAll()
        {
            var number = 2000000;

            var database = new Database(Dialect.MySQL, connectionString);
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

        [TestMethod]
        public void TestAdoNetQueryAll()
        {
            var number = 2000000;

            var database = new Database(Dialect.MySQL, connectionString);
            database.Delete<Student>(x => x.Age >= 0);
            var list = new List<Student>();

            database.Transaction(() =>
            {
                for (var i = 0; i < number; i++)
                {
                    database.Insert<Student>(new Student()
                    {
                        Id = $"{i}",
                        Name = "zhangsan",
                        Age = 18,
                        SchoolId = "123"
                    });
                }
            });

            


            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var connection = new MySqlConnection(connectionString);
            var command = connection.CreateCommand();
            connection.Open();
            command.CommandText = "SELECT * FROM t_student;";
            IDataReader reader = command.ExecuteReader(CommandBehavior.CloseConnection);
            command.Parameters.Clear();

            var list2 = new List<Student>();
            try
            {
                while (reader.Read())
                {
                    var student = new Student();
                    student.Id = reader["id"] == null ? "" : reader["id"].ToString();
                    student.Name = reader["name"] == null ? "" : reader["name"].ToString();
                    student.Age = reader["age"] == null ? 0 : (int)reader["age"];
                    student.SchoolId = reader["school_id"] == null ? "" : reader["school_id"].ToString();
                    student.Birthday = reader["birthday"] == null ? (long)0 : (long)reader["birthday"];
                    list2.Add(student);
                }
            }
            finally
            {
                if (reader != null)
                    reader.Dispose();
            }
            stopwatch.Stop();

            var elapsedMilliseconds = $"数据量为{number}条时，耗时：{stopwatch.ElapsedMilliseconds} ms";
        }
    }
}
