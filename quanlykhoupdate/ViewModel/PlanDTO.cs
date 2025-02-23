namespace quanlykhoupdate.ViewModel
{
    public class PlanDTO
    {
        public string?location_new { get; set; }
        public string? location_old { get; set; }
        public string? areaOld { get; set; }
        public string? lineOld { get; set; }
        public string? shelfOld { get; set; }
        public string? areaNew { get; set; }
        public string? lineNew { get; set; }
        public string? shelfNew { get; set; }
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
        public string? areaOld { get; set; }
        public string? lineOld { get; set; }
        public string? shelfOld { get; set; }
        public string? locationNew { get; set; }
        public string? areaNew { get; set; }
        public string? lineNew { get; set; }
        public string? shelfNew { get; set; }
        public int? status { get; set; }
        public DateTimeOffset? updateat { get; set; }
    }

    public class searchDatetimePlan
    {
        public DateTimeOffset? datefrom { get; set; }
        public DateTimeOffset? dateto { get; set; }
    }

    public class searchDataPost
    {
        public int page { get; set; } = 1;
        public int pageSize { get; set; } = 20;
        public DateTimeOffset? datefrom { get; set; }
        public DateTimeOffset? dateto { get; set; }
    }
}
