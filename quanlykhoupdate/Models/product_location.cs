using quanlykhoupdate.common;
using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class product_location
    {
        [Key]
        public int id { get; set; }
        public int? product_id { get; set; }
        public int? location_addr_id { get; set; }
        public int? quantity { get; set; }

        public product? products { get; set; }
        public location_addr? location_Addrs { get; set; }
    }
}
