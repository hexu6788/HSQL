using HSQL.Attribute;

namespace HSQL.Test.TestDataBaseModel
{
    [Table("t_school")]
    public class School
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
