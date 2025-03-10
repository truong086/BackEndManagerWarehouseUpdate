﻿using Microsoft.EntityFrameworkCore;
using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;
using System.Drawing.Printing;

namespace quanlykhoupdate.Service
{
    public class inboundService : IinboundService
    {
        private readonly DBContext _context;
        public inboundService(DBContext context)
        {
            _context = context;
        }

        public async Task<PayLoad<inboundDTO>> Add(inboundDTO inboundDTO)
        {
            try
            {
                if(!checkProduct(inboundDTO.productListInbounds))
                    return await Task.FromResult(PayLoad<inboundDTO>.CreatedFail(Status.DATANULL));

                var checkDataProduct = inboundDTO.productListInbounds.GroupBy(x => x.productId) // Kiểm tra dữ liệu chuyền lên của product có trùng nhau không
                                        .Where(x => x.Count() > 1)
                                        .Select(g => new
                                        {
                                            product = g.Key,
                                            count = g.Count()
                                        }).ToList();

                if (checkDataProduct.Any())
                {
                    return await Task.FromResult(PayLoad<inboundDTO>.CreatedFail(Status.DATATONTAI));
                }
                _context.inbound.Add(new inbound
                {
                    is_action = false
                });

                _context.SaveChanges();

                var dataNew = _context.inbound.OrderByDescending(x => x.id).FirstOrDefault(x => x.is_action == false);
                dataNew.code = RanDomCode.geneAction(6) + dataNew.id;

                Addproductinbound(inboundDTO.productListInbounds, dataNew);

                _context.SaveChanges();

                return await Task.FromResult(PayLoad<inboundDTO>.Successfully(inboundDTO));
            }
            catch (Exception ex)    
            {
                return await Task.FromResult(PayLoad<inboundDTO>.CreatedFail(ex.Message));
            }
        }

        private void Addproductinbound(List<productListInbound> data, inbound dataNew)
        {
            foreach(var item in data)
            {
                var checkproduct = _context.product.FirstOrDefault(x => x.id == item.productId);
                _context.inbound_product.Add(new inbound_product
                {
                    inbounds = dataNew,
                    inbound_id = dataNew.id,
                    products = checkproduct,
                    product_id = checkproduct.id,
                    quantity = item.quantity.Value
                });

                _context.SaveChanges();
            }
        }
        private bool checkProduct(List<productListInbound> data)
        {
            foreach(var item in data)
            {
                var checkProduct = _context.product.FirstOrDefault(x => x.id == item.productId);
                if (checkProduct == null)
                    return false;

                var checkLocation = _context.product_location.FirstOrDefault(x => x.product_id == item.productId);
                if (checkLocation == null)
                    return false;

            }
            return true;
        }

        public async Task<PayLoad<object>> FindAll(int page = 1, int pageSize = 20)
        {
            try
            {
                var data = _context.inbound.ToList();

                var pageList = new PageList<object>(loadData(data), page - 1, pageSize);

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

        private List<findAllInbound> loadData(List<inbound> data)
        {
            var list = new List<findAllInbound>();

            foreach (var item in data)
            {
                list.Add(findOneData(item));
            }
            return list;
        }

        private findAllInbound findOneData(inbound item)
        {
            var dataItem = new findAllInbound();

            dataItem.code = item.code;
            dataItem.id = item.id;
            dataItem.quantity = _context.inbound_product.Where(x => x.inbound_id == item.id).Count();
            dataItem.quantityProduct = _context.inbound_product.Where(x => x.inbound_id == item.id).Sum(x => x.quantity);
            dataItem.locationDataInbounds = loadLocationData(item.id);
            dataItem.isAction = item.is_action;
            return dataItem;
        }

        private List<LocationDataInbound> loadLocationData(int id)
        {
            var list = new List<LocationDataInbound>();

            var checkData = _context.inbound_product.Where(x => x.inbound_id == id).ToList();
            foreach (var item in checkData)
            {
                var checkLocation = _context.product_location.Include(p => p.products).Include(x => x.location_Addrs).FirstOrDefault(x => x.product_id == item.product_id);
                if (checkLocation != null)
                    list.Add(new LocationDataInbound
                    {
                        id = checkLocation.products.id,
                        title = checkLocation.products.title,
                        area = checkLocation.location_Addrs.area,
                        line = checkLocation.location_Addrs.line,
                        shelf = checkLocation.location_Addrs.shelf,
                        code = checkLocation.location_Addrs.code_location_addr,
                        quantity = item.quantity
                    });
            }

            return list;

        }
        public async Task<PayLoad<object>> FindAllNoIsAction(int page = 1, int pageSize = 20)
        {
            try
            {
                var data = _context.inbound.Where(x => x.is_action == true).ToList();

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
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }

        public async Task<PayLoad<UpdateCodeInbound>> UpdateCode(List<UpdateCodeInbound> inboundDTO)
        {
            try
            {
                foreach(var item in inboundDTO)
                {
                    var checkData = _context.inbound.FirstOrDefault(x => x.code == item.code);
                    if (checkData == null)
                        return await Task.FromResult(PayLoad<UpdateCodeInbound>.CreatedFail(Status.DATANULL));

                    updateLocationQuantity(checkData);

                    checkData.is_action = true;

                    _context.inbound.Update(checkData);
                    _context.SaveChanges();
                }

                return await Task.FromResult(PayLoad<UpdateCodeInbound>.Successfully(new UpdateCodeInbound
                {
                    code = Status.SUCCESS
                }));
            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<UpdateCodeInbound>.CreatedFail(ex.Message));
            }
        }

        private void updateLocationQuantity(inbound data)
        {
            var checkData = _context.inbound_product.Where(x => x.inbound_id == data.id).ToList();
            foreach(var item in checkData)
            {
                var checkProductLocation = _context.product_location.FirstOrDefault(x => x.product_id == item.product_id);
                if(checkProductLocation != null)
                {
                    checkProductLocation.quantity += item.quantity;
                    _context.product_location.Update(checkProductLocation);
                    _context.SaveChanges();

                    saveUpdateHistory(item.product_id, checkProductLocation.id, item.quantity);
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
                status = 1, 
                last_modify_date = DateTime.UtcNow
            };

            _context.update_history.Add(updateHistory);
            _context.SaveChanges();
        }

        public async Task<PayLoad<object>> FindCode(string code)
        {
            try
            {
                var data = _context.inbound.FirstOrDefault(x => x.code == code);

                return await Task.FromResult(PayLoad<object>.Successfully(new
                {
                    data = findOneData(data)
                }));

            }
            catch (Exception ex)
            {
                return await Task.FromResult(PayLoad<object>.CreatedFail(ex.Message));
            }
        }
    }
}
