using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Printing;

namespace quanlykhoupdate.Service
{
    public class location_addrService : Ilocation_addrService
    {
        private readonly DBContext _context;
        private List<productbyShelf> productbyShelves;
        public location_addrService(DBContext context)
        {
            _context = context;
        }
        public async Task<PayLoad<object>> SearchData(string? name, int page = 1, int pageSize = 10)
        {
            try
            {
                var data = _context.location_addr
    .Include(pl => pl.product_Locations)
    .ThenInclude(p => p.products)
    .ThenInclude(s => s.suppliers)
    .SelectMany(x => x.product_Locations)
    .Where(x => x.location_Addrs.code_location_addr.Contains(name))
    .Select(x => new Location_addrDTO
    {
        id = x.product_id,
        code = x.location_Addrs.code_location_addr,
        title = x.products.title,
        area = x.location_Addrs.area,
        line = x.location_Addrs.line,
        shelf = x.location_Addrs.shelf,
        quantity = x.quantity,
        supplier = x.products.suppliers.title,
        supplierName = x.products.suppliers.name,

    })
    .ToList();

                //if (!string.IsNullOrEmpty(name))
                //    data = data.Where(x => x.code.Contains(name)).ToList();

                var pageList = new PageList<object>(data, page - 1, pageSize);
                if(pageList.Count <= 0)
                    pageList = new PageList<object>(data, 0, pageSize);

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
                var checkProduct_location = _context.product_location.Include(p => p.products).ThenInclude(s => s.suppliers).FirstOrDefault(x => x.location_addr_id == item.id);
                if(checkProduct_location != null)
                {
                    list.Add(new Location_addrDTO
                    {
                        id = checkProduct_location.products == null ? 0 : checkProduct_location.products.id,
                        code = item.code_location_addr,
                        title = checkProduct_location.products == null ? Status.DATANULL : checkProduct_location.products.title,
                        area = item.area,
                        line = item.line,
                        shelf = item.shelf,
                        quantity = checkProduct_location.quantity,
                        supplier = checkProduct_location.products == null || checkProduct_location.products.suppliers == null ? Status.DATANULL : checkProduct_location.products.suppliers.title,
                        supplierName = checkProduct_location.products == null || checkProduct_location.products.suppliers == null ? Status.DATANULL : checkProduct_location.products.suppliers.name,
                        history = loadDataHistory(checkProduct_location.products == null ? new product() : checkProduct_location.products),
                        InOutByProducts = loadDataInOut(checkProduct_location.products == null ? 0 : checkProduct_location.products.id)
                    });
                }
            }

            return list;
        }

