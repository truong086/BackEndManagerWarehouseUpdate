using quanlykhoupdate.common;

namespace quanlykhoupdate.Models
{
    public class location_addr : BaseEntity
    {
        public string? code_location_addr { get; set; }
        public string? area { get; set; }
        public string? line { get; set; }
        public string? shelf { get; set; }

        public virtual ICollection<product_location>? product_Locations { get; set; }

    }
}
