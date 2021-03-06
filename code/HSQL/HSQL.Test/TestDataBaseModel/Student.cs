﻿using HSQL.Attribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace HSQL.Test.TestDataBaseModel
{
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
}
