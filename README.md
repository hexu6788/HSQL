# 欢迎使用 ORM框架 HSQL

**HSQL 是一种轻量级的基于 .NET 的数据库对象关系映射「ORM」框架**

![markdown](https://github.com/hexu6788/HSQL/blob/master/file/logo-2.png?raw=true "HSQL")

HSQL 是一种可以使用非常`简单`且`高效`的方式进行数据库操作的一种框架，通过简单的语法，使数据库操作不再成为难事。目前支持的数据库有 MySql、SQLServer。

### 安装方法
核心
```csharp
Install-Package HSQL-standard -Version 1.0.0.6
```
MySQL
```csharp
Install-Package HSQL.MySQL -Version 1.0.0.6
```
MSSQLServer
```csharp
Install-Package HSQL.MSSQLServer -Version 1.0.0.6
```

### 使用方法
+ <a href="#创建映射模型">创建映射模型</a>
+ <a href="#创建数据库操作实例">创建数据库操作实例</a>
+ 进行数据库操作
    + <a href="#新增">新增</a>
    + <a href="#修改">修改</a>
    + <a href="#删除">删除</a>
    + <a href="#查询">查询</a>
    + <a href="#单实例查询">单实例查询</a>
    + <a href="#分页查询">分页查询</a>
    + <a href="#灵活条件查询">灵活条件查询</a>
    + <a href="#SQL语句方式查询">SQL语句方式查询</a>
    + <a href="#事务">事务</a>


<a id="创建映射模型">创建映射模型：</a>
```csharp
[Table("t_student")]
public class Student
{
    [Column("id")]
    public string Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("age")]
    public int Age { get; set; }

    [Column("school_id")]
    public string SchoolId { get; set; }

    [Column("birthday")]
    public long Birthday { get; set; }
}
```
Table 标记一个表对象。如：[Table("t_student")] 代表 Student 类将映射为数据库表 t_student<br/>Column 标记一个列对象。如：[Column("id")] 代表 Id 属性将映射为数据库列 id




<a id="创建数据库操作实例">创建数据库操作实例：</a>

> 连接字符串方式创建
```csharp
var database = new MySQLDatabase("server=127.0.0.1;database=test;user id=root;password=123456;
pooling=True;maxpoolsize=100;minpoolsize=0");
```

> 参数方式创建，可设置连接池，默认开启线程池，线程池最大连接数为100，最小连接数为0
```csharp
var database = new MySQLDatabase("127.0.0.1", "test", "root", "123456");
```


<a id="新增">新增：</a>
```csharp
var result = database.Insert(new Student()
{
    Name = "zhangsan",
    Age = 18,
    SchoolId = "123"
});
```
> Insert 方法可插入一个对象，表示对 t_student 表插入一条数据。<br/>最后被解释为 SQL 语句 -> <br/>INSERT INTO t_student(id,name,age,school_id,birthday) VALUES(@id,@name,@age,@school_id,@birthday);




<a id="修改">修改：</a>
```csharp
var result = database.Update(x => x.Id.Contains("test_update_list"), new Student() { Age = 19 });
```
> Update 方法表示更新操作。如：<br/>参数1：x => x.Id.Contains("test_update_list") 被解释为 WHERE id LIKE '%test_update_list%' <br/>参数2：new Student() { Age = 19 } 被解释为 SET age = @age <br/>最终SQL语句为：<br/>UPDATE t_student SET age = @age WHERE id LIKE '%test_update_list%';





<a id="删除">删除：</a>
```csharp
var result = database.Delete<Student>(x => x.Age > 0);
```
> Delete 方法表示删除操作。最终被解释为 SQL 语句：<br/>DELETE FROM t_student WHERE age > 0;




<a id="查询">查询：</a>
```csharp
var list = database.Query<Student>(x => x.Age == 19 
&& x.Id.Contains("test_query_list"))
.ToList();
```
> Query => ToList 方法表示查询操作。最终被解释为 SQL 语句：<br/>SELECT id,name,age,school_id,birthday FROM t_student WHERE age = 19 AND id LIKE '%test_query_list%';




<a id="单实例查询">单实例查询：</a>
```csharp
var student = database.Query<Student>(x => x.Age == 19 
&& x.Id.Equals("test_query_single"))
.FirstOrDefault();
```
> Query => ToList 方法表示查询操作：<br/>当 Dialect 为 MySQL 时 最终被解释为 SQL 语句：<br/>SELECT id,name,age,school_id,birthday FROM t_student WHERE age = 19 AND id = 'test_query_single' LIMIT 0,1;<br/>当 Dialect 为 SQLServer 时 最终被解释为 SQL 语句：<br/>SELECT TOP 1 id,name,age,school_id,birthday FROM t_student WHERE age = 19 AND id = 'test_query_single';




<a id="分页查询">分页查询：</a>
```csharp
var list = database.Query<Student>(x => x.Age == 19 
&& x.Id.Contains("test_query_page_list"))
.ToList(2, 10);
```
> Query => ToList(2,10) 方法表示分页查询操作，pageIndex 为第几页，pageSize 为每页记录条数。<br/>最终被解释为 SQL 语句：<br/>SELECT id,name,age,school_id,birthday FROM t_student WHERE age = 19 AND id LIKE '%test_query_page_list%' LIMIT 10,10;




<a id="灵活条件查询">灵活条件查询：</a>
```csharp
var list = database.Query<Student>(x => x.Age == 19 
&& x.Id.Contains("test_query_page_list"))
.ConditionAnd(x => x.Name == "zhangsan")
.ToList(2, 10);
```
>  ConditionAnd 方法可以对查询进行动态增加条件。<br/>最终解释的 SQL 的 WHERE 部分会包含 AND name = 'zhangsan'

<a id="SQL语句方式查询">SQL语句方式查询：</a>
```csharp
var list = database.Query("SELECT t.id,t.name,s.id AS school_id 
FROM t_student AS t 
LEFT JOIN t_school AS s ON t.school_id = s.id 
WHERE t.id = @id AND t.age > @age;", 
new
{
    id = "test_query_list",
    age = 1
});
```
>  SQL语句方式查询，如有查询SQL有参数，则需使用参数化查询才能防止SQL注入攻击

<a id="事务">事务：</a>
```csharp
var stu1 = new Student()
{
    Id = "1",
    Name = "zhangsan",
    Age = 18,
    SchoolId = "123"
};
database.Transaction(() => 
{
    var result1 = database.Insert(stu1);
    var result2 = database.Update(x=> x.Id == "2", new Student()
    {
        Name = "zhangsan",
        Age = 18,
        SchoolId = "123"
    });
});
```
>  Transaction 方法可以进行事务操作，当方法中 throw 任意异常，事务将回滚。否则当方法执行完后事务会提交