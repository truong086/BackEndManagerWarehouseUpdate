using quanlykhoupdate.common;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public interface IUserTokenService
    {
        Task<PayLoad<UserToKenAppDTO>> Add(UserToKenAppDTO add);
        Task<PayLoad<string>> SendNotify();
    }
}
