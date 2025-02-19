using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class inbound_product
    {
        [Key]
        public int id { get; set; }
        public int quantity { get; set; }
        public int? product_id { get; set; }
        public int? inbound_id { get; set; }
        public inbound? inbounds { get; set; }
        public product? products { get; set; }
    }
}
