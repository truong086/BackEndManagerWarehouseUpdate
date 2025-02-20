using Microsoft.EntityFrameworkCore;
using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public class ProductService : IProductService
    {
        private readonly DBContext _context;
        public ProductService(DBContext context)
        {
            _context = context;
        }
        public async Task<PayLoad<object>> findAll(string? name, int page = 1, int pageSize = 20)
        {
            try
            {
                var data = _context.product.Select(x => new
                {
                    title = x.title,
                    id = x.id,
                }).ToList();

                if (!string.IsNullOrEmpty(name))
                    data = data.Where(x => x.title.Contains(name)).ToList();

                var pageList = new PageList<object>(data, page - 1, pageSize);

                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    page,
                    pageList.pageSize,
                    pageList.totalCounts,
                    pageList.totalPages
                }));

            }catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> findOne(string? name)
        {
            try
            {
                var checkData = _context.product.FirstOrDefault(x => x.title == name);
                if(checkData == null)
                    return await Task.FromResult(PayLoad<object>.CreatedFail(Status.DATANULL));

                return await Task.FromResult(PayLoad<object>.Successfully(loadDataFindOne(checkData)));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> findOneCode(string? name)
        {
            try
            {
                var checkDataCode = _context.location_addr.FirstOrDefault(x => x.code_location_addr == name);

                if(checkDataCode == null)
                    return await Task.FromResult(PayLoad<object>.CreatedFail(Status.DATANULL));

                var checkDataProductLocation = _context.product_location.Include(p => p.products).Where(x => x.location_addr_id == checkDataCode.id).ToList();

                return await Task.FromResult(PayLoad<object>.Successfully(loadDataCode(checkDataProductLocation)));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        private List<dataProductLocation> loadDataCode(List<product_location> data)
        {
            var list = new List<dataProductLocation>();


            foreach(var item in data)
            {
                if(item.products != null)
                    list.Add(loadDataFindOne(item.products));
            }

            return list;
        }

        private dataProductLocation loadDataFindOne(product data)
        {
            var checkData = _context.product.Where(x => x.id == data.id)
                .Include(pl => pl.product_Locations)
                .ThenInclude(l => l.location_Addrs)
                .AsNoTracking()
                .Select(x => new dataProductLocation
                {
                    Id = x.id,
                    title = x.title,
                    dataLocations = x.product_Locations.Select(lc => new dataLocation
                    {
                        code = lc.location_Addrs.code_location_addr,
                        area = lc.location_Addrs.area,
                        line = lc.location_Addrs.line,
                        shelf = lc.location_Addrs.shelf,
                        quantity = lc.quantity

                    }).ToList(),
                    history = loadDataHistory(data)
                }).FirstOrDefault();

            //var checkData = new dataProductLocation();
            //checkData.title = data.title;
            //checkData.Id = data.id;
            //checkData.history = loadDataHistory(data);
            //checkData.dataLocations = loadDataLocation(data.id);
            return checkData;
        }

        private List<dataLocation> loadDataLocation(int id)
        {
            var list = new List<dataLocation>();

            var productLocation = _context.product_location.Where(x => x.product_id == id).ToList();
            foreach(var item in productLocation)
            {
                var checkDataLcoation = _context.location_addr.FirstOrDefault(x => x.id == item.location_addr_id);
                list.Add(new dataLocation
                {
                    area = checkDataLcoation.area,
                    line = checkDataLcoation.line,
                    shelf = checkDataLcoation.shelf,
                    code = checkDataLcoation.code_location_addr
                });
            }

            return list;
        }
        private List<object> loadDataHistory(product data)
        {
            var list = new List<object>();
            bool isCheck = true;

            var checkDataproduct = _context.product_location.Select(x => new
            {
                id = x.id,
                product_id = x.product_id,
                id_location = x.location_addr_id
            }).FirstOrDefault(x => x.product_id == data.id);

            if(checkDataproduct != null)
            {
                var checkLocation = _context.location_addr.FirstOrDefault(x => x.id == checkDataproduct.id_location);
                var checkPlan = _context.plan.Select(x => new
                {
                    id = x.id,
                    location_old = x.location_addr_id_old,
                    location_new = x.location_addr_id_new,
                    status = x.status,
                }).OrderByDescending(x => x.id).FirstOrDefault(x => x.location_new == checkLocation.id && x.status == 1);
                if(checkPlan != null)
                {
                    int? isCheckLocationOld = 0;
                    int checkWhile = 0;
                    var listCheckInt = new List<int>();
                    while (isCheck)
                    {
                        if(checkWhile == 0)
                            isCheckLocationOld = checkPlan.location_old;
                        var checkLocationOld = _context.plan.Include(x => x.location_Addr_Old).Select(x => new {
                                id = x.id,
                                location_old = x.location_addr_id_old,
                                location_new  = x.location_addr_id_new,
                                locationOld = x.location_Addr_Old,
                                status = x.status,
                        }).FirstOrDefault(x => x.location_new == isCheckLocationOld && x.status == 1 && !listCheckInt.Contains(x.id));

                        if(checkLocationOld != null)
                        {
                            list.Add(checkLocationOld.locationOld);
                            isCheckLocationOld = checkLocationOld.location_old;
                            listCheckInt.Add(checkLocationOld.id);
                        }
                        else
                        {
                            isCheck = false;
                        }

                        checkWhile++;
                    }
                    list.Add(checkLocation);
                }
            }
            return list;
        }
    }
}
