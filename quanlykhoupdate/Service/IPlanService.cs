using quanlykhoupdate.common;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public interface IPlanService
    {
        Task<PayLoad<PlanDTO>> Add(PlanDTO planDTO);
        Task<PayLoad<UpdatePlan>> Update(UpdatePlan planData);
        Task<PayLoad<string>> Delete(int id);
        Task<PayLoad<object>> FindAll();
        Task<PayLoad<object>> FindAllDone();

    }
}
