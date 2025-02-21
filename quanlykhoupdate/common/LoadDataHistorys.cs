using Microsoft.EntityFrameworkCore;
using quanlykhoupdate.Models;

namespace quanlykhoupdate.common
{
    public static class LoadDataHistorys
    {
        public static async Task<List<object>> loadDataHistoryAsync(product data, DBContext _context)
        {
            var list = new List<object>();
            bool isCheck = true;

            // Sử dụng _context đã được inject vào thay vì tạo mới và sử dụng bất đồng bộ
            var checkDataproduct = await _context.product_location
                .AsNoTracking()
                .Where(x => x.product_id == data.id)
                .Select(x => new
                {
                    id = x.id,
                    product_id = x.product_id,
                    id_location = x.location_addr_id
                })
                .FirstOrDefaultAsync();

            if (checkDataproduct != null)
            {
                var checkLocation = await _context.location_addr
                    .FirstOrDefaultAsync(x => x.id == checkDataproduct.id_location);

                var checkPlan = await _context.plan
                    .Select(x => new
                    {
                        id = x.id,
                        location_old = x.location_addr_id_old,
                        location_new = x.location_addr_id_new,
                        status = x.status,
                    })
                    .OrderByDescending(x => x.id)
                    .FirstOrDefaultAsync(x => x.location_new == checkLocation.id && x.status == 1);

                if (checkPlan != null)
                {
                    int? isCheckLocationOld = 0;
                    int checkWhile = 0;
                    var listCheckInt = new List<int>();

                    while (isCheck)
                    {
                        if (checkWhile == 0)
                            isCheckLocationOld = checkPlan.location_old;

                        var checkLocationOld = await _context.plan
                            .Include(x => x.location_Addr_Old)
                            .Select(x => new
                            {
                                id = x.id,
                                location_old = x.location_addr_id_old,
                                location_new = x.location_addr_id_new,
                                locationOld = x.location_Addr_Old,
                                status = x.status,
                            })
                            .OrderByDescending(x => x.id)
                            .FirstOrDefaultAsync(x => x.location_new == isCheckLocationOld && x.status == 1 && !listCheckInt.Contains(x.id));

                        if (checkLocationOld != null)
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
