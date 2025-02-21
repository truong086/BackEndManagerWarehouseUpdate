using System;
using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
	public class update_history
	{
        [Key]
        public int id { get; set; }
        public int? product_id { get; set; }
        public int? location_addr_id { get; set; }
        public int? quantity { get; set; }
        public int? status { get; set; }
        public DateTimeOffset? last_modify_date { get; set; }

        public product? products { get; set; }
        public location_addr? location_Addrs { get; set; }
    }
}

