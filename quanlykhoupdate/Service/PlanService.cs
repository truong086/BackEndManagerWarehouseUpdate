using Microsoft.EntityFrameworkCore;
using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public class PlanService : IPlanService
    {
        private readonly DBContext _context;
        private readonly IUserTokenService _userTokenService;
        public PlanService(DBContext context, IUserTokenService userTokenService)
        {
            _context = context;
            _userTokenService = userTokenService;

        }
        public async Task<PayLoad<PlanDTO>> Add(PlanDTO planDTO)
        {
            try
            {
                var checkLocationOld = _context.location_addr.FirstOrDefault(x => x.code_location_addr == planDTO.location_old);
                var checkLocationNew = _context.location_addr.FirstOrDefault(x => x.code_location_addr == planDTO.location_new);


                var checkLocationCodeOld = _context.product_location.FirstOrDefault(x => x.location_addr_id == checkLocationOld.id);
                var checkLocationCodeNew = _context.product_location.FirstOrDefault(x => x.location_addr_id == checkLocationNew.id);

                if (checkLocationOld == null || checkLocationNew == null || checkLocationCodeOld == null || checkLocationCodeNew == null)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATANULL));

                var checkPlan = _context.plan.Where(x => (x.location_addr_id_old == checkLocationOld.id
                || x.location_addr_id_new == checkLocationNew.id || x.location_addr_id_old == checkLocationNew.id ||
                x.location_addr_id_new == checkLocationOld.id) && x.status != 1).FirstOrDefault();

                if(checkPlan != null)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATATONTAI));

                _context.plan.Add(new plan
                {
                    location_addr_id_new = checkLocationNew.id,
                    location_addr_id_old = checkLocationOld.id,
                    location_Addr_New = checkLocationNew,
                    location_Addr_Old = checkLocationOld,
                    status = 0,
                    time = DateTimeOffset.UtcNow
                });

                _context.SaveChanges();
               await _userTokenService.SendNotify();

                return await Task.FromResult(PayLoad<PlanDTO>.Successfully(planDTO));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<string>> Delete(int id)
        {
            try
            {
                var checkData = _context.plan.FirstOrDefault(x => x.id == id);
                if(checkData == null)
                    return await Task.FromResult(PayLoad<string>.CreatedFail(Status.DATANULL));

                _context.plan.Remove(checkData);
                _context.SaveChanges();

                return await Task.FromResult(PayLoad<string>.Successfully(Status.SUCCESS));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<string>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAll()
        {
            try
            {
                var data = _context.plan.Where(x => x.status != 1).ToList();

                return await Task.FromResult(PayLoad<object>.Successfully(loadData(data)));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public Task<PayLoad<object>> FindAllDone()
        {
            throw new NotImplementedException();
        }

        private List<FindAllPlanData> loadData(List<plan> data)
        {
            var list = new List<FindAllPlanData>();

            foreach (var plan in data)
            {
                var checkDataOld = _context.location_addr.Select(x => new
                {
                    id = x.id,
                    locationOld = x.code_location_addr
                }).FirstOrDefault(x => x.id == plan.location_addr_id_old);
                var checkDataNew = _context.location_addr.Select(x => new
                {
                    id = x.id,
                    locationNew = x.code_location_addr
                }).FirstOrDefault(x => x.id == plan.location_addr_id_new);

                list.Add(new FindAllPlanData
                {
                    id = plan.id,
                    locationNew = checkDataNew.locationNew,
                    locationOld = checkDataOld.locationOld
                });
            }

            return list;
        }
        public async Task<PayLoad<UpdatePlan>> Update(UpdatePlan planData)
        {
            try
            {
                var checkPlan = _context.plan.FirstOrDefault(x => x.id == planData.id && x.status != 1);
                if(checkPlan == null)
                    return await Task.FromResult(PayLoad<UpdatePlan>.CreatedFail(Status.DATANULL));

                if(planData.status == 1)
                {
                    var checkLocaOld = _context.product_location.Include(x => x.location_Addrs).Where(x => x.location_addr_id == checkPlan.location_addr_id_old).ToList();
                    var checkLocationNew = _context.product_location.Include(x => x.location_Addrs).Where(x => x.location_addr_id == checkPlan.location_addr_id_new).ToList();

                    var checkLocationNewItem = _context.location_addr.FirstOrDefault(x => x.id == checkPlan.location_addr_id_new);
                    var checkLocationOldItem = _context.location_addr.FirstOrDefault(x => x.id == checkPlan.location_addr_id_old);

                    UpdateLocationData(checkLocaOld, checkLocationNewItem);
                    UpdateLocationData(checkLocationNew, checkLocationOldItem);
                    checkPlan.status = 1;
                }
                else
                {
                    checkPlan.status = planData.status;
                }

                _context.plan.Update(checkPlan);
                _context.SaveChanges();

                return await Task.FromResult(PayLoad<UpdatePlan>.Successfully(planData));

            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<UpdatePlan>.CreatedFail(ex.Message));
            }
        }

        private void UpdateLocationData(List<product_location> data, location_addr location)
        {
            if(data.Count > 0)
            {
                foreach(var item in data)
                {
                    item.location_addr_id = location.id;
                    item.location_Addrs = location;

                    _context.product_location.Update(item);
                    _context.SaveChanges();

                }
            }
        }
    }
}
