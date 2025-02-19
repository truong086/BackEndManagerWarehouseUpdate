using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quanlykhoupdate.common;
using quanlykhoupdate.Service;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InboundController : ControllerBase
    {
        private readonly IinboundService _inboundService;
        public InboundController(IinboundService iinboundService)
        {
            _inboundService = iinboundService;
        }

        [HttpPost]
        [Route(nameof(Add))]
        public async Task<PayLoad<inboundDTO>> Add(inboundDTO inboundDTO)
        {
           return await _inboundService.Add(inboundDTO);
        }

        [HttpPut]
        [Route(nameof(Update))]
        public async Task<PayLoad<UpdateCodeInbound>> Update(UpdateCodeInbound inboundDTO)
        {
            return await _inboundService.UpdateCode(inboundDTO);
        }

        [HttpGet]
        [Route(nameof(FindAll))]
        public async Task<PayLoad<object>> FindAll(int page = 1, int pageSize = 20)
        {
            return await _inboundService.FindAll(page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindAllNoIsAction))]
        public async Task<PayLoad<object>> FindAllNoIsAction(int page = 1, int pageSize = 20)
        {
            return await _inboundService.FindAllNoIsAction(page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindCode))]
        public async Task<PayLoad<object>> FindCode(string code)
        {
            return await _inboundService.FindCode(code);
        }
    }
}
