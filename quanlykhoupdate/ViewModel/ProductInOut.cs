namespace quanlykhoupdate.ViewModel
{
    public class ListDataProduct
    {
        public List<ProductInOut>? list {  get; set; }  
    }
    public class ProductInOut
    {
        public string? title { get; set; }
        public int? quantity { get; set; }
        public string? supplier { get; set; }
        public List<InOutByProduct>? InOutByProducts { get; set; }
        public List<object>? history { get; set; }  
    }

    public class InOutByProduct
    {
        public int? status { get; set; }
        public string? location { get; set; }
        public DateTimeOffset? updateat { get; set; }
        public int? quantity { get; set; }
    }
}
