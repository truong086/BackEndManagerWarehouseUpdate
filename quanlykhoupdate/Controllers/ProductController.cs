using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quanlykhoupdate.common;
using quanlykhoupdate.Service;
using System.Drawing.Imaging;

namespace quanlykhoupdate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Route(nameof(FindAll))]
        public async Task<PayLoad<object>> FindAll(string? name,int page = 1, int pageSize = 20)
        {
            return await _productService.findAll(name, page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindOne))]
        public async Task<PayLoad<object>> FindOne(string? name, int page = 1, int pageSize = 20)
        {
            return await _productService.findOne(name, page, pageSize);
        }

        [HttpGet]
        [Route(nameof(findOneByOutAndIn))]
        public async Task<PayLoad<object>> findOneByOutAndIn(int id)
        {
            return await _productService.findOneByOutAndIn(id);
        }

        [HttpGet]
        [Route(nameof(findBySuppliers))]
        public async Task<PayLoad<object>> findBySuppliers(int id, int page = 1, int pageSize = 20)
        {
            return await _productService.findBySuppliers(id, page, pageSize);
        }

        [HttpPost]
        [Route(nameof(FindAllDownLoadExcel))]
        public IActionResult FindAllDownLoadExcel(int id)
        {
            byte[] data = _productService.FindAllDownLoadExcel(id);

            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DataExcelPlanProduct.xlsx");
        }

        [HttpPost]
        [Route(nameof(FindAllDownLoadExcelByCodeProduct))]
        public IActionResult FindAllDownLoadExcelByCodeProduct(string code)
        {
            byte[] data = _productService.FindAllDownLoadExcelByCodeProduct(code);

            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DataExcelPlanProducts.xlsx");
        }

        [HttpPost]
        [Route(nameof(FindAllDownLoadExcelByCodeProductList))]
        public IActionResult FindAllDownLoadExcelByCodeProductList(List<string> code)
        {
            byte[] data = _productService.FindAllDownLoadExcelByCodeProductList(code);

            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DataExcelPlanProduct.xlsx");
        }

        [HttpGet]
        [Route(nameof(findOneCode))]
        public async Task<PayLoad<object>> findOneCode(string? name)
        {
            return await _productService.findOneCode(name);
        }

        [HttpGet]
        [Route(nameof(AddDataSupplier))]
        public async Task<PayLoad<string>> AddDataSupplier()
        {
            return await _productService.AddDataSupplier();
        }

        [HttpPost]
        [Route(nameof(ImportDataExcel))]
        public async Task<PayLoad<object>> ImportDataExcel(IFormFile file)
        {
            return await _productService.ImportDataExcel(file);
        }
    }
}
