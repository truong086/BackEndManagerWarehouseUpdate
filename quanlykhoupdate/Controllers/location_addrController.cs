using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quanlykhoupdate.common;
using quanlykhoupdate.Service;
using System.IO;

namespace quanlykhoupdate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class location_addrController : ControllerBase
    {
        private readonly Ilocation_addrService _location_addrService;
        public location_addrController(Ilocation_addrService location_addrService)
        {
            _location_addrService = location_addrService;
        }

        [HttpGet]
        [Route(nameof(SearchData))]
        public async Task<PayLoad<object>> SearchData(string? name, int page = 1, int pageSize = 20)
        {
            return await _location_addrService.SearchData(name, page, pageSize);
        }

        [HttpGet]
        [Route(nameof(dowloadexcel))]
        public IActionResult dowloadexcel(string code)
        {
            byte[] data = _location_addrService.FindAllDataByDateExcel(code);

            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DataExcel.xlsx");
        }
    }
}
