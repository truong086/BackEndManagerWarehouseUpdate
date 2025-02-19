namespace quanlykhoupdate.ViewModel
{
    public class PlanDTO
    {
        public string?location_new { get; set; }
        public string? location_old { get; set; }
    }

    public class UpdatePlan
    {
        public int id { get; set; }
        public int status { get; set; }
    }

    public class FindAllPlanData
    {
        public int id { get; set; }
        public string? locationOld { get; set; }
        public string? locationNew { get; set; }
    }
}
