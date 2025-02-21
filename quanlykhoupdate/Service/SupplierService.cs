using OfficeOpenXml.Style.XmlAccess;
using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public class SupplierService : ISupplierService
    {
        private readonly DBContext _context;
        public SupplierService(DBContext context)
        {
            _context = context;
        }
        public async Task<PayLoad<SupplierDTO>> Add(SupplierDTO supplierDTO)
        {
            try
            {
                var checkData = _context.supplier.FirstOrDefault(x => x.name == supplierDTO.name && x.title == supplierDTO.title);
                if(checkData != null)
                    return await Task.FromResult(PayLoad<SupplierDTO>.CreatedFail(Status.DATANULL));

                _context.supplier.Add(new supplier
                {
                    name = supplierDTO.name,
                    title = supplierDTO.title
                });

                _context.SaveChanges();

                return await Task.FromResult(PayLoad<SupplierDTO>.Successfully(supplierDTO));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<SupplierDTO>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAll()
        {
            try
            {
                var data = _context.supplier.ToList();

                return await Task.FromResult(PayLoad<object>.Successfully(data));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }
    }
}
