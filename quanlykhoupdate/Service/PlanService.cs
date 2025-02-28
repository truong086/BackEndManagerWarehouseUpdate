using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;
using System.Drawing.Printing;
using System.Data;

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
                //var checkLocationOld = _context.location_addr.FirstOrDefault(x => x.code_location_addr == planDTO.location_old);
                //var checkLocationNew = _context.location_addr.FirstOrDefault(x => x.code_location_addr == planDTO.location_new);

                if(planDTO.location_old == planDTO.location_new)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATATONTAI));

                var checkLocationOld = checkDataLocationExsis(planDTO.areaOld, planDTO.lineOld, planDTO.shelfOld, planDTO.location_old);
                var checkLocationNew = checkDataLocationExsis(planDTO.areaNew, planDTO.lineNew, planDTO.shelfNew, planDTO.location_new);

                if (checkLocationOld == null ||  checkLocationNew == null)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATANULL));

                var checkLocationCodeOld = _context.product_location.FirstOrDefault(x => x.location_addr_id == checkLocationOld.id);
                var checkLocationCodeNew = _context.product_location.FirstOrDefault(x => x.location_addr_id == checkLocationNew.id);

                if (checkLocationOld == null || checkLocationNew == null || checkLocationCodeOld == null)
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
        
        private location_addr checkDataLocationExsis(string area, string line, string shelf, string code)
        {
            var dataItem = new location_addr();

            dataItem = _context.location_addr.FirstOrDefault(x => x.area == area && x.line == line && x.shelf == shelf && x.code_location_addr == code);
            if(dataItem != null)
            {
                return dataItem;
            }

            else
            {
                _context.location_addr.Add(new location_addr
                {
                    code_location_addr = code,
                    area = area,
                    line = line,
                    shelf = shelf
                });

                _context.SaveChanges();

                dataItem = _context.location_addr.OrderByDescending(x => x.id).FirstOrDefault();

                return dataItem == null ? null : dataItem;
            }
        } 
        public async Task<PayLoad<string>> Delete(int id)
        {
            try
            {
                var checkData = _context.plan.FirstOrDefault(x => x.id == id && x.status != 1);
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

        public async Task<PayLoad<object>> FindAll(int page = 1, int pageSize = 20)
        {
            try
            {
                var data = _context.plan.Where(x => x.status != 1).OrderByDescending(x => x.id).ToList();

                var pageList = new PageList<object>(loadData(data), page - 1, pageSize);

                if (pageList.Count <= 0)
                    pageList = new PageList<object>(loadData(data), 0, pageSize);
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAllDone(int page = 1, int pageSize = 20)
        {
            try
            {
                var data = _context.plan.Where(x => x.status == 1).OrderByDescending(x => x.id).ToList();

                var pageList = new PageList<object>(loadData(data), page - 1, pageSize);

                if (pageList.Count <= 0)
                    pageList = new PageList<object>(loadData(data), 0, pageSize);
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        private List<FindAllPlanData> loadData(List<plan> data)
        {
            var list = new List<FindAllPlanData>();

            foreach (var plan in data)
            {
                
                list.Add(findOneDataPlan(plan));
            }

            return list;
        }

        private FindAllPlanData findOneDataPlan(plan item)
        {
            
            var checkDataOld = _context.location_addr.Select(x => new
            {
                id = x.id,
                locationOld = x.code_location_addr,
                area = x.area,
                line = x.line,
                shelf = x.shelf,
            }).FirstOrDefault(x => x.id == item.location_addr_id_old);
            var checkDataNew = _context.location_addr.Select(x => new
            {
                id = x.id,
                locationNew = x.code_location_addr,
                area = x.area,
                line = x.line,
                shelf = x.shelf,
            }).FirstOrDefault(x => x.id == item.location_addr_id_new);

            return new FindAllPlanData
            {
                id = item.id,
                locationNew = checkDataNew.locationNew,
                areaNew = checkDataNew.area,
                lineNew = checkDataNew.line,
                shelfNew = checkDataNew.shelf,
                locationOld = checkDataOld.locationOld,
                areaOld = checkDataOld.area,
                lineOld = checkDataOld.line,
                shelfOld = checkDataOld.shelf,
                status = item.status,
                updateat = item.time
            };
        }
        public async Task<PayLoad<UpdatePlan>> Update(UpdatePlan planData)
        {
            try
            {
                var checkPlan = _context.plan.FirstOrDefault(x => x.id == planData.id && x.status != 1);
                if (checkPlan == null)
                    return await Task.FromResult(PayLoad<UpdatePlan>.CreatedFail(Status.DATANULL));

                if (planData.status == 1)
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

                //if (planData.status == 1)
                //{
                //    var checkLocaOld = _context.product_location.Include(x => x.location_Addrs).Where(x => x.location_addr_id == checkPlan.location_addr_id_old).FirstOrDefault();
                //    var checkLocationNew = _context.product_location.Include(x => x.location_Addrs).Where(x => x.location_addr_id == checkPlan.location_addr_id_new).FirstOrDefault();

                //    var checkLocationNewItem = _context.location_addr.FirstOrDefault(x => x.id == checkPlan.location_addr_id_new);
                //    var checkLocationOldItem = _context.location_addr.FirstOrDefault(x => x.id == checkPlan.location_addr_id_old);

                //    UpdateLocationData(checkLocaOld, checkLocationNewItem, checkLocationNew.quantity.Value);
                //    UpdateLocationData(checkLocationNew, checkLocationOldItem, checkLocaOld.quantity.Value);
                //    checkPlan.status = 1;
                //}
                //else
                //{
                //    checkPlan.status = planData.status;
                //}

                _context.plan.Update(checkPlan);
                _context.SaveChanges();

                return await Task.FromResult(PayLoad<UpdatePlan>.Successfully(planData));

            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<UpdatePlan>.CreatedFail(ex.Message));
            }
        }

        private void UpdateLocationData(List<product_location> data, location_addr location)
        {
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    item.location_addr_id = location.id;
                    item.location_Addrs = location;

                    _context.product_location.Update(item);
                    _context.SaveChanges();

                    saveUpdateHistory(item.product_id, location.id, item.quantity.Value);

                }
            }
        }

        private void saveUpdateHistory(int? productId, int locationAddrId, int quantity)
        {
            var updateHistory = new update_history
            {
                product_id = productId,
                location_addr_id = locationAddrId,
                quantity = quantity,
                status = 2,
                last_modify_date = DateTime.UtcNow
            };

            _context.update_history.Add(updateHistory);
            _context.SaveChanges();
        }

        //private void UpdateLocationData(product_location data, location_addr location, int? quantity)
        //{
        //    if (data != null)
        //    {
        //        data.location_addr_id = location.id;
        //        data.location_Addrs = location;
        //        data.quantity = quantity;

        //        _context.product_location.Update(data);
        //        _context.SaveChanges();
        //    }
        //}

        public async Task<PayLoad<object>> FindOne(int id)
        {
            try
            {
                var data = _context.plan.FirstOrDefault(x => x.id == id);

                return await Task.FromResult(PayLoad<object>.Successfully(findOneDataPlan(data)));
            }catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public byte[] FindAllDownLoadExcel(searchDatetimePlan data)
        {
            //DateTimeOffset dateFromUtc = data.datefrom.Value.ToUniversalTime();
            //DateTimeOffset dateToUtc = data.dateto.Value.ToUniversalTime(); // Lấy hết ngày
            var dateFromUtc = data.datefrom.Value.AddHours(-8);
            var dateToUtc = data.dateto.Value.AddHours(-8);

            var list = _context.plan.Where(x => x.time >= dateFromUtc && x.time <= dateToUtc).ToList();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Area New";
                worksheet.Cells[1, 3].Value = "Line New";
                worksheet.Cells[1, 4].Value = "Shelf New";
                worksheet.Cells[1, 5].Value = "Code New";
                worksheet.Cells[1, 6].Value = "Area Old";
                worksheet.Cells[1, 7].Value = "Line Old";
                worksheet.Cells[1, 8].Value = "Shelf Old";
                worksheet.Cells[1, 9].Value = "Code Old";

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true; // Chữ in đậm
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Nền đặc
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Nền xám nhạt
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Căn giữa nội dung
                }

                // Đổ dữ liệu vào file Excel
                int row = 2;
                foreach (var product in loadData(list))
                {
                    worksheet.Cells[row, 1].Value = product.id;
                    worksheet.Cells[row, 2].Value = product.areaNew;
                    worksheet.Cells[row, 3].Value = product.lineNew;
                    worksheet.Cells[row, 4].Value = product.shelfNew;
                    worksheet.Cells[row, 5].Value = product.locationNew;

                    worksheet.Cells[row, 6].Value = product.areaOld;
                    worksheet.Cells[row, 7].Value = product.lineOld;
                    worksheet.Cells[row, 8].Value = product.shelfOld;
                    worksheet.Cells[row, 9].Value = product.locationOld;

                    row++;
                }

                worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
                return package.GetAsByteArray();
            }
        }

        public Task<PayLoad<object>> FindAllNoDone(int page = 1, int pageSize = 20)
        {
            throw new NotImplementedException();
        }

        public async Task<PayLoad<PlanDTO>> UpdateData(int id, PlanDTO planDTO)
        {
            try
            {
                if (planDTO.location_old == planDTO.location_new)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATATONTAI));

                var checkPlanId = _context.plan.FirstOrDefault(x => x.id == id);
                
                if(checkPlanId == null)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATANULL));

                var checkLocationOld = checkDataLocationExsis(planDTO.areaOld, planDTO.lineOld, planDTO.shelfOld, planDTO.location_old);
                var checkLocationNew = checkDataLocationExsis(planDTO.areaNew, planDTO.lineNew, planDTO.shelfNew, planDTO.location_new);

                if (checkLocationOld == null || checkLocationNew == null)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATANULL));

                var checkLocationCodeOld = _context.product_location.FirstOrDefault(x => x.location_addr_id == checkLocationOld.id);
                var checkLocationCodeNew = _context.product_location.FirstOrDefault(x => x.location_addr_id == checkLocationNew.id);

                if (checkLocationOld == null || checkLocationNew == null || checkLocationCodeOld == null)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATANULL));

                var checkPlan = _context.plan.Where(x => (x.location_addr_id_old == checkLocationOld.id
                || x.location_addr_id_new == checkLocationNew.id || x.location_addr_id_old == checkLocationNew.id ||
                x.location_addr_id_new == checkLocationOld.id) && x.status != 1 && x.id != checkPlanId.id).FirstOrDefault();

                if (checkPlan != null)
                    return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(Status.DATATONTAI));

                checkPlanId.location_addr_id_new = checkLocationNew.id;
                checkPlanId.location_Addr_New = checkLocationNew;
                checkPlanId.location_addr_id_old = checkLocationOld.id;
                checkPlanId.location_Addr_Old = checkLocationOld;

                _context.plan.Update(checkPlanId);

                _context.SaveChanges();
                await _userTokenService.SendNotify();

                return await Task.FromResult(PayLoad<PlanDTO>.Successfully(planDTO));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<PlanDTO>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAllDataByDone(searchDataPost datas)
        {
            try
            {
                // Chỉ lấy phần ngày, bỏ qua múi giờ
                // Chuyển DateTimeOffset? sang DateTime, bỏ múi giờ
                //DateTime dateFrom = datas.datefrom?.UtcDateTime.Date ?? DateTime.UtcNow.Date;
                //DateTime dateTo = datas.dateto?.UtcDateTime.Date.AddHours(1) ?? DateTime.UtcNow.Date.AddHours(1);

                //DateTimeOffset dateFromUtc = datas.datefrom.Value.ToOffset(TimeSpan.FromHours(8));
                //DateTimeOffset dateToUtc = datas.dateto.Value.ToOffset(TimeSpan.FromHours(8)); // Lấy hết ngày

                var dateFromUtc = datas.datefrom.Value.AddHours(-8);
                var dateToUtc = datas.dateto.Value.AddHours(-8);
                var data = await _context.plan
                    .Where(x => x.time >= dateFromUtc && x.time <= dateToUtc && x.status == 1)
                    .OrderByDescending(x => x.id)
                    .ToListAsync();

                var pageList = new PageList<object>(loadData(data), datas.page - 1, datas.pageSize);
                if (pageList.Count <= 0)
                    pageList = new PageList<object>(loadData(data), 0, datas.pageSize);
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    datas.page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindByDataAreaLineShelf(SearchAreaLineShelf data)
        {
            try
            {
                var checkData = _context.location_addr.Where(x => x.area == data.area && x.line == data.line && x.shelf == data.shelf).Select(x => x.code_location_addr).ToList();
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = checkData
                }));
            }catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindByDataTimeAll(searchDatetimePlan data, int page = 1, int pageSize = 20)
        {
            try
            {
                var dataList = _context.plan.OrderByDescending(x => x.id).ToList();

                var pageList = new PageList<object>(loadData(dataList), page - 1, pageSize);

                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> checkPlanLocationAdd(string code)
        {
            try
            {
                var checkData = _context.location_addr.FirstOrDefault(x => x.code_location_addr == code);
                

                var checkPlan = _context.plan.FirstOrDefault(x => (x.location_addr_id_old == checkData.id || x.location_addr_id_new == checkData.id) && x.status != 1);
                if(checkPlan != null) return await Task.FromResult(PayLoad<object>.CreatedFail(Status.DATATONTAI));

                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    Status.SUCCESS
                }));

            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    Status.SUCCESS
                }));
            }
        }

        public async Task<PayLoad<object>> checkPlanLocationUpdate(int id, string code)
        {
            try
            {
                var checkIdPlan = _context.plan.FirstOrDefault(x => x.id == id);
                if (checkIdPlan == null) return await Task.FromResult(PayLoad<object>.CreatedFail(Status.DATATONTAI));

                var checkData = _context.location_addr.FirstOrDefault(x => x.code_location_addr == code);

                var checkPlan = _context.plan.FirstOrDefault(x => (x.location_addr_id_old == checkData.id || x.location_addr_id_new == checkData.id) && x.id != checkIdPlan.id && x.status != 1);
                if (checkPlan != null) return await Task.FromResult(PayLoad<object>.CreatedFail(Status.DATATONTAI));

                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    Status.SUCCESS
                }));

            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    Status.SUCCESS
                }));
            }
        }

        public async Task<PayLoad<object>> FindAllDataByNoDone(searchDataPost datas)
        {
            try
            {
                // Chuyển datefrom và dateto về UTC (nếu dữ liệu trong DB lưu UTC)
                //DateTimeOffset dateFromUtc = datas.datefrom.Value.ToOffset(TimeSpan.FromHours(0));
                //DateTimeOffset dateToUtc = datas.dateto.Value.ToOffset(TimeSpan.FromHours(0)); // Lấy hết ngày

                var dateFromUtc = datas.datefrom.Value.AddHours(-8);
                var dateToUtc = datas.dateto.Value.AddHours(-8);
                // Lọc dữ liệu trong khoảng thời gian
                var data = await _context.plan
                    .Where(x => x.time >= dateFromUtc && x.time <= dateToUtc && x.status != 1)
                    .OrderByDescending(x => x.id)
                    .ToListAsync();

                var pageList = new PageList<object>(loadData(data), datas.page - 1, datas.pageSize);
                if(pageList.Count <= 0)
                    pageList = new PageList<object>(loadData(data), 0, datas.pageSize);
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    datas.page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAllDataByNoDoneAndDone(searchDataPost datas)
        {
            try { 
                // Chuyển datefrom và dateto về UTC (nếu dữ liệu trong DB lưu UTC)
                //DateTimeOffset dateFromUtc = datas.datefrom.Value.ToOffset(TimeSpan.FromHours(0));
                //DateTimeOffset dateToUtc = datas.dateto.Value.ToOffset(TimeSpan.FromHours(0)); // Lấy hết ngày

                var dateFromUtc = datas.datefrom.Value.AddHours(-8);
                var dateToUtc = datas.dateto.Value.AddHours(-8);
                // Lọc dữ liệu trong khoảng thời gian
                var data = await _context.plan
                    .Where(x => x.time >= dateFromUtc && x.time <= dateToUtc)
                    .OrderByDescending(x => x.id)
                    .ToListAsync();

                var pageList = new PageList<object>(loadData(data), datas.page - 1, datas.pageSize);
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    datas.page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAllNoDoneAndDone(int page = 1, int pageSize = 20)
        {
            try
            {
                var data = _context.plan.OrderByDescending(x => x.id).ToList();

                var pageList = new PageList<object>(loadData(data), page - 1, pageSize);
                if (pageList.Count <= 0)
                    pageList = new PageList<object>(loadData(data), 0, pageSize);
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }
    }
}
