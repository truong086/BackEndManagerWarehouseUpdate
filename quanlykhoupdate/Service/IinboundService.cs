using quanlykhoupdate.common;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public interface IinboundService
    {
        Task<PayLoad<inboundDTO>> Add(inboundDTO inboundDTO);
        Task<PayLoad<UpdateCodeInbound>> UpdateCode(UpdateCodeInbound inboundDTO);
        Task<PayLoad<object>> FindAll(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindAllNoIsAction(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindCode(string code);

    }
}
