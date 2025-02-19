using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class outbound
    {
        [Key]
        public int id { get; set; }
        public string? code { get; set; }
        public DateTimeOffset? created_time { get; set; } = DateTimeOffset.UtcNow;
        public bool? is_action { get; set; }
        public bool? is_pocked { get; set; }
        public ICollection<outbound_product>? outbound_Products { get; set; }
    }
}
