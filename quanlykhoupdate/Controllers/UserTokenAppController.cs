using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quanlykhoupdate.common;
using quanlykhoupdate.Service;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTokenAppController : ControllerBase
    {
        private readonly IUserTokenService _userTokenService;
        public UserTokenAppController(IUserTokenService userTokenService)
        {
            _userTokenService = userTokenService;
        }

        [HttpPost]
        [Route(nameof(Add))]
        public async Task<PayLoad<UserToKenAppDTO>> Add(UserToKenAppDTO data)
        {
            return await _userTokenService.Add(data);
        }
    }
}
