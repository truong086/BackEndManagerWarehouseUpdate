namespace quanlykhoupdate.ViewModel
{
    public class AreaDashboard
    {
        public string? Area { get; set; }
        public List<LineArea>? Line { get; set; }
    }

    public class LineArea
    {
        public string? Line { get; set;}
        public List<ShelfLineArea>? Shelf { get; set;}
    }

    public class ShelfLineArea
    {
        public string? shelf { get; set; }
        public bool? isProduct { get; set; }
    }
}
