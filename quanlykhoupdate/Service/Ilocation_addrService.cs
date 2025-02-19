using quanlykhoupdate.common;

namespace quanlykhoupdate.Service
{
    public interface Ilocation_addrService
    {
        Task<PayLoad<object>> SearchData(string? name, int page = 1, int pageSize = 10);
        public byte[] FindAllDataByDateExcel(string code);
    }
}
