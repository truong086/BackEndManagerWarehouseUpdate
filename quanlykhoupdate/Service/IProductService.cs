using quanlykhoupdate.common;

namespace quanlykhoupdate.Service
{
    public interface IProductService
    {
        Task<PayLoad<object>> findAll(string? name, int page = 1, int pageSize = 20);
        Task<PayLoad<object>> findOne(string? name);
        Task<PayLoad<object>> findOneCode(string? name);
    }
}