        private List<InOutByProduct> loadDataInOut(int id)
        {
            var list = new List<InOutByProduct>();

            if(id != 0)
            {
                var checkUpdateHistory = _context.update_history.Include(p => p.products).Include(l => l.location_Addrs).Where(x => x.product_id == id).ToList();

                foreach (var product in checkUpdateHistory)
                {
                    list.Add(new InOutByProduct
                    {
                        location = product != null && product.location_Addrs != null ? product.location_Addrs.code_location_addr : Status.DATANULL,
                        updateat = product.last_modify_date,
                        quantity = product.quantity,
                        status = product.status
                    });
                }
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

            if (checkDataproduct != null)
            {
                var checkLocation = _context.location_addr.FirstOrDefault(x => x.id == checkDataproduct.id_location);
                var checkPlan = _context.plan.Select(x => new
                {
                    id = x.id,
                    location_old = x.location_addr_id_old,
                    location_new = x.location_addr_id_new,
                    status = x.status,
                }).OrderByDescending(x => x.id).FirstOrDefault(x => x.location_new == checkLocation.id && x.status == 1);
                if (checkPlan != null)
                {
                    int? isCheckLocationOld = 0;
                    int checkWhile = 0;
                    var listCheckInt = new List<int>();
                    while (isCheck)
                    {
                        if (checkWhile == 0)
                            isCheckLocationOld = checkPlan.location_old;
                        var checkLocationOld = _context.plan.Include(x => x.location_Addr_Old).Select(x => new {
                            id = x.id,
                            location_old = x.location_addr_id_old,
                            location_new = x.location_addr_id_new,
                            locationOld = x.location_Addr_Old,
                            status = x.status,
                        }).FirstOrDefault(x => x.location_new == isCheckLocationOld && x.status == 1 && !listCheckInt.Contains(x.id));

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

        public byte[] FindAllDataByDateExcel(string code)
        {
            var data = _context.location_addr.Where(x => x.code_location_addr.Contains(code)).ToList();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");
                worksheet.Cells[1, 1].Value = "產品";
                worksheet.Cells[1, 2].Value = "區域";
                worksheet.Cells[1, 3].Value = "線";
                worksheet.Cells[1, 4].Value = "架子";
                worksheet.Cells[1, 5].Value = "地點";
                worksheet.Cells[1, 6].Value = "供應商";
                worksheet.Cells[1, 7].Value = "數量";
                //worksheet.Cells[1, 8].Value = "Status";
                //worksheet.Cells[1, 9].Value = "Quantity";
                //worksheet.Cells[1, 10].Value = "Location";
                //worksheet.Cells[1, 11].Value = "Date";

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true; // Chữ in đậm
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Nền đặc
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Nền xám nhạt
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Căn giữa nội dung
                }

                // Đổ dữ liệu vào file Excel
                int row = 2;
                foreach (var product in loadData(data))
                {
                    worksheet.Cells[row, 1].Value = product.title;
                    worksheet.Cells[row, 2].Value = product.area;
                    worksheet.Cells[row, 3].Value = product.line;
                    worksheet.Cells[row, 4].Value = product.shelf;
                    worksheet.Cells[row, 5].Value = product.code;
                    worksheet.Cells[row, 6].Value = product.supplier;
                    worksheet.Cells[row, 7].Value = product.quantity;

                    //if (product.InOutByProducts != null && product.InOutByProducts.Any() && product.InOutByProducts.Count > 0)
                    //{
                    //    foreach (var product2 in product.InOutByProducts)
                    //    {
                    //        worksheet.Cells[row, 8].Value = product2.status == 1 ? "Import" : "Deliverynote";
                    //        worksheet.Cells[row, 9].Value = product2.quantity;
                    //        worksheet.Cells[row, 10].Value = product2.location;
                    //        worksheet.Cells[row, 11].Value = product2.updateat;

                    //        row++;

                    //    }
                    //}
                    //else
                    //{
                    //    worksheet.Cells[row, 7].Value = product.supplier;
                    //    row++;
                    //}

                    row++;
                }

                worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
                return package.GetAsByteArray();
            }
        }

        public async Task<PayLoad<object>> FindAllData()
        {
            try
            {
                var dataArea = _context.location_addr.GroupBy(x => x.area).Select(x => x.Key).ToList();

                return await Task.FromResult(PayLoad<object>.Successfully(dataArea));
            }catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAllDataLine(string area)
        {
            try
            {
                var dataArea = _context.location_addr.Where(x => x.area == area).GroupBy(x => x.line).Select(x => x.Key).ToList();

                return await Task.FromResult(PayLoad<object>.Successfully(dataArea));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAllDataShelf(string line, string area)
        {
            try
            {
                var data = await _context.location_addr
            .Where(x => x.line == line && x.area == area)
            .AsNoTracking() // Tối ưu hiệu suất, giúp Entity Framework không theo dõi các đối tượng này. Do chỉ đọc dữ liệu, không cần theo dõi thay đổi -> tiết kiệm bộ nhớ và tăng tốc độ.
            .Select(x => new
            {
                shelf = x.shelf,
                area = x.area,
                line = x.line,
                code = x.code_location_addr,
                id = x.id
            })
            .GroupBy(x => new { x.shelf, x.area, x.line })
            .Select(g => new dataShelByLine /* Tạo một danh sách dataShelByLine chứa thông tin kệ.
                                                Mỗi kệ (shelf) chứa:
                                                    "location": danh sách các mã vị trí (code_location_addr).
                                                    "productIds": danh sách ID (id) để dùng trong truy vấn sản phẩm sau này.
                                             */
            {
                shelf = g.Key.shelf,
                location = g.Select(x => x.code).ToList(),
                productIds = g.Select(x => x.id).ToList() // Tạo một danh sách ID riêng để lấy productbyShelf sau
            })
            .ToListAsync();

                // Gọi 1 lần duy nhất để lấy toàn bộ sản phẩm
                var allIds = data.SelectMany(x => x.productIds).ToList(); /* Lấy danh sách tất cả productIds từ data 
                                                                           "data.SelectMany(x => x.productIds)" sẽ trải phẳng (flatten) danh sách productIds từ nhiều kệ (shelf) thành một danh sách duy nhất.
                                                                           */
                var productData = await dataLoadByShelfs(allIds); /* Gọi hàm dataLoadByShelfs(allIds) để truy vấn thông tin sản phẩm theo danh sách ID. 
                                                                    Chỉ chạy một truy vấn duy nhất để lấy tất cả dữ liệu sản phẩm, thay vì truy vấn riêng lẻ từng kệ (shelf).
                                                                   */

                // Gán danh sách sản phẩm từ productData
                foreach (var item in data) /* Duyệt qua từng shelf trong data 
                                            */
                {
                    // Với mỗi shelf, lọc danh sách sản phẩm (productData) có Id nằm trong productIds.
                    // Gán danh sách sản phẩm đó vào thuộc tính productbyShelf của shelf.
                    item.productbyShelf = productData.Where(p => item.productIds.Contains(p.Id)).ToList();
                }

                // Xóa danh sách ID sau khi đã dùng
                data.ForEach(x => x.productIds = null); // Sau khi đã lấy được dữ liệu sản phẩm đầy đủ, không cần giữ lại danh sách productIds nữa.
                                                        // Đặt "productIds = null" để giải phóng bộ nhớ, tránh lưu trữ dữ liệu không cần thiết.

                return await Task.FromResult(PayLoad<object>.Successfully(data));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        private async Task<List<productbyShelf>> dataLoadByShelfs(List<int> ids)
        {
            if (!ids.Any()) return new List<productbyShelf>();

            var productData = await _context.product_location
                .Where(x => ids.Contains(x.location_addr_id.Value))
                .Include(l => l.location_Addrs)
                .Include(p => p.products)
                .ThenInclude(s => s.suppliers)
                .AsNoTracking()
                .Select(x => new productbyShelf
                {
                    location = x.location_Addrs.code_location_addr,
                    title = x.products.title,
                    quantity = x.quantity,
                    supplier = x.products.suppliers.name,
                    Id = x.location_addr_id.Value, // Thêm ID để dễ lọc
                    IdProduct = x.products.id // Thêm ID để dễ lọc
                })
                .ToListAsync();

            return productData;
        }
        private List<dataShelByLine> loadDataLocationShelf(List<dataMapShelf> data)
        {
            var list = new List<dataShelByLine>();

            foreach (var item in data)
            {
                var checkLocation = _context.location_addr.Where(x => x.shelf == item.shelf && x.area == item.area && x.line == item.line).Select(x => new
                {
                    code = x.code_location_addr,
                    id = x.id
                }).ToList();

                list.Add(new dataShelByLine
                {
                    shelf = item.shelf,
                    location = checkLocation.Select(x => x.code).ToList(),
                    productbyShelf = dataLoadByShelf(checkLocation.Select(x => x.id).ToList())
                });
            }

            return list;
        }

        private List<productbyShelf> loadData(IEnumerable<dynamic> data)
        {
            

            foreach (var item in data)
            {
                productbyShelves.Add(new productbyShelf
                {
                    location = item.location.code_location_addr,
                    title = item.product.title,
                    quantity = item.quantity,
                    supplier = item.supplier
                });
            }

            return productbyShelves;
        }

        public async Task<PayLoad<object>> FindAllDataLocation(string line, string area, string shelf)
        {
            try
            {
                // Truy vấn danh sách vị trí theo điều kiện
                var checkLocation = await _context.location_addr
                    .Where(x => x.area == area && x.line == line && x.shelf == shelf)
                    .Select(x => new
                    {
                        x.id,
                        x.code_location_addr
                    }).ToListAsync();

                if (!checkLocation.Any())
                {
                    return PayLoad<object>.Successfully(new dataShelByLine
                    {
                        location = new List<string>(),
                        productbyShelf = new List<productbyShelf>(),
                        FindAllPlanDatas = new List<FindAllPlanData>()
                    });
                }

                // Danh sách ID vị trí
                var locationIds = checkLocation.Select(x => x.id).ToList();

                // Lấy dữ liệu sản phẩm theo danh sách ID vị trí
                var productbyShelfList = await dataLoadByShelfLocation(locationIds);

                // Lấy danh sách kế hoạch theo danh sách ID vị trí
                var planDataList = await dataPlanLocation(locationIds);

                return await Task.FromResult(PayLoad<object>.Successfully(new dataShelByLine
                {
                    location = checkLocation.Select(x => x.code_location_addr).ToList(),
                    productbyShelf = productbyShelfList,
                    FindAllPlanDatas = planDataList
                }));

            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        private List<FindAllPlanData> dataPlan(List<int> id)
        {
            var list = new List<FindAllPlanData>();

            foreach (var idItem in id)
            {
                var checkPlan = _context.plan.Where(x => (x.location_addr_id_old == idItem || x.location_addr_id_new == idItem) && x.status != 1).ToList();
                if(checkPlan != null && checkPlan.Count > 0)
                {
                    foreach (var itemPlan in checkPlan)
                    {
                        var checkLocationOld = _context.location_addr.FirstOrDefault(x => x.id == itemPlan.location_addr_id_old);
                        var checkLocationNew = _context.location_addr.FirstOrDefault(x => x.id == itemPlan.location_addr_id_new);

                        list.Add(new FindAllPlanData
                        {
                            id = itemPlan.id,
                            locationNew = checkLocationNew == null ? Status.DATANULL : checkLocationNew.code_location_addr,
                            areaNew = checkLocationNew == null ? Status.DATANULL : checkLocationNew.area,
                            lineNew = checkLocationNew == null ? Status.DATANULL : checkLocationNew.line,
                            shelfNew = checkLocationNew == null ? Status.DATANULL : checkLocationNew.shelf,
                            status = itemPlan == null ? null : itemPlan.status,
                            areaOld = checkLocationOld == null ? Status.DATANULL : checkLocationOld.area,
                            lineOld = checkLocationOld == null ? Status.DATANULL : checkLocationOld.line,
                            shelfOld = checkLocationOld == null ? Status.DATANULL : checkLocationOld.shelf,
                            locationOld = checkLocationOld == null ? Status.DATANULL : checkLocationOld.code_location_addr,
                            updateat = itemPlan.time
                        });
                    }
                }

            }

            return list;
        }

        // Tối ưu lấy dữ liệu sản phẩm
        private async Task<List<productbyShelf>> dataLoadByShelfLocation(List<int> locationIds)
        {
            var productList = await _context.product_location
                .Where(x => locationIds.Contains(x.location_addr_id.Value))
                .Include(x => x.location_Addrs)
                .Include(x => x.products)
                .ThenInclude(p => p.suppliers)
                .Select(x => new productbyShelf
                {
                    location = x.location_Addrs.code_location_addr,
                    title = x.products.title,
                    quantity = x.quantity,
                    supplier = x.products.suppliers.name,
                    IdProduct = x.products.id
                })
                .AsNoTracking()
                .ToListAsync();

            return productList;
        }

        // Tối ưu lấy dữ liệu kế hoạch
        private async Task<List<FindAllPlanData>> dataPlanLocation(List<int> locationIds)
        {
            var plans = await _context.plan
                .Where(x => (locationIds.Contains(x.location_addr_id_old.Value) || locationIds.Contains(x.location_addr_id_new.Value)) && x.status != 1)
                .Select(x => new
                {
                    x.id,
                    x.status,
                    x.time,
                    LocationOld = _context.location_addr.Where(loc => loc.id == x.location_addr_id_old)
                        .Select(loc => new { loc.code_location_addr, loc.area, loc.line, loc.shelf })
                        .FirstOrDefault(),
                    LocationNew = _context.location_addr.Where(loc => loc.id == x.location_addr_id_new)
                        .Select(loc => new { loc.code_location_addr, loc.area, loc.line, loc.shelf })
                        .FirstOrDefault()
                })
                .AsNoTracking()
                .ToListAsync();

            return plans.Select(p => new FindAllPlanData
            {
                id = p.id,
                locationNew = p.LocationNew?.code_location_addr ?? Status.DATANULL,
                areaNew = p.LocationNew?.area ?? Status.DATANULL,
                lineNew = p.LocationNew?.line ?? Status.DATANULL,
                shelfNew = p.LocationNew?.shelf ?? Status.DATANULL,
                status = p.status,
                areaOld = p.LocationOld?.area ?? Status.DATANULL,
                lineOld = p.LocationOld?.line ?? Status.DATANULL,
                shelfOld = p.LocationOld?.shelf ?? Status.DATANULL,
                locationOld = p.LocationOld?.code_location_addr ?? Status.DATANULL,
                updateat = p.time
            }).ToList();
        }
        private List<productbyShelf> dataLoadByShelf(List<int> id)
        {
            productbyShelves = new List<productbyShelf>();

            foreach (var item in id)
            {
                var checIdLocaation = _context.product_location.Include(l => l.location_Addrs).Include(p => p.products).ThenInclude(s => s.suppliers).Where(x => x.location_addr_id == item).Select(x => new
                {
                    product = x.products,
                    location = x.location_Addrs,
                    quantity = x.quantity,
                    supplier = x.products.suppliers.name
                }).ToList();

                loadData(checIdLocaation);
                
            }

            return productbyShelves;
        }
        private List<listProductByLocationData> dataProductByShefl(List<int> id)
        {
            var list = new List<listProductByLocationData>();

            foreach (var item in id)
            {
                var checIdLocaation = _context.product_location.Include(p => p.products).Include(l => l.location_Addrs).Where(x => x.location_addr_id == item).Select(x => new
                {
                    product = x.products,
                    location = x.location_Addrs
                }).ToList();

                list.Add(new listProductByLocationData
                {
                    productbyShelf = loadData(checIdLocaation)
                });
            }

            return list;
        }

        public async Task<PayLoad<object>> FindAllDataShelfOne(string line, string area)
        {
            try
            {
                var dataArea = _context.location_addr.Where(x => x.line == line && x.area == area).GroupBy(x => new
                {
                    shelf = x.shelf
                }).Select(x => new
                {
                    shelf = x.Key.shelf
                }).ToList();

                return await Task.FromResult(PayLoad<object>.Successfully(dataArea));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> FindAllDataDashBoad(int page = 1, int pageSize = 20)
        {
            try
            {
                //var list = new List<AreaDashboard>();
                var dataArea = new List<string>() {
                    "C1",
                    "C2",
                    "C3",
                    "C4",
                    "C5",
                    "C6",
                    "C7",
                    "C8",
                    "C9",
                    "Q1",
                    "Q2",
                    "Q3",
                    "Q4",
                    "Q5",
                    "Q6"
                };
                // Truy vấn toàn bộ dữ liệu một lần, lấy DISTINCT để tránh trùng lặp
                var locationData = _context.location_addr
                    .Where(x => dataArea.Contains(x.area))
                    .GroupBy(x => new { x.area, x.line, x.shelf }) // Nhóm theo 3 giá trị chính
                    .Select(g => new
                    {
                        id = g.Min(x => x.id), // (g.Min(x => x.id): Chọn id nhỏ nhất trong mỗi nhóm để đảm bảo chỉ có một id duy nhất cho mỗi nhóm.
                        g.Key.area,
                        g.Key.line,
                        g.Key.shelf
                    })
                    .ToList();

                // Lấy danh sách location_addr_id có trong product_location
                var productLocationIds = _context.product_location
                    .Select(x => x.location_addr_id)
                    .ToHashSet(); /* "ToHashSet()" là một phương thức chuyển đổi dữ liệu từ một IEnumerable (mảng hoặc danh sách) thành một HashSet. 
                                    "HashSet" là một cấu trúc dữ liệu trong C# dùng để lưu trữ các phần tử mà không cho phép
                                    giá trị trùng lặp và cho phép tìm kiếm nhanh hơn (với độ phức tạp gần như O(1) trong việc kiểm tra sự tồn tại của một phần tử).
                                    
                                    Lý do sử dụng HashSet:
                                    Loại bỏ trùng lặp: Nếu trong bảng product_location có các giá trị trùng lặp của location_addr_id, HashSet sẽ tự động loại bỏ những giá trị đó.
                                    Tìm kiếm nhanh: Nếu bạn muốn kiểm tra nhanh một location_addr_id có tồn tại trong productLocationIds hay không, việc sử dụng HashSet giúp giảm thời gian tra cứu, vì tìm kiếm trong HashSet nhanh hơn so với trong một danh sách bình thường (List).
                                   */

                // Nhóm dữ liệu theo khu vực -> Line -> Shelf (tránh trùng lặp)
                var areaDictionary = locationData
                    .GroupBy(x => x.area)
                    .ToDictionary( /* ToDictionary(areaGroup => areaGroup.Key, ...)
                                    * Giải thích: Phương thức ToDictionary chuyển các nhóm đã được nhóm lại thành một từ điển (Dictionary).
                                    * areaGroup.Key là area, tức là tên khu vực, sẽ là chìa khóa của từ điển.
                                    * Các nhóm sau khi nhóm lại sẽ trở thành giá trị của từ điển, chứa các phần tử có cùng khu vực.
                                    */
                        areaGroup => areaGroup.Key,
                        areaGroup => areaGroup
                            .GroupBy(x => x.line) /* "GroupBy(x => x.line)"  Giải thích: Sau khi nhóm theo area, 
                                                   * tiếp tục nhóm các phần tử trong mỗi nhóm theo line, Ví dụ: Nếu trong khu vực "C1" có các dòng "01", "02", "03", thì các phần tử sẽ được nhóm lại theo từng dòng.
                                                   
                                                   */
                            
                            .ToDictionary( /* "ToDictionary(lineGroup => lineGroup.Key, ...)" Giải thích: Tương tự như với area, phương thức ToDictionary chuyển các nhóm dòng thành từ điển, với chìa khóa là line (dòng). Mỗi nhóm dòng sẽ có các giá trị là các kệ (shelf).*/
                                lineGroup => lineGroup.Key,
                                lineGroup => lineGroup
                                    .Select(shelf => new ShelfLineArea /* ".Select(shelf => new ShelfLineArea { ... })" Giải thích: Sau khi nhóm theo dòng, tiếp tục chọn và tạo một đối tượng "ShelfLineArea" cho mỗi phần tử. 
                                                                            Mỗi ShelfLineArea chứa: 
                                                                                    "shelf": là giá trị của kệ (shelf.shelf)
                                                                                    "isProduct": là giá trị boolean cho biết liệu kệ này có tồn tại 
                                                                                                  trong bảng product_location hay không. Để kiểm tra 
                                                                                                  điều này, ta sử dụng "productLocationIds.Contains(shelf.id)", 
                                                                                                  nơi productLocationIds là một HashSet chứa các "location_addr_id" có trong "product_location".
                                                                        */
                                    {
                                        shelf = shelf.shelf,
                                        isProduct = productLocationIds.Contains(shelf.id)
                                    })
                                    .Distinct() // ".Distinct()": Giải thích: Phương thức Distinct() loại bỏ các phần tử trùng lặp trong danh sách.
                                    .ToList()
                            )
                    );

                // Tạo danh sách line mặc định từ 1 - 10
                /*
                 * "Enumerable.Range(1, 10)"
                 * Giải thích: Phương thức Enumerable.Range tạo ra một chuỗi các số nguyên trong một khoảng cho trước. Cú pháp là Enumerable.Range(start, count), trong đó:
                    "start": Là giá trị bắt đầu của dãy số.
                    "count": Là số lượng các phần tử cần tạo ra.
                    Ở đây, Enumerable.Range(1, 10) sẽ tạo ra dãy số từ 1 đến 10 (tổng cộng 10 số).

                    Kết quả: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

                ".Select(x => x.ToString("D2"))": 
                Giải thích: Phương thức Select là một phương thức trong LINQ dùng để chuyển đổi mỗi phần tử trong dãy số thành một giá trị mới. Trong trường hợp này, x.ToString("D2") chuyển mỗi số nguyên thành một chuỗi với định dạng số có ít nhất 2 chữ số, bổ sung số 0 ở đầu nếu cần.
                "D2" là một kiểu định dạng số nguyên (decimal) với 2 chữ số, nếu số nhỏ hơn 10 thì nó sẽ được điền thêm số 0 ở phía trước.
                Ví dụ: 1 sẽ trở thành "01", 10 sẽ giữ nguyên là "10".
                Kết quả sau khi áp dụng Select:

                [ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" ]
                 */
                var defaultLines = Enumerable.Range(1, 10).Select(x => x.ToString("D2")).ToList();
                // Tạo danh sách shelf mặc định từ 01 - 20
                var defaultShelves = Enumerable.Range(1, 20).Select(x => x.ToString("D2")).ToList();

                // Chuyển dữ liệu về dạng cần thiết
                var list = dataArea.Select(area => new AreaDashboard /* dataArea.Select(area => new AreaDashboard {...}).ToList();
                                                                        "dataArea" là một danh sách các khu vực. Mỗi khu vực trong dataArea sẽ được chuyển thành một đối tượng "AreaDashboard".
                                                                        Mỗi đối tượng AreaDashboard chứa hai thuộc tính:
                                                                        "Area": Lưu tên khu vực (ví dụ: "C6", "C1", ...).
                                                                        "Line": Lưu danh sách các đối tượng LineArea đại diện cho các dòng trong khu vực đó.
                                                                        Kết quả: Một danh sách các đối tượng AreaDashboard, mỗi đối tượng chứa thông tin về khu vực và các dòng của khu vực đó.
                                                                      */
                {
                    Area = area,
                    Line = defaultLines.Select(line => new LineArea /* "defaultLines.Select(line => new LineArea {...}).ToList();" 
                                                                     Giải thích:
                                                                        "defaultLines" là danh sách các dòng mặc định (tạo từ dãy số từ "01" đến "10").
                                                                        Select(line => new LineArea {...}): Với mỗi dòng trong defaultLines, một đối tượng LineArea mới được tạo ra.
                                                                        LineArea chứa:
                                                                        Line: Tên của dòng (ví dụ: "01", "02", ...).
                                                                        Shelf: Danh sách các kệ (shelves) của dòng này.
                                                                        Kết quả: Mỗi khu vực sẽ có một danh sách các dòng, và mỗi dòng này có một danh sách các kệ.
                                                                     */
                    {
                        Line = line,
                        Shelf = areaDictionary.ContainsKey(area) && areaDictionary[area].ContainsKey(line) /* "areaDictionary.ContainsKey(area) && areaDictionary[area].ContainsKey(line)"
                                                                                                                Giải thích:
                                                                                                                    "areaDictionary" là từ điển chứa dữ liệu các khu vực, dòng và kệ đã được xử lý trước đó (được nhóm theo khu vực và dòng).
                                                                                                                    Câu lệnh này kiểm tra xem khu vực area và dòng line có tồn tại trong areaDictionary hay không.
                                                                                                                    Nếu có, tức là có dữ liệu kệ (shelves) đã được lấy từ cơ sở dữ liệu, nếu không thì sử dụng kệ mặc định (defaultShelves).
                                                                                                            */
                            ? MergeShelves(areaDictionary[area][line], defaultShelves) /* "MergeShelves(areaDictionary[area][line], defaultShelves)"
                                                                                        Giải thích:
                                                                                                Nếu "areaDictionary" chứa kệ cho khu vực và dòng cụ thể, hàm MergeShelves sẽ được gọi.
                                                                                                Mục đích của hàm này là kết hợp các kệ đã tồn tại trong areaDictionary với các kệ mặc định trong defaultShelves, đảm bảo rằng mỗi kệ sẽ có thông tin về sản phẩm (isProduct).
                                                                                                Hàm này sẽ trả về một danh sách các đối tượng "ShelfLineArea", mỗi đối tượng chứa tên kệ (shelf) và trạng thái có sản phẩm (isProduct).
                                                                                        */
                            : defaultShelves.Select(shelf => new ShelfLineArea { shelf = shelf, isProduct = false }).ToList() /* "defaultShelves.Select(shelf => new ShelfLineArea { shelf = shelf, isProduct = false }).ToList()"
                                                                                                                               Giải thích:
                                                                                                                                    Nếu không có kệ dữ liệu cho khu vực và dòng trong areaDictionary, tức là không có kệ nào được lưu trong cơ sở dữ liệu, thì hệ thống sẽ sử dụng các kệ mặc định từ "defaultShelves".
                                                                                                                                    Mỗi kệ sẽ được đánh dấu là không có sản phẩm (isProduct = false).
                                                                                                                               */
                    }).ToList()
                }).ToList();

                var pageList = new PageList<object>(list, page - 1, pageSize);
                if(pageList.Count <= 0)
                    pageList = new PageList<object>(list, 0, pageSize);
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

        // Hàm hợp nhất danh sách shelf từ database với danh sách shelf mặc định
        private List<ShelfLineArea> MergeShelves(List<ShelfLineArea> existingShelves, List<string> defaultShelves)
        {
            // "existingShelves": Danh sách các kệ đã tồn tại (được lấy từ areaDictionary)
            // "defaultShelves": Danh sách các kệ mặc định (tạo từ defaultLines)
            /*
             Hàm này thực hiện các bước sau:
                Tạo một từ điển existingShelfDict từ existingShelves, trong đó key là tên kệ (shelf), và value là giá trị của isProduct (kiểm tra xem kệ có sản phẩm hay không).
                Sử dụng Select để tạo danh sách mới từ defaultShelves, trong đó:
                "shelf": Tên của kệ.
                "isProduct": Nếu kệ này có trong existingShelfDict, giá trị của isProduct sẽ được lấy từ existingShelfDict; nếu không, giá trị sẽ là false.
                Kết quả: Một danh sách các đối tượng ShelfLineArea, mỗi đối tượng chứa tên kệ và trạng thái có sản phẩm (isProduct).
             */
            var existingShelfDict = existingShelves.ToDictionary(x => x.shelf, x => x.isProduct);
            return defaultShelves.Select(shelf => new ShelfLineArea
            {
                shelf = shelf,
                isProduct = existingShelfDict.ContainsKey(shelf) ? existingShelfDict[shelf] : false
            }).ToList();
        }

        private AreaDashboard findOneDataDashBoard(string area)
        {
            return new AreaDashboard
            {
                Area = area,
                Line = listLineData(area)
            };
        }

        private List<LineArea> listLineData(string area)
        {
            var list = new List<LineArea>();
            var dataLineByArea = _context.location_addr.Where(x => x.area == area).GroupBy(x => x.line).Select(x => x.Key).ToList();
            foreach(var line in dataLineByArea)
            {
                list.Add(new LineArea
                {
                    Line = line,
                    Shelf = dataShelfLineAreaData(area, line)
                });
            }
            return list;
        }

        private List<ShelfLineArea> dataShelfLineAreaData(string area, string line)
        {
            var list = new List<ShelfLineArea>();

            var data = _context.location_addr.Where(x => x.area == area && x.line == line).GroupBy(x => x.shelf).Select(x => x.Key)
            .ToList();

            foreach(var item in data)
            {
                //var checkDataLocationProduct = _context.product_location.FirstOrDefault(x => x.location_addr_id == item.id);

                list.Add(new ShelfLineArea
                {
                    shelf = item
                });
            }
            return list;
        }

        public async Task<PayLoad<object>> FindAllDataDashBoadSearch(FindAllDataDashBoard data)
        {
            try
            {
                
                // Truy vấn toàn bộ dữ liệu một lần, lấy DISTINCT để tránh trùng lặp
                var locationData = _context.location_addr
                    .Where(x => data.data.Contains(x.area))
                    .GroupBy(x => new { x.area, x.line, x.shelf }) // Nhóm theo 3 giá trị chính
                    .Select(g => new
                    {
                        id = g.Min(x => x.id), // (g.Min(x => x.id): Chọn id nhỏ nhất trong mỗi nhóm để đảm bảo chỉ có một id duy nhất cho mỗi nhóm.
                        g.Key.area,
                        g.Key.line,
                        g.Key.shelf
                    })
                    .ToList();

                // Lấy danh sách location_addr_id có trong product_location
                var productLocationIds = _context.product_location
                    .Select(x => x.location_addr_id)
                    .ToHashSet(); /* "ToHashSet()" là một phương thức chuyển đổi dữ liệu từ một IEnumerable (mảng hoặc danh sách) thành một HashSet. 
                                    "HashSet" là một cấu trúc dữ liệu trong C# dùng để lưu trữ các phần tử mà không cho phép
                                    giá trị trùng lặp và cho phép tìm kiếm nhanh hơn (với độ phức tạp gần như O(1) trong việc kiểm tra sự tồn tại của một phần tử).
                                    
                                    Lý do sử dụng HashSet:
                                    Loại bỏ trùng lặp: Nếu trong bảng product_location có các giá trị trùng lặp của location_addr_id, HashSet sẽ tự động loại bỏ những giá trị đó.
                                    Tìm kiếm nhanh: Nếu bạn muốn kiểm tra nhanh một location_addr_id có tồn tại trong productLocationIds hay không, việc sử dụng HashSet giúp giảm thời gian tra cứu, vì tìm kiếm trong HashSet nhanh hơn so với trong một danh sách bình thường (List).
                                   */

                // Nhóm dữ liệu theo khu vực -> Line -> Shelf (tránh trùng lặp)
                var areaDictionary = locationData
                    .GroupBy(x => x.area)
                    .ToDictionary( /* ToDictionary(areaGroup => areaGroup.Key, ...)
                                    * Giải thích: Phương thức ToDictionary chuyển các nhóm đã được nhóm lại thành một từ điển (Dictionary).
                                    * areaGroup.Key là area, tức là tên khu vực, sẽ là chìa khóa của từ điển.
                                    * Các nhóm sau khi nhóm lại sẽ trở thành giá trị của từ điển, chứa các phần tử có cùng khu vực.
                                    */
                        areaGroup => areaGroup.Key,
                        areaGroup => areaGroup
                            .GroupBy(x => x.line) /* "GroupBy(x => x.line)"  Giải thích: Sau khi nhóm theo area, 
                                                   * tiếp tục nhóm các phần tử trong mỗi nhóm theo line, Ví dụ: Nếu trong khu vực "C1" có các dòng "01", "02", "03", thì các phần tử sẽ được nhóm lại theo từng dòng.
                                                   
                                                   */

                            .ToDictionary( /* "ToDictionary(lineGroup => lineGroup.Key, ...)" Giải thích: Tương tự như với area, phương thức ToDictionary chuyển các nhóm dòng thành từ điển, với chìa khóa là line (dòng). Mỗi nhóm dòng sẽ có các giá trị là các kệ (shelf).*/
                                lineGroup => lineGroup.Key,
                                lineGroup => lineGroup
                                    .Select(shelf => new ShelfLineArea /* ".Select(shelf => new ShelfLineArea { ... })" Giải thích: Sau khi nhóm theo dòng, tiếp tục chọn và tạo một đối tượng "ShelfLineArea" cho mỗi phần tử. 
                                                                            Mỗi ShelfLineArea chứa: 
                                                                                    "shelf": là giá trị của kệ (shelf.shelf)
                                                                                    "isProduct": là giá trị boolean cho biết liệu kệ này có tồn tại 
                                                                                                  trong bảng product_location hay không. Để kiểm tra 
                                                                                                  điều này, ta sử dụng "productLocationIds.Contains(shelf.id)", 
                                                                                                  nơi productLocationIds là một HashSet chứa các "location_addr_id" có trong "product_location".
                                                                        */
                                    {
                                        shelf = shelf.shelf,
                                        isProduct = productLocationIds.Contains(shelf.id)
                                    })
                                    .Distinct() // ".Distinct()": Giải thích: Phương thức Distinct() loại bỏ các phần tử trùng lặp trong danh sách.
                                    .ToList()
                            )
                    );

                // Tạo danh sách line mặc định từ 1 - 10
                /*
                 * "Enumerable.Range(1, 10)"
                 * Giải thích: Phương thức Enumerable.Range tạo ra một chuỗi các số nguyên trong một khoảng cho trước. Cú pháp là Enumerable.Range(start, count), trong đó:
                    "start": Là giá trị bắt đầu của dãy số.
                    "count": Là số lượng các phần tử cần tạo ra.
                    Ở đây, Enumerable.Range(1, 10) sẽ tạo ra dãy số từ 1 đến 10 (tổng cộng 10 số).

                    Kết quả: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]

                ".Select(x => x.ToString("D2"))": 
                Giải thích: Phương thức Select là một phương thức trong LINQ dùng để chuyển đổi mỗi phần tử trong dãy số thành một giá trị mới. Trong trường hợp này, x.ToString("D2") chuyển mỗi số nguyên thành một chuỗi với định dạng số có ít nhất 2 chữ số, bổ sung số 0 ở đầu nếu cần.
                "D2" là một kiểu định dạng số nguyên (decimal) với 2 chữ số, nếu số nhỏ hơn 10 thì nó sẽ được điền thêm số 0 ở phía trước.
                Ví dụ: 1 sẽ trở thành "01", 10 sẽ giữ nguyên là "10".
                Kết quả sau khi áp dụng Select:

                [ "01", "02", "03", "04", "05", "06", "07", "08", "09", "10" ]
                 */
                var defaultLines = Enumerable.Range(1, 10).Select(x => x.ToString("D2")).ToList();
                // Tạo danh sách shelf mặc định từ 01 - 20
                var defaultShelves = Enumerable.Range(1, 20).Select(x => x.ToString("D2")).ToList();

                // Chuyển dữ liệu về dạng cần thiết
                var list = data.data.Select(area => new AreaDashboard /* dataArea.Select(area => new AreaDashboard {...}).ToList();
                                                                        "dataArea" là một danh sách các khu vực. Mỗi khu vực trong dataArea sẽ được chuyển thành một đối tượng "AreaDashboard".
                                                                        Mỗi đối tượng AreaDashboard chứa hai thuộc tính:
                                                                        "Area": Lưu tên khu vực (ví dụ: "C6", "C1", ...).
                                                                        "Line": Lưu danh sách các đối tượng LineArea đại diện cho các dòng trong khu vực đó.
                                                                        Kết quả: Một danh sách các đối tượng AreaDashboard, mỗi đối tượng chứa thông tin về khu vực và các dòng của khu vực đó.
                                                                      */
                {
                    Area = area,
                    Line = defaultLines.Select(line => new LineArea /* "defaultLines.Select(line => new LineArea {...}).ToList();" 
                                                                     Giải thích:
                                                                        "defaultLines" là danh sách các dòng mặc định (tạo từ dãy số từ "01" đến "10").
                                                                        Select(line => new LineArea {...}): Với mỗi dòng trong defaultLines, một đối tượng LineArea mới được tạo ra.
                                                                        LineArea chứa:
                                                                        Line: Tên của dòng (ví dụ: "01", "02", ...).
                                                                        Shelf: Danh sách các kệ (shelves) của dòng này.
                                                                        Kết quả: Mỗi khu vực sẽ có một danh sách các dòng, và mỗi dòng này có một danh sách các kệ.
                                                                     */
                    {
                        Line = line,
                        Shelf = areaDictionary.ContainsKey(area) && areaDictionary[area].ContainsKey(line) /* "areaDictionary.ContainsKey(area) && areaDictionary[area].ContainsKey(line)"
                                                                                                                Giải thích:
                                                                                                                    "areaDictionary" là từ điển chứa dữ liệu các khu vực, dòng và kệ đã được xử lý trước đó (được nhóm theo khu vực và dòng).
                                                                                                                    Câu lệnh này kiểm tra xem khu vực area và dòng line có tồn tại trong areaDictionary hay không.
                                                                                                                    Nếu có, tức là có dữ liệu kệ (shelves) đã được lấy từ cơ sở dữ liệu, nếu không thì sử dụng kệ mặc định (defaultShelves).
                                                                                                            */
                            ? MergeShelves(areaDictionary[area][line], defaultShelves) /* "MergeShelves(areaDictionary[area][line], defaultShelves)"
                                                                                        Giải thích:
                                                                                                Nếu "areaDictionary" chứa kệ cho khu vực và dòng cụ thể, hàm MergeShelves sẽ được gọi.
                                                                                                Mục đích của hàm này là kết hợp các kệ đã tồn tại trong areaDictionary với các kệ mặc định trong defaultShelves, đảm bảo rằng mỗi kệ sẽ có thông tin về sản phẩm (isProduct).
                                                                                                Hàm này sẽ trả về một danh sách các đối tượng "ShelfLineArea", mỗi đối tượng chứa tên kệ (shelf) và trạng thái có sản phẩm (isProduct).
                                                                                        */
                            : defaultShelves.Select(shelf => new ShelfLineArea { shelf = shelf, isProduct = false }).ToList() /* "defaultShelves.Select(shelf => new ShelfLineArea { shelf = shelf, isProduct = false }).ToList()"
                                                                                                                               Giải thích:
                                                                                                                                    Nếu không có kệ dữ liệu cho khu vực và dòng trong areaDictionary, tức là không có kệ nào được lưu trong cơ sở dữ liệu, thì hệ thống sẽ sử dụng các kệ mặc định từ "defaultShelves".
                                                                                                                                    Mỗi kệ sẽ được đánh dấu là không có sản phẩm (isProduct = false).
                                                                                                                               */
                    }).ToList()
                }).ToList();

                var pageList = new PageList<object>(list, data.page - 1, data.pageSize);
                if (pageList.Count <= 0)
                    pageList = new PageList<object>(list, 0, data.pageSize);
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = pageList,
                    data.page,
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
