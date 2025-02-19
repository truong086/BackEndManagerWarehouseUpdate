using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public class location_addrService : Ilocation_addrService
    {
        private readonly DBContext _context;
        public location_addrService(DBContext context)
        {
            _context = context;
        }
        public async Task<PayLoad<object>> SearchData(string? name, int page = 1, int pageSize = 10)
        {
            try
            {
                var data = _context.location_addr.ToList();

                if (!string.IsNullOrEmpty(name))
                    data = data.Where(x => x.code_location_addr.Contains(name)).ToList();

                var pageList = new PageList<object>(loadData(data), page - 1, pageSize);

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

        private List<Location_addrDTO> loadData(List<location_addr> data) 
        { 
        
            var list = new List<Location_addrDTO>();

            foreach (var item in data)
            {
                var checkProduct_location = _context.product_location.FirstOrDefault(x => x.location_addr_id == item.id);
                if(checkProduct_location != null)
                {
                    var checkproduct = _context.product.Select(x => new
                    {
                        id = x.id,
                        title = x.title
                    }).FirstOrDefault(x => x.id == checkProduct_location.product_id);

                    list.Add(new Location_addrDTO
                    {
                        code = item.code_location_addr,
                        title = checkproduct.title,
                        area = item.area,
                        line = item.line,
                        shelf = item.shelf
                    });
                }
            }

            return list;
        }
    }
}
