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
        public async Task<PayLoad<object>> FindAll()
        {
            return await _planService.FindAll();
        }

        [HttpDelete]
        [Route(nameof(Delete))]
        public async Task<PayLoad<string>> Delete(int id)
        {
            return await _planService.Delete(id);
        }
    }
}
