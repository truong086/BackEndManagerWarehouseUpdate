using quanlykhoupdate.Models;

namespace quanlykhoupdate.ViewModel
{
    public class Location_addrDTO
    {
        public int? id { get; set; }
        public string? code { get; set; }
        public string? title { get; set; }  
        public string? area { get; set; }
        public string? line { get; set; }
        public string? shelf { get; set; }
        public int? quantity { get; set; }
        public List<object>? history { get; set; }
    }

    public class Location_addrGetAll
    {
        public string? area { get; set; }
        public List<dataLineShelfByArea>? dataLineShelfByAreas { get; set; }
    }

    public class dataLineShelfByArea
    {
        public string? line { get; set; }
        public List<dataShelByLine>? dataShelByLines { get; set; }
        
    }

    public class dataShelByLine
    {
        public string? shelf { get; set; }
        public List<string>? location { get; set; }
        public List<productbyShelf>? productbyShelf { get; set; }
        public List<listProductByLocationData>? listProductByLocationDatas { get; set; }
    }

    public class productbyShelf
    {
        public string? location { get; set;}
        public string? title { get; set;}
        public int? quantity { get; set;}
    }

    public class listProductByLocationData
    {
        public List<productbyShelf>? productbyShelf { get; set; }
    }

    public class findAllAreaPlan
    {
        public List<string>? areas { get; set; }
    }

    public class findAllLocationAndPrduct
    {
        public location_addr? location_Addrs { get; set; }
        public product? products { get; set; }
    }
}
