using quanlykhoupdate.common;
using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class supplier
    {
        [Key]
        public int id { get; set; }
        public string? title { get; set; }
        public string? name { get; set; }
        public DateTimeOffset? createdateat { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? updatedateat { get; set; }
        public ICollection<product>? products { get; set; }

    }
}
