using quanlykhoupdate.common;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public interface IoutboundService
    {
        Task<PayLoad<inboundDTO>> Add(inboundDTO inboundDTO);
        Task<PayLoad<UpdateCodeInbound>> UpdateCode(UpdateCodeInbound inboundDTO);
        Task<PayLoad<string>> UpdateCodePack(string code);
        Task<PayLoad<object>> FindAll(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindAllNoIsActionOkpack(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindAllNoIsActionNoPack(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindAllOkIsActionNoPack(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindAllOkIsActionOkPack(int page = 1, int pageSize = 20);
    }
}
