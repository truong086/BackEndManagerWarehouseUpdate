using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quanlykhoupdate.common;
using quanlykhoupdate.Service;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : ControllerBase
    {
        private readonly ISupplierService _supplierService;
        public SupplierController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        [HttpPost]
        [Route(nameof(Add))]
        public async Task<PayLoad<SupplierDTO>> Add(SupplierDTO supplierDTO)
        {
            return await _supplierService.Add(supplierDTO);
        }

        [HttpGet]
        [Route(nameof(FindAll))]
        public async Task<PayLoad<object>> FindAll()
        {
            return await _supplierService.FindAll();
        }
    }
}
