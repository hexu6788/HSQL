using HSQL.Attribute;
using System;
using System.Collections.Generic;
using System.Text;

namespace HSQL.Test.TestDataBaseModel
{
    [Table("t_score")]
    public class Score
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("student_id")]
        public string StudentId { get; set; }

        [Column("subject_id")]
        public string SubjectId { get; set; }

        [Column("value")]
        public decimal Value { get; set; }
    }
}
