using quanlykhoupdate.common;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public interface ISupplierService
    {
        Task<PayLoad<SupplierDTO>> Add(SupplierDTO supplierDTO);

        Task<PayLoad<object>> FindAll();
    }
}