using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quanlykhoupdate.common;
using quanlykhoupdate.Service;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;
        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpPost]
        [Route(nameof(Add))]
        public async Task<PayLoad<PlanDTO>> Add(PlanDTO add)
        {
            return await _planService.Add(add);
        }

        [HttpPut]
        [Route(nameof(Update))]
        public async Task<PayLoad<UpdatePlan>> Update(UpdatePlan add)
        {
            return await _planService.Update(add);
        }

        [HttpGet]
        [Route(nameof(FindAll))]
        public async Task<PayLoad<object>> FindAll(int page = 1, int pageSize = 20)
        {
            return await _planService.FindAll(page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindAllDone))]
        public async Task<PayLoad<object>> FindAllDone(int page = 1, int pageSize = 20)
        {
            return await _planService.FindAllDone(page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindOne))]
        public async Task<PayLoad<object>> FindOne(int id)
        {
            return await _planService.FindOne(id);
        }

        [HttpPost]
        [Route(nameof(FindAllDownLoadExcel))]
        public IActionResult FindAllDownLoadExcel(searchDatetimePlan data)
        {
            byte[] dataFile = _planService.FindAllDownLoadExcel(data);

            return File(dataFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DataExcelPlan.xlsx");
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<PayLoad<string>> Delete(int id)
        {
            return await _planService.Delete(id);
        }

        [HttpPut]
        [Route(nameof(UpdateData))]
        public async Task<PayLoad<PlanDTO>> UpdateData(int id, PlanDTO data)
        {
            return await _planService.UpdateData(id, data);
        }
    }
}
