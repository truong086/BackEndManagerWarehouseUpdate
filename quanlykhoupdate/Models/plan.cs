using quanlykhoupdate.common;
using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class plan
    {
        [Key]
        public int id { get; set; }
        public int? location_addr_id_new { get; set; }
        public int? location_addr_id_old { get; set; }
        public location_addr? location_Addr_New { get; set; }
        public location_addr? location_Addr_Old { get; set; }
        public int? status { get; set; }
        public DateTimeOffset? time { get; set; } = DateTimeOffset.UtcNow;
    }
}
