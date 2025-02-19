using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;
using OfficeOpenXml;
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
                var checkProduct_location = _context.product_location.Include(p => p.products).FirstOrDefault(x => x.location_addr_id == item.id);
                if(checkProduct_location != null)
                {
                    list.Add(new Location_addrDTO
                    {
                        code = item.code_location_addr,
                        title = checkProduct_location.products.title,
                        area = item.area,
                        line = item.line,
                        shelf = item.shelf,
                        history = loadDataHistory(checkProduct_location.products)
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
                worksheet.Cells[1, 1].Value = "Title";
                worksheet.Cells[1, 2].Value = "Area";
                worksheet.Cells[1, 3].Value = "Line";
                worksheet.Cells[1, 4].Value = "Shelf";
                worksheet.Cells[1, 5].Value = "Code";

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
                foreach (var product in loadData(data))
                {
                    worksheet.Cells[row, 1].Value = product.title;
                    worksheet.Cells[row, 2].Value = product.area;
                    worksheet.Cells[row, 3].Value = product.line;
                    worksheet.Cells[row, 4].Value = product.shelf;
                    worksheet.Cells[row, 5].Value = product.code;

                    row++;
                }

                worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
                return package.GetAsByteArray();
            }
        }
    }
}
