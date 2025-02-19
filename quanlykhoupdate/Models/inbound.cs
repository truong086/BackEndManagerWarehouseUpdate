using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class inbound
    {
        [Key]
        public int id { get; set; }
        public string? code { get; set; }
        public DateTimeOffset? created_time { get; set; } = DateTimeOffset.UtcNow;
        public bool? is_action { get; set; }
        public ICollection<inbound_product>? inbound_Products { get; set; }
    }
}
