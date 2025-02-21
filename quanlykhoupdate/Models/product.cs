using quanlykhoupdate.common;
using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class product
    {
        [Key]
        public int id { get; set; }
        public string? title { get; set; }
        public int? warehouseID { get; set; }
        public supplier? suppliers { get; set; }
        public ICollection<product_location>? product_Locations { get; set; }
        public ICollection<inbound_product>? inbound_Products { get; set; }
        public ICollection<outbound_product>? outbound_Products { get; set; }
    }
}
