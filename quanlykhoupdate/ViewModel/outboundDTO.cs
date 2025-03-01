namespace quanlykhoupdate.ViewModel
{
    public class outboundDTO
    {
        public List<productListOutbound>? productListOutbounds { get; set; }

    }

    public class productListOutbound
    {
        public string? productTitle { get; set; }
        public int? quantity { get; set; }
    }

    //public class findAllOutbound
    //{
    //    public int? id { get; set; }
    //    public int? quantity { get; set; }
    //    public int? quantityProduct { get; set; }
    //    public string? code { get; set; }
    //    public bool? isAction { get; set; }
    //    public bool? isPack { get; set; }
    //    public List<LocationDataInbound>? locationDataInbounds { get; set; }    
    //}
    //public class LocationDataOutbound
    //{
    //    public int? id { get; set; }
    //    public string? title { get; set; }
    //    public string? code { get; set; }
    //    public string? area { get; set; }
    //    public string? line { get; set; }
    //    public string? shelf { get; set; }
    //    public int? quantity { get; set; }
    //}
}
