using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quanlykhoupdate.common;
using quanlykhoupdate.Service;

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
        public async Task<PayLoad<object>> FindOne(string? name)
        {
            return await _productService.findOne(name);
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
