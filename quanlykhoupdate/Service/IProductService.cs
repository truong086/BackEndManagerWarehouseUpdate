﻿using quanlykhoupdate.common;

namespace quanlykhoupdate.Service
{
    public interface IProductService
    {
        Task<PayLoad<object>> findAll(string? name, int page = 1, int pageSize = 20);
        Task<PayLoad<object>> findOne(string? name, int page = 1, int pageSize = 20);
        Task<PayLoad<object>> findOneByOutAndIn(int id);
        Task<PayLoad<object>> findBySuppliers(int id, int page = 1, int pageSize = 20);
        Task<PayLoad<object>> findOneCode(string? name);
        Task<PayLoad<string>> AddDataSupplier();
        Task<PayLoad<object>> ImportDataExcel(IFormFile file);
    }
}
