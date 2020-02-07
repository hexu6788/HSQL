# 欢迎使用 ORM框架 HSQL

**HSQL 是一种轻量级的基于 .NET 的数据库对象关系映射「ORM」框架**

![markdown](https://github.com/hexu6788/HSQL/blob/master/file/logo-2.png?raw=true "hsql")

HSQL 是一种可以使用非常`简单`且`高效`的方式进行数据库操作的一种框架，通过简单的语法，使数据库操作不再成为难事。

### 使用方法
+ <a href="#创建映射模型">创建映射模型</a>
+ <a href="#创建数据库操作实例">创建数据库操作实例</a>
+ 进行数据库操作
    + <a href="#新增">新增</a>
    + <a href="#批量新增">批量新增</a>
    + <a href="#修改">修改</a>
    + <a href="#删除">删除</a>
    + <a href="#查询">查询</a>
    + <a href="#单实例查询">单实例查询</a>
    + <a href="#分页查询">分页查询</a>
    + <a href="#灵活条件查询">灵活条件查询</a>


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
> Table 标记一个表对象。如：[Table("t_student")] 代表 Student 类将映射为数据库表 t_student<br/>Column 标记一个列对象。如：[Column("id")] 代表 Id 属性将映射为数据库列 id




<a id="创建数据库操作实例">创建数据库操作实例：</a>
```csharp
var connectionString = $"Server=127.0.0.1;Database=test;Uid=root;Pwd=123456;";
var database = new Database(Dialect.MySQL, connectionString);
```
> connectionString 为数据库连接字符串。<br/>Dialect.MySQL 表示访问数据库的类型为 MYSQL。



<a id="新增">新增：</a>
```csharp
var result = database.Insert<Student>(new Student()
{
    Name = "zhangsan",
    Age = 18,
    SchoolId = "123"
});
```
> Insert 方法可插入一个对象，表示对 t_student 表插入一条数据。<br/>最后被解释为 SQL 语句 -> INSERT INTO t_student(id,name,age,school_id,birthday) VALUES(@id,@name,@age,@school_id,@birthday);




<a id="批量新增">批量新增：</a>
```csharp
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
var result = database.Insert<Student>(list);
```
> Insert 方法可插入一个集合对象，表示对 t_student 表进行批量插入。<br/>最后被解释为事务性批量插入的 SQL 语句，如 INSERT INTO t_student(id,name,age,school_id,birthday) VALUES(@id,@name,@age,@school_id,@birthday); 会进行多条语句事务操作。




<a id="修改">修改：</a>
```csharp
var result = database.Update<Student>(x => x.Id.Contains("test_update_list"), new Student() { Age = 19 });
```
> Update 方法表示更新操作。如：<br/>参数1：x => x.Id.Contains("test_update_list") 被解释为 WHERE id LIKE '%test_update_list%' <br/>参数2：new Student() { Age = 19 } 被解释为 SET age = @age <br/>最终SQL语句为：UPDATE t_student SET age = @age WHERE id LIKE '%test_update_list%';





<a id="删除">删除：</a>
```csharp
var result = database.Delete<Student>(x => x.Age > 0);
```
> Delete 方法表示删除操作。最终被解释为 SQL 语句：DELETE FROM t_student WHERE age > 0;




<a id="查询">查询：</a>
```csharp
var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_list")).ToList();
```
> Query => ToList 方法表示查询操作。最终被解释为 SQL 语句：SELECT id,name,age,school_id,birthday FROM t_student WHERE age = 19 AND id LIKE '%test_query_list%';




<a id="单实例查询">单实例查询：</a>
```csharp
var student = database.Query<Student>(x => x.Age == 19 && x.Id.Equals("test_query_single")).FirstOrDefault();
```
> Query => ToList 方法表示查询操作：<br/>当 Dialect 为 MySQL 时 最终被解释为 SQL 语句：SELECT id,name,age,school_id,birthday FROM t_student WHERE age = 19 AND id = 'test_query_single' LIMIT 0,1;<br/>当 Dialect 为 SQLServer 时 最终被解释为 SQL 语句：SELECT TOP 1 id,name,age,school_id,birthday FROM t_student WHERE age = 19 AND id = 'test_query_single';




<a id="分页查询">分页查询：</a>
```csharp
var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_list")).ToList(2, 10);
```
> Query => ToList(2,10) 方法表示分页查询操作，pageIndex 为第几页，pageSize 为每页记录条数。最终被解释为 SQL 语句：SELECT id,name,age,school_id,birthday FROM t_student WHERE age = 19 AND id LIKE '%test_query_page_list%' LIMIT 10,10;




<a id="灵活条件查询">灵活条件查询：</a>
```csharp
var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_list")).AddCondition(x => x.Name == "zhangsan").ToList(2, 10);
```
>  AddCondition 方法可以对查询进行动态增加条件。最终解释的 SQL 的 WHERE 部分会包含 AND name = 'zhangsan'

