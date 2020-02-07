# 欢迎使用 ORM框架 HSQL

**HSQL 是一种轻量级的基于 .NET 的数据库对象关系映射「ORM」框架**

![markdown](https://github.com/hexu6788/HSQL/blob/master/file/logo-2.png?raw=true "hsql")

HSQL 是一种可以使用非常简单且高效的方式进行数据库操作的一种框架，通过简单的语法，使数据库操作不再成为难事。

### 使用方法
+ <a href="#创建数据库操作实例">创建数据库操作实例</a>
+ 进行数据库操作
    + <a href="#新增">新增</a>
    + <a href="#批量新增">批量新增</a>
    + <a href="#修改">修改</a>
    + <a href="#删除">删除</a>
    + <a href="#查询">查询</a>
    + <a href="#单实例查询">单实例查询</a>
    + <a href="#分页查询">分页查询</a>


<a id="创建数据库操作实例">创建数据库操作实例：</a>
```csharp
var database = new Database(Dialect.MySQL, $"Server=127.0.0.1;Database=test;Uid=root;Pwd=123456;");
```



<a id="新增">新增：</a>
```csharp
var result = database.Insert<Student>(new Student()
{
	Name = "zhangsan",
	Age = 18,
	SchoolId = "123"
});
```

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


<a id="修改">修改：</a>
```csharp
var result = database.Update<Student>(x => x.Id.Contains("test_update_list"), new Student() { Age = 19 });
```


<a id="删除">删除：</a>
```csharp
var result = database.Delete<Student>(x => x.Age > 0);
```

<a id="查询">查询：</a>
```csharp
var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_list")).ToList();
```

<a id="单实例查询">单实例查询：</a>
```csharp
var student = database.Query<Student>(x => x.Age == 19 && x.Id.Equals("test_query_single")).FirstOrDefault();
```


<a id="分页查询">分页查询：</a>
```csharp
var list = database.Query<Student>(x => x.Age == 19 && x.Id.Contains("test_query_page_list")).ToList(2, 10);
```



