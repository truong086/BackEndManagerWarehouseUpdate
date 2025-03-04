using quanlykhoupdate.common;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public interface Ilocation_addrService
    {
        Task<PayLoad<object>> SearchData(string? name, int page = 1, int pageSize = 10);
        Task<PayLoad<object>> FindAllData();
        Task<PayLoad<object>> FindAllDataLine(string area);
        Task<PayLoad<object>> FindAllDataShelf(string line, string area);
        Task<PayLoad<object>> FindAllDataLocation(string line, string area, string shelf);
        public byte[] FindAllDataByDateExcel(string code);

        Task<PayLoad<object>> FindAllDataShelfOne(string line, string area);
        Task<PayLoad<object>> FindAllDataDashBoad(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindAllDataDashBoadSearch(FindAllDataDashBoard data);
    }
}
