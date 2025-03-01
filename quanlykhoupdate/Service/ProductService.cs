using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Xml.Linq;

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

        public async Task<PayLoad<object>> findOne(string? name, int page = 1, int pageSize = 20)
        {
            try
            {
                var list = new List<dataProductLocation>();

                var checkData = _context.product.Where(x => x.title == name).ToList();
                if(checkData == null)
                    return await Task.FromResult(PayLoad<object>.CreatedFail(Status.DATANULL));

                foreach(var item in checkData)
                {
                    list.Add(loadDataFindOne(item));
                }

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
                 .Include(x => x.suppliers)
                .Include(pl => pl.product_Locations)
                .ThenInclude(l => l.location_Addrs)
                .AsNoTracking()
                .Select(x => new dataProductLocation
                {
                    Id = x.id,
                    title = x.title,
                    nameSupplier = x.suppliers.name,
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
                    code = checkDataLcoation.code_location_addr,
                    
                });
                
            }

            return list;
        }

        private List<dynamic> loadDataHistory(product data)
        {

            var checkData = _context.update_history.Include(l => l.location_Addrs).Where(x => x.product_id == data.id && x.status == 2)
                .Select(x => new
                {
                    location_old = x.location_Addrs,
                    time = x.last_modify_date
                })
                .ToList<dynamic>();

            return checkData;
            
        }
        //private List<object> loadDataHistory(product data)
        //{
        //    var list = new List<object>();
        //    bool isCheck = true;

        //    var checkDataproduct = _context.product_location.Select(x => new
        //    {
        //        id = x.id,
        //        product_id = x.product_id,
        //        id_location = x.location_addr_id
        //    }).FirstOrDefault(x => x.product_id == data.id);

        //    if(checkDataproduct != null)
        //    {
        //        var checkLocation = _context.location_addr.FirstOrDefault(x => x.id == checkDataproduct.id_location);
        //        var checkPlan = _context.plan.Select(x => new
        //        {
        //            id = x.id,
        //            location_old = x.location_addr_id_old,
        //            location_new = x.location_addr_id_new,
        //            status = x.status,
        //        }).OrderByDescending(x => x.id).FirstOrDefault(x => x.location_new == checkLocation.id && x.status == 1);

        //        if(checkPlan == null)
        //        {
        //            checkPlan = _context.plan.Select(x => new
        //            {
        //                id = x.id,
        //                location_old = x.location_addr_id_old,
        //                location_new = x.location_addr_id_new,
        //                status = x.status,
        //            }).OrderByDescending(x => x.id).FirstOrDefault(x => x.location_old == checkLocation.id && x.status == 1);

        //            int? isCheckLocationOld = 0;
        //            int checkWhile = 0;
        //            var listCheckInt = new List<int>();
        //            if(checkPlan != null)
        //            {
        //                while (isCheck)
        //                {
        //                    if (checkWhile == 0)
        //                        isCheckLocationOld = checkPlan.location_new;
        //                    var checkLocationOld = _context.plan.Include(x => x.location_Addr_Old).Select(x => new {
        //                        id = x.id,
        //                        location_old = x.location_addr_id_old,
        //                        location_new = x.location_addr_id_new,
        //                        locationOld = x.location_Addr_Old,
        //                        status = x.status,
        //                    }).OrderByDescending(x => x.id).FirstOrDefault(x => x.location_old == isCheckLocationOld && x.status == 1 && !listCheckInt.Contains(x.id));

        //                    if (checkLocationOld != null)
        //                    {
        //                        list.Add(checkLocationOld.location_new);
        //                        isCheckLocationOld = checkLocationOld.location_new;
        //                        listCheckInt.Add(checkLocationOld.id);
        //                    }
        //                    else
        //                    {
        //                        list.Add(checkLocation);
        //                        if (list.Count == 1)
        //                        {
        //                            var checkLocationOldData = _context.location_addr.FirstOrDefault(x => x.id == checkPlan.location_new);
        //                            if (checkLocationOldData != null)
        //                                list.Add(checkLocationOldData);
        //                        }

        //                        isCheck = false;
        //                    }

        //                    checkWhile++;
        //                }
        //            }

        //        }


        //        else if (checkPlan != null)
        //        {
        //            int? isCheckLocationOld = 0;
        //            int checkWhile = 0;
        //            var listCheckInt = new List<int>();
        //            while (isCheck)
        //            {
        //                if(checkWhile == 0)
        //                    isCheckLocationOld = checkPlan.location_old;
        //                var checkLocationOld = _context.plan.Include(x => x.location_Addr_Old).Select(x => new {
        //                        id = x.id,
        //                        location_old = x.location_addr_id_old,
        //                        location_new  = x.location_addr_id_new,
        //                        locationOld = x.location_Addr_Old,
        //                        status = x.status,
        //                }).OrderByDescending(x => x.id).FirstOrDefault(x => x.location_new == isCheckLocationOld && x.status == 1 && !listCheckInt.Contains(x.id));

        //                if(checkLocationOld != null)
        //                {
        //                    list.Add(checkLocationOld.locationOld);
        //                    isCheckLocationOld = checkLocationOld.location_old;
        //                    listCheckInt.Add(checkLocationOld.id);
        //                }
        //                else
        //                {
        //                    list.Add(checkLocation);
        //                    if (list.Count == 1)
        //                    {
        //                        var checkLocationOldData = _context.location_addr.FirstOrDefault(x => x.id == checkPlan.location_old);
        //                        if(checkLocationOldData != null)
        //                            list.Add(checkLocationOldData);
        //                    }   

        //                    isCheck = false;
        //                }

        //                checkWhile++;
        //            }

        //        }
        //    }
        //    return list;
        //}

        public async Task<PayLoad<string>> AddDataSupplier()
        {
            try
            {
                int idSupplier = 1;
                for(var i = 1; i <= 28381; i++)
                {
                    if(idSupplier > 8)
                        idSupplier = 1;

                    var checkData = _context.product.FirstOrDefault(x => x.id == i);
                    if(checkData != null)
                    {
                        var checkSupplier = _context.supplier.FirstOrDefault(x => x.id == idSupplier);
                        if(checkSupplier != null)
                        {
                            checkData.suppliers = checkSupplier;
                            checkData.warehouseID = checkSupplier.id;

                            _context.product.Update(checkData);
                            _context.SaveChanges();
                        }

                        idSupplier++;
                    }
                }
                return await Task.FromResult(PayLoad<string>.Successfully(Status.SUCCESS));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<string>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> ImportDataExcel(IFormFile file)
        {
            try
            {
                var columNames = new List<string>();
                var columName = "ptNo";
                var columNameWsNo = "warehouseID";
                var result = new List<Dictionary<string, object>>();


                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0]; // Lấy sheet đầu tiên
                        int rowCount = worksheet.Dimension.Rows; // Tổng số dòng
                        int colCount = worksheet.Dimension.Columns; // Tổng số cột

                        // Tìm vị trí cột cần lấy
                        int targetColumnIndex = -1;
                        int targetColumnIndexWsNo = -1;
                        for (int col = 1; col <= colCount; col++)
                        {
                            if (worksheet.Cells[1, col].Text.Trim().Equals(columName, StringComparison.OrdinalIgnoreCase))
                            {
                                targetColumnIndex = col;

                            }

                            if (worksheet.Cells[1, col].Text.Trim().Equals(columNameWsNo, StringComparison.OrdinalIgnoreCase))
                            {
                                targetColumnIndexWsNo = col;
                            }

                            if (targetColumnIndex != -1 && targetColumnIndexWsNo != -1)
                                break;
                        }

                        //if (targetColumnIndex == -1)
                        //    return BadRequest($"Không tìm thấy cột '{columName}' trong file Excel.");

                        // Lặp qua từng dòng để lấy dữ liệu trong cột cần tìm
                        for (int row = 10000; row <= rowCount; row++) // Bỏ qua dòng tiêu đề
                        {

                            if (targetColumnIndexWsNo != -1 || targetColumnIndex != -1)
                            {
                                string oldValueWsNo = worksheet.Cells[row, targetColumnIndexWsNo].Text;
                                string oldValue = worksheet.Cells[row, targetColumnIndex].Text;

                                //var updateData = oldValue.TrimStart('0');
                                //updateData = string.IsNullOrEmpty(updateData) ? "0" : updateData;

                                var checkProduct = _context.product.FirstOrDefault(x => x.title == oldValue && x.warehouseID == null && x.suppliers == null);
                                if(checkProduct != null)
                                {
                                    var checkDataSupplier = _context.supplier.FirstOrDefault(x => x.title == oldValueWsNo);

                                    if (checkDataSupplier != null && checkProduct != null)
                                    {
                                        checkProduct.warehouseID = checkDataSupplier.id;
                                        checkProduct.suppliers = checkDataSupplier;

                                        _context.product.Update(checkProduct);
                                        _context.SaveChanges();
                                    }
                                }
                                else if(checkProduct == null)
                                {
                                    var checkProductUpdate = _context.product.FirstOrDefault(x => x.title == "00" + oldValue && x.warehouseID == null && x.suppliers == null);

                                    if (checkProductUpdate != null)
                                    {
                                        var checkDataSupplier = _context.supplier.FirstOrDefault(x => x.title == oldValueWsNo);

                                        if (checkDataSupplier != null && checkProductUpdate != null)
                                        {
                                            checkProductUpdate.warehouseID = checkDataSupplier.id;
                                            checkProductUpdate.suppliers = checkDataSupplier;

                                            _context.product.Update(checkProductUpdate);
                                            _context.SaveChanges();
                                        }
                                    }

                                    else
                                    {
                                        var checkProductUpdateData = _context.product.FirstOrDefault(x => x.title == "0" + oldValue && x.warehouseID == null && x.suppliers == null);

                                        if (checkProductUpdateData != null)
                                        {
                                            var checkDataSupplier = _context.supplier.FirstOrDefault(x => x.title == oldValueWsNo);

                                            if (checkDataSupplier != null && checkProductUpdateData != null)
                                            {
                                                checkProductUpdateData.warehouseID = checkDataSupplier.id;
                                                checkProductUpdateData.suppliers = checkDataSupplier;

                                                _context.product.Update(checkProductUpdateData);
                                                _context.SaveChanges();
                                            }
                                        }
                                    }
                                }


                            }
                        }
                    }
                }
                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = Status.SUCCESS
                }));

            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<object>> findBySuppliers(int id, int page = 1, int pageSize = 20)
        {
            try
            {
                var checkDataSupplier = await _context.product.Where(x => x.warehouseID == id)
            .Include(x => x.suppliers)
            .Include(pl => pl.product_Locations)
            .ThenInclude(l => l.location_Addrs)
            .AsNoTracking()
            .Select(x => new dataProductLocation
            {
                Id = x.id,
                title = x.title,
                nameSupplier = x.suppliers.name,
                dataLocations = x.product_Locations.Select(lc => new dataLocation
                {
                    code = lc.location_Addrs.code_location_addr,
                    area = lc.location_Addrs.area,
                    line = lc.location_Addrs.line,
                    shelf = lc.location_Addrs.shelf,
                    quantity = lc.quantity

                }).ToList(),
            }).ToListAsync();

                var pageList = new PageList<object>(checkDataSupplier, page - 1, pageSize);
                if(pageList.Count <= 0)
                    pageList = new PageList<object>(checkDataSupplier, 0, pageSize);
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

        public async Task<PayLoad<object>> findOneByOutAndIn(int id)
        {
            try
            {
                var checkProduct = _context.product.Include(s => s.suppliers).FirstOrDefault(x => x.id == id);
                if (checkProduct == null)
                    return await Task.FromResult(PayLoad<object>.CreatedFail(Status.DATANULL));

                return await Task.FromResult(PayLoad<object>.Successfully(loadDataOutIn(checkProduct)));
            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        private ProductInOut loadDataOutIn(product data)
        {
            return new ProductInOut
            {
                title = data.title,
                quantity = _context.product_location.Where(x => x.product_id == data.id).Sum(x => x.quantity),
                InOutByProducts = loadDataInOut(data.id),
                supplier = data.suppliers == null ? Status.DATANULL : data.suppliers.name,
                supplierTitle = data.suppliers == null ? Status.DATANULL : data.suppliers.title,
                history = loadDataHistory(data),
                locationProductDatas = loadData(data)
            };
        }

        private List<locationProductData> loadData(product data)
        {
            var list = new List<locationProductData>();

            var checkproduct = _context.product_location.Include(x => x.location_Addrs).Where(x => x.product_id == data.id).ToList();
            foreach (var item in checkproduct)
            {
                list.Add(new locationProductData
                {
                    area = item.location_Addrs.area,
                    line = item.location_Addrs.line,
                    shelf = item.location_Addrs.shelf,
                    code = item.location_Addrs.code_location_addr,
                    quantity = item.quantity,
                });
            }
            return list;
        }
        private List<InOutByProduct> loadDataInOut(int id)
        {
            var list = new List<InOutByProduct>();

            var checkUpdateHistory = _context.update_history.Include(p => p.products).Include(l => l.location_Addrs).Where(x => x.product_id == id && (x.status == 1 || x.status == 0)).ToList();

            foreach (var product in checkUpdateHistory)
            {
                list.Add(new InOutByProduct
                {
                    location = product.location_Addrs.code_location_addr,
                    updateat = product.last_modify_date,
                    quantity = product.quantity,
                    status = product.status
                });
            }

            return list;
        }

        public byte[] FindAllDownLoadExcel(int id)
        {
            var checkDataSupplier = _context.product
        .Where(x => x.id == id)
        .Include(x => x.suppliers)
        .Include(pl => pl.product_Locations)
            .ThenInclude(l => l.location_Addrs)
        .Include(u => u.update_Histories)
            .ThenInclude(l => l.location_Addrs)
        .AsNoTracking()
        .Select(x => new dataProductLocation
        {
            Id = x.id,
            title = x.title,
            nameSupplier = x.suppliers.title,
            dataLocations = x.product_Locations.Select(lc => new dataLocation
            {
                code = lc.location_Addrs.code_location_addr,
                area = lc.location_Addrs.area,
                line = lc.location_Addrs.line,
                shelf = lc.location_Addrs.shelf,
                quantity = lc.quantity
            }).ToList()
        }).FirstOrDefault();
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");
                worksheet.Cells[1, 1].Value = "產品";
                worksheet.Cells[1, 2].Value = "區域";
                worksheet.Cells[1, 3].Value = "線";
                worksheet.Cells[1, 4].Value = "架子";
                worksheet.Cells[1, 5].Value = "地點";
                worksheet.Cells[1, 6].Value = "供應商";
                worksheet.Cells[1, 7].Value = "供應商";

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

                worksheet.Cells[row, 1].Value = checkDataSupplier.title;
                worksheet.Cells[row, 6].Value = checkDataSupplier.nameSupplier;
                foreach (var product in checkDataSupplier.dataLocations)
                {

                    worksheet.Cells[row, 2].Value = product.area;
                    worksheet.Cells[row, 3].Value = product.line;
                    worksheet.Cells[row, 4].Value = product.shelf;
                    worksheet.Cells[row, 4].Value = product.code;
                    worksheet.Cells[row, 7].Value = product.quantity;

                    row++;
                }

                worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
                return package.GetAsByteArray();
            }
        }

        //public byte[] FindAllDownLoadExcelByCodeProduct(string code)
        //{
        //    var checkDataSupplier = _context.product
        //        .Where(x => x.title == code)
        //        .Include(x => x.suppliers)
        //        .Include(pl => pl.product_Locations)
        //        .ThenInclude(l => l.location_Addrs)
        //        .Include(u => u.update_Histories)
        //        .ThenInclude(l => l.location_Addrs)
        //        .AsNoTracking()
        //        .Select(x => new dataProductLocation
        //        {
        //        Id = x.id,
        //        title = x.title,
        //        nameSupplier = x.suppliers.title,
        //        dataLocations = x.product_Locations.Select(lc => new dataLocation
        //        {
        //            code = lc.location_Addrs.code_location_addr,
        //            area = lc.location_Addrs.area,
        //            line = lc.location_Addrs.line,
        //            shelf = lc.location_Addrs.shelf,
        //            quantity = lc.quantity
        //        }).ToList()
        //        }).ToList();
        //    using (var package = new ExcelPackage())
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("Products");
        //        worksheet.Cells[1, 1].Value = "產品";
        //        worksheet.Cells[1, 2].Value = "區域";
        //        worksheet.Cells[1, 3].Value = "線";
        //        worksheet.Cells[1, 4].Value = "架子";
        //        worksheet.Cells[1, 5].Value = "地點";
        //        worksheet.Cells[1, 6].Value = "供應商";
        //        worksheet.Cells[1, 7].Value = "數量";

        //        // Định dạng tiêu đề
        //        using (var range = worksheet.Cells[1, 1, 1, 7])
        //        {
        //            range.Style.Font.Bold = true; // Chữ in đậm
        //            range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Nền đặc
        //            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Nền xám nhạt
        //            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Căn giữa nội dung
        //        }

        //        // Đổ dữ liệu vào file Excel
        //        int row = 2;

        //        foreach (var product in checkDataSupplier)
        //        {
        //            worksheet.Cells[row, 1].Value = product.title;
        //            worksheet.Cells[row, 6].Value = product.nameSupplier;

        //            if (product.dataLocations != null && product.dataLocations.Any() && product.dataLocations.Count > 0)
        //            {

        //                foreach (var product2 in product.dataLocations)
        //                {
        //                    worksheet.Cells[row, 3].Value = product2.area;
        //                    worksheet.Cells[row, 4].Value = product2.line;
        //                    worksheet.Cells[row, 5].Value = product2.shelf;
        //                    worksheet.Cells[row, 6].Value = product2.code;
        //                    worksheet.Cells[row, 7].Value = product2.quantity;

        //                    row++;

        //                }
        //            }
        //            else
        //            {
        //                row++;
        //            }
        //        }


        //        worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
        //        return package.GetAsByteArray();
        //    }
        //}

        public byte[] FindAllDownLoadExcelByCodeProduct(string code)
        {
            var checkDataSupplier = _context.product
        .Where(x => x.title == code)
        .Include(x => x.suppliers)
        .Include(pl => pl.product_Locations)
            .ThenInclude(l => l.location_Addrs)
        .Include(u => u.update_Histories)
            .ThenInclude(l => l.location_Addrs)
        .AsNoTracking()
        .Select(x => new dataProductLocation
        {
            Id = x.id,
            title = x.title,
            nameSupplier = x.suppliers.title,
            dataLocations = x.product_Locations.Select(lc => new dataLocation
            {
                code = lc.location_Addrs.code_location_addr,
                area = lc.location_Addrs.area,
                line = lc.location_Addrs.line,
                shelf = lc.location_Addrs.shelf,
                quantity = lc.quantity
            }).ToList(),
            inOutByProducts = x.update_Histories.Where(s => s.status == 1 || s.status == 0).Select(ud => new InOutByProduct
            {
                location = ud.location_Addrs.code_location_addr,
                quantity = ud.quantity,
                status = ud.status,
                updateat = ud.last_modify_date
            }).ToList()
        }).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Products");
                worksheet.Cells[1, 1].Value = "產品";
                worksheet.Cells[1, 2].Value = "數量";
                worksheet.Cells[1, 3].Value = "地位";
                worksheet.Cells[1, 4].Value = "地點";
                worksheet.Cells[1, 5].Value = "時間";
                worksheet.Cells[1, 6].Value = "地點產品";
                worksheet.Cells[1, 7].Value = "區域";
                worksheet.Cells[1, 8].Value = "線";
                worksheet.Cells[1, 9].Value = "地點";
                worksheet.Cells[1, 10].Value = "數量";
                worksheet.Cells[1, 11].Value = "供應商";

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 11])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                int row = 2; // Bắt đầu từ dòng thứ 2

                foreach (var product in checkDataSupplier)
                {
                    int rowStart = row;
                    int maxRows = Math.Max(product.dataLocations.Count, product.inOutByProducts.Count);

                    for (int i = 0; i < maxRows; i++)
                    {
                        if (i == 0)
                        {
                            worksheet.Cells[row, 1].Value = product.title;
                            worksheet.Cells[row, 11].Value = product.nameSupplier;
                        }

                        // Ghi dữ liệu từ danh sách địa chỉ
                        if (i < product.dataLocations.Count)
                        {
                            var location = product.dataLocations[i];
                            worksheet.Cells[row, 6].Value = location.code;
                            worksheet.Cells[row, 7].Value = location.area;
                            worksheet.Cells[row, 8].Value = location.line;
                            worksheet.Cells[row, 9].Value = location.shelf;
                            worksheet.Cells[row, 10].Value = location.quantity;
                        }

                        // Ghi dữ liệu từ danh sách nhập xuất
                        if (i < product.inOutByProducts.Count)
                        {
                            var history = product.inOutByProducts[i];
                            worksheet.Cells[row, 2].Value = history.quantity;
                            worksheet.Cells[row, 3].Value = history.status == 1 ? "Import" : "Deliverynote";
                            worksheet.Cells[row, 4].Value = history.location;
                            worksheet.Cells[row, 5].Value = history.updateat;
                        }

                        row++;
                    }

                    // Merge cells cho cột Product và Supplier nếu có nhiều dòng
                    if (maxRows > 1)
                    {
                        worksheet.Cells[rowStart, 1, row - 1, 1].Merge = true; // Merge Product
                        worksheet.Cells[rowStart, 11, row - 1, 11].Merge = true; // Merge Supplier
                        worksheet.Cells[rowStart, 1, row - 1, 11].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                }

                worksheet.Cells.AutoFitColumns(); // Tự động căn chỉnh cột
                return package.GetAsByteArray();
            }
        }

        public byte[] FindAllDownLoadExcelBySupplier(int supplier)
        {
            var checkDataSupplier = _context.product
        .Where(x => x.warehouseID == supplier)
        .Include(x => x.suppliers)
        .Include(pl => pl.product_Locations)
            .ThenInclude(l => l.location_Addrs)
        .Include(u => u.update_Histories)
            .ThenInclude(l => l.location_Addrs)
        .AsNoTracking()
        .Select(x => new dataProductLocation
        {
            Id = x.id,
            title = x.title,
            nameSupplier = x.suppliers.title,
            dataLocations = x.product_Locations.Select(lc => new dataLocation
            {
                code = lc.location_Addrs.code_location_addr,
                area = lc.location_Addrs.area,
                line = lc.location_Addrs.line,
                shelf = lc.location_Addrs.shelf,
                quantity = lc.quantity
            }).ToList(),
            inOutByProducts = x.update_Histories.Select(ud => new InOutByProduct
            {
                location = ud.location_Addrs.code_location_addr,
                quantity = ud.quantity,
                status = ud.status,
                updateat = ud.last_modify_date
            }).ToList()
        }).ToList();

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

                foreach (var product in checkDataSupplier)
                {
                    worksheet.Cells[row, 1].Value = product.title;
                    worksheet.Cells[row, 6].Value = product.nameSupplier;

                    if (product.dataLocations != null && product.dataLocations.Any() && product.dataLocations.Count > 0)
                    {

                        foreach (var product2 in product.dataLocations)
                        {
                            worksheet.Cells[row, 3].Value = product2.area;
                            worksheet.Cells[row, 4].Value = product2.line;
                            worksheet.Cells[row, 5].Value = product2.shelf;
                            worksheet.Cells[row, 6].Value = product2.code;
                            worksheet.Cells[row, 7].Value = product2.quantity;

                            row++;

                        }
                    }
                    else
                    {
                        row++;
                    }
                }


                worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
                return package.GetAsByteArray();
            }
        }

        public byte[] FindAllDownLoadExcelByCodeProductList(List<string> code)
        {
            var list = new List<ProductInOut>();
            foreach (var itemData in code)
            {
                var data = _context.product.Include(s => s.suppliers).Where(x => x.title == itemData).ToList();

                foreach (var item in data)
                {
                    list.Add(loadDataOutIn(item));
                }
            }
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

                // Định dạng tiêu đề
                using (var range = worksheet.Cells[1, 1, 1, 6])
                {
                    range.Style.Font.Bold = true; // Chữ in đậm
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Nền đặc
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Nền xám nhạt
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Căn giữa nội dung
                }

                // Đổ dữ liệu vào file Excel
                int row = 2;

                int rowItem = 2;

                foreach (var product in list)
                {
                    worksheet.Cells[row, 1].Value = product.title;

                    worksheet.Cells[row, 6].Value = product.supplierTitle;
                    if (product.locationProductDatas != null && product.locationProductDatas.Any() && product.locationProductDatas.Count > 0)
                    {

                        foreach (var product2 in product.locationProductDatas)
                        {
                            worksheet.Cells[row, 2].Value = product2.area;
                            worksheet.Cells[row, 3].Value = product2.line;
                            worksheet.Cells[row, 4].Value = product2.shelf;
                            worksheet.Cells[row, 5].Value = product2.code;
                            worksheet.Cells[row, 7].Value = product2.quantity;

                            row++;

                        }
                    }
                    else
                    {
                        row++;
                    }

                }

                worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
                return package.GetAsByteArray();
            }
        }

        //public byte[] FindAllDownLoadExcel(int id)
        //{
        //    var data = _context.product.FirstOrDefault(x => x.id == id);

        //    var dataMap = loadDataOutIn(data);
        //    using (var package = new ExcelPackage())
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("Products");
        //        worksheet.Cells[1, 1].Value = "Product";
        //        worksheet.Cells[1, 2].Value = "Quantity";
        //        worksheet.Cells[1, 3].Value = "Status";
        //        worksheet.Cells[1, 4].Value = "Location";
        //        worksheet.Cells[1, 5].Value = "Date";
        //        worksheet.Cells[1, 6].Value = "Quantity";
        //        worksheet.Cells[1, 7].Value = "Warehouse ID";

        //        // Định dạng tiêu đề
        //        using (var range = worksheet.Cells[1, 1, 1, 6])
        //        {
        //            range.Style.Font.Bold = true; // Chữ in đậm
        //            range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Nền đặc
        //            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Nền xám nhạt
        //            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Căn giữa nội dung
        //        }

        //        // Đổ dữ liệu vào file Excel
        //        int row = 2;

        //        worksheet.Cells[row, 1].Value = dataMap.title;
        //        worksheet.Cells[row, 2].Value = dataMap.quantity;
        //        foreach (var product in dataMap.InOutByProducts)
        //        {

        //            worksheet.Cells[row, 3].Value = product.status == 1 ? "Import" : "Deliverynote";
        //            worksheet.Cells[row, 4].Value = product.location;
        //            worksheet.Cells[row, 5].Value = product.updateat;
        //            worksheet.Cells[row, 6].Value = product.quantity;

        //            row++;
        //        }

        //        worksheet.Cells[row, 7].Value = dataMap.supplier;
        //        worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
        //        return package.GetAsByteArray();
        //    }
        //}

        //public byte[] FindAllDownLoadExcelByCodeProduct(string code)
        //{
        //    var list = new List<ProductInOut>();
        //    var data = _context.product.Include(s => s.suppliers).Where(x => x.title == code).ToList();

        //    foreach(var item in data)
        //    {
        //        list.Add(loadDataOutIn(item));
        //    }
        //    using (var package = new ExcelPackage())
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("Products");
        //        worksheet.Cells[1, 1].Value = "Product";
        //        worksheet.Cells[1, 2].Value = "Quantity";
        //        worksheet.Cells[1, 3].Value = "Status";
        //        worksheet.Cells[1, 4].Value = "Location";
        //        worksheet.Cells[1, 5].Value = "Date";
        //        worksheet.Cells[1, 6].Value = "Quantity";
        //        worksheet.Cells[1, 7].Value = "Warehouse ID";

        //        // Định dạng tiêu đề
        //        using (var range = worksheet.Cells[1, 1, 1, 7])
        //        {
        //            range.Style.Font.Bold = true; // Chữ in đậm
        //            range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Nền đặc
        //            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Nền xám nhạt
        //            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Căn giữa nội dung
        //        }

        //        // Đổ dữ liệu vào file Excel
        //        int row = 2;

        //        int rowItem = 2;

        //        foreach (var product in list)
        //        {
        //            worksheet.Cells[row, 1].Value = product.title;
        //            worksheet.Cells[row, 2].Value = product.quantity;

        //            if (product.InOutByProducts != null && product.InOutByProducts.Any() && product.InOutByProducts.Count > 0)
        //            {
        //                worksheet.Cells[row, 7].Value = product.supplier;

        //                foreach (var product2 in product.InOutByProducts)
        //                {
        //                    worksheet.Cells[row, 3].Value = product2.status == 1 ? "Import" : "Deliverynote";
        //                    worksheet.Cells[row, 4].Value = product2.location;
        //                    worksheet.Cells[row, 5].Value = product2.updateat;
        //                    worksheet.Cells[row, 6].Value = product2.quantity;

        //                    row++;

        //                }
        //            }
        //            else
        //            {
        //                worksheet.Cells[row, 7].Value = product.supplier;
        //                row++;
        //            }
        //        }

        //        worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
        //        return package.GetAsByteArray();
        //    }
        //}

        //public byte[] FindAllDownLoadExcelByCodeProductList(List<string> code)
        //{
        //    var list = new List<ProductInOut>();
        //    foreach (var itemData in code)
        //    {
        //        var data = _context.product.Include(s => s.suppliers).Where(x => x.title == itemData).ToList();

        //        foreach (var item in data)
        //        {
        //            list.Add(loadDataOutIn(item));
        //        }
        //    }
        //    using (var package = new ExcelPackage())
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("Products");
        //        worksheet.Cells[1, 1].Value = "Product";
        //        worksheet.Cells[1, 2].Value = "Quantity";
        //        worksheet.Cells[1, 3].Value = "Status";
        //        worksheet.Cells[1, 4].Value = "Location";
        //        worksheet.Cells[1, 5].Value = "Date";
        //        worksheet.Cells[1, 6].Value = "Quantity";
        //        worksheet.Cells[1, 7].Value = "Warehouse ID";

        //        // Định dạng tiêu đề
        //        using (var range = worksheet.Cells[1, 1, 1, 6])
        //        {
        //            range.Style.Font.Bold = true; // Chữ in đậm
        //            range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Nền đặc
        //            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray); // Nền xám nhạt
        //            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Căn giữa nội dung
        //        }

        //        // Đổ dữ liệu vào file Excel
        //        int row = 2;

        //        int rowItem = 2;

        //        foreach (var product in list)
        //        {
        //            worksheet.Cells[row, 1].Value = product.title;
        //            worksheet.Cells[row, 2].Value = product.quantity;
        //            if(product.InOutByProducts != null && product.InOutByProducts.Any() && product.InOutByProducts.Count > 0)
        //            {
        //                worksheet.Cells[row, 7].Value = product.supplier;

        //                foreach (var product2 in product.InOutByProducts)
        //                {
        //                    worksheet.Cells[row, 3].Value = product2.status == 1 ? "Import" : "Deliverynote";
        //                    worksheet.Cells[row, 4].Value = product2.location;
        //                    worksheet.Cells[row, 5].Value = product2.updateat;
        //                    worksheet.Cells[row, 6].Value = product2.quantity;

        //                    row++;

        //                }
        //            }
        //            else
        //            {
        //                worksheet.Cells[row, 7].Value = product.supplier;
        //                row++;
        //            }

        //        }

        //        worksheet.Cells.AutoFitColumns(); // Tự động chỉnh độ rộng cột
        //        return package.GetAsByteArray();
        //    }
        //}

        //public byte[] FindAllDownLoadExcelBySupplier(int supplier)
        //{
        //    var checkDataSupplier = _context.product
        //.Where(x => x.warehouseID == supplier)
        //.Include(x => x.suppliers)
        //.Include(pl => pl.product_Locations)
        //    .ThenInclude(l => l.location_Addrs)
        //.Include(u => u.update_Histories)
        //    .ThenInclude(l => l.location_Addrs)
        //.AsNoTracking()
        //.Select(x => new dataProductLocation
        //{
        //    Id = x.id,
        //    title = x.title,
        //    nameSupplier = x.suppliers.title,
        //    dataLocations = x.product_Locations.Select(lc => new dataLocation
        //    {
        //        code = lc.location_Addrs.code_location_addr,
        //        area = lc.location_Addrs.area,
        //        line = lc.location_Addrs.line,
        //        shelf = lc.location_Addrs.shelf,
        //        quantity = lc.quantity
        //    }).ToList(),
        //    inOutByProducts = x.update_Histories.Select(ud => new InOutByProduct
        //    {
        //        location = ud.location_Addrs.code_location_addr,
        //        quantity = ud.quantity,
        //        status = ud.status,
        //        updateat = ud.last_modify_date
        //    }).ToList()
        //}).ToList();

        //    using (var package = new ExcelPackage())
        //    {
        //        var worksheet = package.Workbook.Worksheets.Add("Products");
        //        worksheet.Cells[1, 1].Value = "Product";
        //        worksheet.Cells[1, 2].Value = "Quantity";
        //        worksheet.Cells[1, 3].Value = "Status";
        //        worksheet.Cells[1, 4].Value = "Location";
        //        worksheet.Cells[1, 5].Value = "Date";
        //        worksheet.Cells[1, 6].Value = "Location Product";
        //        worksheet.Cells[1, 7].Value = "Area";
        //        worksheet.Cells[1, 8].Value = "Line";
        //        worksheet.Cells[1, 9].Value = "Shelf";
        //        worksheet.Cells[1, 10].Value = "Supplier";

        //        // Định dạng tiêu đề
        //        using (var range = worksheet.Cells[1, 1, 1, 10])
        //        {
        //            range.Style.Font.Bold = true;
        //            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
        //            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
        //            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        //        }

        //        int row = 2; // Bắt đầu từ dòng thứ 2

        //        foreach (var product in checkDataSupplier)
        //        {
        //            int rowStart = row;
        //            int maxRows = Math.Max(product.dataLocations.Count, product.inOutByProducts.Count);

        //            for (int i = 0; i < maxRows; i++)
        //            {
        //                if (i == 0)
        //                {
        //                    worksheet.Cells[row, 1].Value = product.title;
        //                    worksheet.Cells[row, 10].Value = product.nameSupplier;
        //                }

        //                // Ghi dữ liệu từ danh sách địa chỉ
        //                if (i < product.dataLocations.Count)
        //                {
        //                    var location = product.dataLocations[i];
        //                    worksheet.Cells[row, 6].Value = location.code;
        //                    worksheet.Cells[row, 7].Value = location.area;
        //                    worksheet.Cells[row, 8].Value = location.line;
        //                    worksheet.Cells[row, 9].Value = location.shelf;
        //                }

        //                // Ghi dữ liệu từ danh sách nhập xuất
        //                if (i < product.inOutByProducts.Count)
        //                {
        //                    var history = product.inOutByProducts[i];
        //                    worksheet.Cells[row, 2].Value = history.quantity;
        //                    worksheet.Cells[row, 3].Value = history.status == 1 ? "Import" : "Deliverynote";
        //                    worksheet.Cells[row, 4].Value = history.location;
        //                    worksheet.Cells[row, 5].Value = history.updateat;
        //                }

        //                row++;
        //            }

        //            // Merge cells cho cột Product và Supplier nếu có nhiều dòng
        //            if (maxRows > 1)
        //            {
        //                worksheet.Cells[rowStart, 1, row - 1, 1].Merge = true; // Merge Product
        //                worksheet.Cells[rowStart, 10, row - 1, 10].Merge = true; // Merge Supplier
        //                worksheet.Cells[rowStart, 1, row - 1, 10].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        //            }
        //        }

        //        worksheet.Cells.AutoFitColumns(); // Tự động căn chỉnh cột
        //        return package.GetAsByteArray();
        //    }
        //}
    }
}
