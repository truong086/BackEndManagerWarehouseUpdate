using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.common
{
    public class BaseEntity
    {
        protected BaseEntity() { }
        [Key]
        public int id { get; set; }
        public DateTimeOffset? created_date { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? last_modify_date { get; set; }
    }
}
