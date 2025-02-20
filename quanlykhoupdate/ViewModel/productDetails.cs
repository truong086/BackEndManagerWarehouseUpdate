namespace quanlykhoupdate.ViewModel
{
    public class productDetails
    {
        public int Id { get; set; }
        public string? title { get; set; }
        public string? code { get; set; }
        public string? area { get; set; }
        public string? line { get; set; }
        public string? shelf { get; set; }
        public List<object>? history { get; set; }
    }

    public class dataProductLocation
    {
        public int Id { get; set; }
        public string? title { get; set; }
        public List<dataLocation>? dataLocations { get; set; }
        public List<object>? history { get; set; }
    }
    public class dataLocation
    {
        public string? code { get; set; }
        public string? area { get; set; }
        public string? line { get; set; }
        public string? shelf { get; set; }
        public int? quantity { get; set; }
    } 
}
