﻿using quanlykhoupdate.common;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public interface IPlanService
    {
        Task<PayLoad<PlanDTO>> Add(PlanDTO planDTO);
        Task<PayLoad<PlanDTO>> UpdateData(int id, PlanDTO planDTO);
        Task<PayLoad<UpdatePlan>> Update(UpdatePlan planData);
        Task<PayLoad<string>> Delete(int id);
        Task<PayLoad<object>> FindAll(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindOne(int id);
        Task<PayLoad<object>> FindAllDone(int page = 1, int pageSize = 20);
        Task<PayLoad<object>> FindAllNoDone(int page = 1, int pageSize = 20);
        byte[] FindAllDownLoadExcel(searchDatetimePlan data);

    }
}
