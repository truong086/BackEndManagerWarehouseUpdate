using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quanlykhoupdate.common;
using quanlykhoupdate.Service;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OutboundController : ControllerBase
    {
        private readonly IoutboundService _ioutboundService;
        public OutboundController(IoutboundService ioutboundService)
        {
            _ioutboundService = ioutboundService;
        }

        [HttpPost]
        [Route(nameof(Add))]
        public async Task<PayLoad<inboundDTO>> Add (inboundDTO data)
        {
            return await _ioutboundService.Add(data);
        }

        [HttpPut]
        [Route(nameof(UpdateCode))]
        public async Task<PayLoad<UpdateCodeInbound>> UpdateCode(List<UpdateCodeInbound> data)
        {
            return await _ioutboundService.UpdateCode(data);
        }

        [HttpPut]
        [Route(nameof(UpdatePack))]
        public async Task<PayLoad<string>> UpdatePack(string code)
        {
            return await _ioutboundService.UpdateCodePack(code);
        }

        [HttpGet]
        [Route(nameof(FindAll))]
        public async Task<PayLoad<object>> FindAll(int page = 1, int pageSize = 20)
        {
            return await _ioutboundService.FindAll(page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindAllNoIsActionOkpack))]
        public async Task<PayLoad<object>> FindAllNoIsActionOkpack(int page = 1, int pageSize = 20)
        {
            return await _ioutboundService.FindAllNoIsActionOkpack(page, pageSize);
        }


        [HttpGet]
        [Route(nameof(FindAllOkIsActionNoPack))]
        public async Task<PayLoad<object>> FindAllOkIsActionNoPack(int page = 1, int pageSize = 20)
        {
            return await _ioutboundService.FindAllOkIsActionNoPack(page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindAllOkIsActionOkPack))]
        public async Task<PayLoad<object>> FindAllOkIsActionOkPack(int page = 1, int pageSize = 20)
        {
            return await _ioutboundService.FindAllOkIsActionOkPack(page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindAllNoIsActionNoPack))]
        public async Task<PayLoad<object>> FindAllNoIsActionNoPack(int page = 1, int pageSize = 20)
        {
            return await _ioutboundService.FindAllNoIsActionNoPack(page, pageSize);
        }

        [HttpGet]
        [Route(nameof(FindCode))]
        public async Task<PayLoad<object>> FindCode(string code)
        {
            return await _ioutboundService.FindCode(code);
        }
    }
}
