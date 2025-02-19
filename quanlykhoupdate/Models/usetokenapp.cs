using System.ComponentModel.DataAnnotations;

namespace quanlykhoupdate.Models
{
    public class usetokenapp
    {
        [Key]
        public int id { get; set; }
        public string? token { get; set; }
    }
}
