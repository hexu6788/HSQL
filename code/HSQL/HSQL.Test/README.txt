未解决BUG:
1. 不能查询重复参数，如：Query<Student>(x => x.Name == "transaction_1" || x.Name == "transaction_2")