using HSQL.Attribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace HSQL.Test.TestDataBaseModel
{
    [Table("t_subject")]
    public class Subject
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
