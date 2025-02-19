using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class outbound_product
    {
        [Key]
        public int id { get; set; }
        public int quantity { get; set; }
        public int? product_id { get; set; }
        public int? outbound_id { get; set; }

        public product? products { get; set; }
        public outbound? outbounds { get; set; }
    }
}
