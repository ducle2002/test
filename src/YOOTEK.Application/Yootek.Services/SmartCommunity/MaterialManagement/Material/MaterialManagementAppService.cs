using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using System;
using System.Threading.Tasks;
using System.Linq;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using Abp.Application.Services;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Yootek.Core.Dto;
using Yootek.DataExporting.Excel.NPOI;
using Yootek.Storage;
using Abp.Collections.Extensions;

namespace Yootek.Services
{
    public interface IMaterialManagementAppService : IApplicationService
    {
        Task<object> GetAllAsync(GetAllMaterialInputDto input);
        Task<object> Create(MaterialDto input);
        Task<object> Update(MaterialDto input);
        Task<object> Delete(long id);

    }

    public interface IMaterialExcelExport
    {
        FileDto ExportToFile(List<MaterialDto> input);
    }

    public class MaterialExcelExport : NpoiExcelExporterBase, IMaterialExcelExport
    {
        public MaterialExcelExport(ITempFileCacheManager tempFileCacheManager) : base(tempFileCacheManager) { }
        public FileDto ExportToFile(List<MaterialDto> input)
        {
            return CreateExcelPackage("materials.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.CreateSheet("Materials");
                    /*AddHeader(sheet,
                        L("MaterialCode"),
                        L("MaterialName"),
                        L("MaterialType"),
                        L("MaterialGroup"),
                        L("Price"),
                        L("SerialNo"),
                        L("AssetLocation"),
                        L("Status"),
                        L("Description")
                    );*/
                    AddHeader(sheet,
                        "Mã tài sản",
                        "Tên tài sản",
                        "Loại tài sản",
                        "Nhóm tài sản",
                        "Đơn giá",
                        "Số serial",
                        "Vị trí tài sản",
                        L("Status"),
                        L("Description")
                    );
                    AddObjects(sheet, input,
                        _ => _.MaterialCode,
                        _ => _.MaterialName,
                        _ => _.TypeName,
                        _ => _.GroupName,
                        _ => _.Price,
                        _ => _.SerialNumber,
                        _ => _.LocationName,
                        _ => _.Status,
                        _ => _.Description);
                });
        }
    }

    [AbpAuthorize]
    public class MaterialManagementAppService : YootekAppServiceBase, IMaterialManagementAppService
    {

        private readonly IRepository<Material, long> _materialRepos;
        private readonly IRepository<MaterialCategory, long> _materialCategoryRepos;
        private readonly IRepository<Inventory, long> _inventoryRepos;
        private readonly IMaterialExcelExport _materialExcelExport;

        public MaterialManagementAppService(
            IRepository<Material, long> materialRepos,
            IRepository<MaterialCategory, long> materialCategoryRepos,
            IRepository<Inventory, long> inventoryRepos,
            IMaterialExcelExport materialExcelExport
            )
        {
            _materialRepos = materialRepos;
            _materialCategoryRepos = materialCategoryRepos;
            _inventoryRepos = inventoryRepos;
            _materialExcelExport = materialExcelExport;
        }

        public async Task<object> GetAllAsync(GetAllMaterialInputDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var queryInput = new QueryMaterialDto()
                    {
                        BuildingId = input.BuildingId,
                        CategoryType = input.CategoryType,
                        FormQuery = 1,
                        Keyword = input.Keyword,
                        UrbanId = input.UrbanId,
                        TypeId = input.TypeId,
                        Status = input.Status,
                        LocationId = input.LocationId,

                    };
                    var query = await QueryMaterialAsync(queryInput);

                    if (input.LocationId > 0)
                    {
                        query = query.Where(x => x.LocationId == input.LocationId);
                    }
                    else if (input.LocationId == 0)
                    {
                        query = query.Where(x => x.LocationId == null);
                    }

                    var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> GetAllMaterialInventoryAsync(GetAllMaterialInputDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {

                    var query = await QueryInventory(input);

                    if (input.LocationId > 0)
                    {
                        query = query.Where(x => x.LocationId == input.LocationId);
                    }
                    else if (input.LocationId == 0)
                    {
                        query = query.Where(x => x.LocationId == null);
                    }

                    var result = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();

                    var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                    return data;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        private async Task<IQueryable<MaterialDto>> QueryMaterialAsync(QueryMaterialDto input)
        {
            var query = (from mt in _materialRepos.GetAll()
                         join gr in _materialCategoryRepos.GetAll() on mt.GroupId equals gr.Id into tb_gr
                         from gr in tb_gr.DefaultIfEmpty()
                         join tp in _materialCategoryRepos.GetAll() on mt.TypeId equals tp.Id into tb_tp
                         from tp in tb_tp.DefaultIfEmpty()
                         join un in _materialCategoryRepos.GetAll() on mt.UnitId equals un.Id into tb_un
                         from un in tb_un.DefaultIfEmpty()
                         join lc in _materialCategoryRepos.GetAll() on mt.LocationId equals lc.Id into tb_lc
                         from lc in tb_lc.DefaultIfEmpty()
                         join pd in _materialCategoryRepos.GetAll() on mt.ProducerId equals pd.Id into tb_pd
                         from pd in tb_pd.DefaultIfEmpty()
                         select new MaterialDto
                         {
                             Id = mt.Id,
                             BuildingId = mt.BuildingId,
                             Description = mt.Description,
                             ImageUrl = mt.ImageUrl,
                             UrbanId = mt.UrbanId,
                             Amount = mt.Amount,
                             CreationTime = mt.CreationTime,
                             ExpiredDate = mt.ExpiredDate,
                             FileUrl = mt.FileUrl,
                             GroupId = mt.GroupId,
                             LocationId = mt.LocationId,
                             ManufacturerDate = mt.ManufacturerDate,
                             MaterialCode = mt.MaterialCode,
                             MaterialName = mt.MaterialName,
                             OrganizationUnitId = mt.OrganizationUnitId,
                             Price = mt.Price,
                             ProducerId = mt.ProducerId,
                             SerialNumber = mt.SerialNumber,
                             Specifications = mt.Specifications,
                             TotalPrice = mt.TotalPrice,
                             TypeId = mt.TypeId,
                             Status = mt.Status,
                             UnitId = mt.UnitId,
                             UsedDate = mt.UsedDate,
                             WearRate = mt.WearRate,
                             WarrantyMonth = mt.WarrantyMonth,

                             GroupName = gr.Name,
                             LocationName = lc.Name,
                             TypeName = tp.Name,
                             ProducerName = pd.Name,
                             UnitName = un.Name,
                             TenantId = un.TenantId,


                         })
                         .WhereIf(input.BuildingId.HasValue, u => u.BuildingId == input.BuildingId)
                           .WhereIf(input.UrbanId.HasValue, u => u.UrbanId == input.UrbanId)
                           .WhereIf(input.TypeId.HasValue, u => u.TypeId == input.TypeId)
                           .WhereIf(input.Status != null, u => u.Status == input.Status)
                           .WhereIf(input.LocationId.HasValue, u => u.LocationId == input.LocationId)
                           .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword), u => u.MaterialCode.ToLower().Contains(input.Keyword.ToLower()) || u.MaterialName.ToLower().Contains(input.Keyword.ToLower()))

                           .AsQueryable();
            return query;
        }

        private async Task<IQueryable<MaterialDto>> QueryInventory(GetAllMaterialInputDto input)
        {
            var queryInput = new QueryMaterialDto()
            {
                BuildingId = input.BuildingId,
                CategoryType = input.CategoryType,
                FormQuery = 1,
                Keyword = input.Keyword,
                UrbanId = input.UrbanId
            };
            var query = await QueryMaterialAsync(queryInput);
            var inventoryQuery = (from mt in query
                                  join iv in _inventoryRepos.GetAll() on mt.Id equals iv.MaterialId into tb_iv
                                  from iv in tb_iv
                                  select new MaterialDto
                                  {
                                      Id = iv.Id,
                                      BuildingId = mt.BuildingId,
                                      Description = mt.Description,
                                      ImageUrl = mt.ImageUrl,
                                      UrbanId = mt.UrbanId,
                                      Amount = iv.Amount,
                                      CreationTime = mt.CreationTime,
                                      ExpiredDate = mt.ExpiredDate,
                                      FileUrl = mt.FileUrl,
                                      GroupId = mt.GroupId,
                                      LocationId = iv.InventoryId,
                                      ManufacturerDate = mt.ManufacturerDate,
                                      MaterialCode = mt.MaterialCode,
                                      MaterialName = mt.MaterialName,
                                      OrganizationUnitId = mt.OrganizationUnitId,
                                      Price = iv.Price,
                                      ProducerId = mt.ProducerId,
                                      SerialNumber = mt.SerialNumber,
                                      Specifications = mt.Specifications,
                                      TotalPrice = mt.TotalPrice,
                                      TypeId = mt.TypeId,
                                      Status = mt.Status,
                                      UnitId = mt.UnitId,
                                      UsedDate = mt.UsedDate,
                                      WearRate = mt.WearRate,
                                      WarrantyMonth = mt.WarrantyMonth,

                                      GroupName = mt.GroupName,
                                      LocationName = mt.LocationName,
                                      TypeName = mt.TypeName,
                                      ProducerName = mt.ProducerName,
                                      UnitName = mt.UnitName,
                                      TenantId = mt.TenantId,
                                      MaterialId = iv.MaterialId,


                                  });

            return inventoryQuery;
        }


        public async Task<object> GetAllInventoryAsync(GetAllMaterialInputDto input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var inventorys = await _materialCategoryRepos.GetAll().Where(x => x.Type == CategoryType.Location).ToListAsync();

                    var inventoryQuery = await QueryInventory(input);

                    var result = new List<InventoryDictionaryDto>();
                    foreach (var item in inventorys)
                    {
                        var inventory = new InventoryDictionaryDto()
                        {
                            Id = item.Id,
                            Code = item.Code,
                            Name = item.Name,
                        };

                        var qr = inventoryQuery.Where(x => x.LocationId == item.Id);
                        inventory.CountMaterial = qr.Count();
                        inventory.CountAmount = qr.Sum(x => x.Amount.HasValue ? x.Amount.Value : 0);
                        result.Add(inventory);
                    }

                    //var tn = new InventoryDictionaryDto()
                    //{
                    //    Id = 0,
                    //    Code = "KHAC", 
                    //    Name = "Các tài sản khác",
                    //};
                    //var qur = query.Where(x => x.LocationId == null);
                    //tn.CountMaterial = qur.Count();
                    //tn.CountAmount = qur.Sum(x => x.Amount.HasValue ? x.Amount.Value : 0);
                    //result.Add(tn);

                    var data = DataResult.ResultSuccess(result, "Get success!");
                    return data;
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Create(MaterialDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;

                var insertInput = input.MapTo<Material>();


                long id = await _materialRepos.InsertAndGetIdAsync(insertInput);
                if (insertInput.LocationId > 0)
                {
                    var inventory = new Inventory()
                    {
                        Amount = input.Amount.Value,
                        InventoryId = insertInput.LocationId.Value,
                        MaterialId = id,
                        Price = input.Price.Value,
                        TenantId = AbpSession.TenantId,
                        UrbanId = insertInput.UrbanId,
                        BuildingId = insertInput.BuildingId
                    };
                    await _inventoryRepos.InsertAsync(inventory);
                }
                var data = DataResult.ResultSuccess(insertInput, "Insert success !");
                mb.statisticMetris(t1, 0, "is_material");

                return data;

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> CreateInventory(InventoryDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;

                var inventory = new Inventory()
                {
                    Amount = input.Amount.Value,
                    InventoryId = input.InventoryId,
                    MaterialId = input.MaterialId,
                    Price = input.Price.Value,
                    TenantId = AbpSession.TenantId,
                    UrbanId = input.UrbanId,
                    BuildingId = input.BuildingId,
                    TotalPrice = input.TotalPrice
                };
                await _inventoryRepos.InsertAsync(inventory);
                var data = DataResult.ResultSuccess(inventory, "Insert success !");
                mb.statisticMetris(t1, 0, "is_material");

                return data;

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> UpdateInventory(InventoryDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();

                input.TenantId = AbpSession.TenantId;
                var updateData = await _inventoryRepos.GetAsync(input.Id);
                if (updateData != null)
                {
                    input.MapTo(updateData);
                    await _inventoryRepos.UpdateAsync(updateData);
                }
                mb.statisticMetris(t1, 0, "Ud_administrative");
                var data = DataResult.ResultSuccess(input, "Insert success !");
                mb.statisticMetris(t1, 0, "is_material");

                return data;

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Update(MaterialDto input)
        {
            try
            {

                if (input.Id > 0)
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    //update
                    var updateData = await _materialRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _materialRepos.UpdateAsync(updateData);
                    }
                    mb.statisticMetris(t1, 0, "Ud_administrative");

                    var data = DataResult.ResultSuccess(updateData, "Update success !");
                    return data;
                }

                return null;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Delete(long id)
        {
            try
            {
                await _materialRepos.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> DeleteMultiple([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var task = Delete(id);
                    tasks.Add(task);
                }
                Task.WaitAll(tasks.ToArray());
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetMaterialStatisticsAsync(MaterialStatisticsDto input)
        {
            try
            {
                DateTime now = DateTime.Now;
                int currentMonth = now.Month;
                int currentYear = now.Year;

                Dictionary<string, Dictionary<string, MaterialStatisticsOutput>> dataResult = new Dictionary<string, Dictionary<string, MaterialStatisticsOutput>>();

                switch (input.QueryCase)
                {
                    case QueryCaseMaterial.ByMonth:
                        int startMonth = currentMonth - input.NumberRange + 1;
                        int startYear = currentYear;
                        if (startMonth <= 0)
                        {
                            startYear -= 1;
                            startMonth += 12;
                        }

                        for (int year = startYear; year <= currentYear; year++)
                        {
                            int endMonth = (year == currentYear) ? currentMonth : 12;

                            for (int index = startMonth; index <= endMonth; index++)
                            {
                                var query = _materialRepos.GetAll();
                                query = query.Where(x => x.CreationTime.Month == index && x.CreationTime.Year == year);
                                var materialList = await query.ToListAsync();

                                // Nhóm kết quả theo locationId
                                var groupedMaterial = materialList.GroupBy(m => m.LocationId);

                                Dictionary<string, MaterialStatisticsOutput> monthData = new Dictionary<string, MaterialStatisticsOutput>();
                                foreach (var group in groupedMaterial)
                                {
                                    int? locationId = (int?)group.Key;
                                    int materialCount = group.Count();
                                    int inventoryMaterialCount = group.Count(m => m.Status != "using");
                                    int unInventoryMaterialCount = materialCount - inventoryMaterialCount;
                                    // Lưu kết quả vào Dictionary
                                    string groupKey = locationId.HasValue ? locationId.Value.ToString() : "0";
                                    monthData.Add(groupKey, new MaterialStatisticsOutput
                                    {
                                        LocationId = locationId,
                                        MaterialCount = materialCount,
                                        InventoryMaterialCount = inventoryMaterialCount,
                                        UnInventoryMaterialCount = unInventoryMaterialCount

                                    });
                                }

                                string key = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index)} {year}";
                                dataResult.Add(key, monthData);
                            }

                            // Sau khi lặp qua tháng cuối cùng của năm hiện tại, đặt lại giá trị ban đầu của startMonth để xử lý các năm sau
                            startMonth = 1;
                        }
                        break;

                    case QueryCaseMaterial.ByYear:
                        break;
                }

                var data = DataResult.ResultSuccess(dataResult, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message, e);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> CreateListMaterialAsync(List<MaterialDto> input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var duplicateList = new List<MaterialDto>();
                var insertList = new List<MaterialDto>();
                if (input != null)
                {
                    var index = 0;
                    foreach (var obj in input)
                    {
                        index++;
                        obj.TenantId = AbpSession.TenantId;
                        //khong trung
                        if (obj.MaterialName != null)
                        {
                            var citizen = await _materialRepos
                                .FirstOrDefaultAsync(x => (x.TenantId == AbpSession.TenantId) 
                                && (x.MaterialCode == obj.MaterialCode) 
                                && (x.MaterialName.Trim() == obj.MaterialName.Trim()));
                            if (citizen == null)
                            {
                                var insertInput = obj.MapTo<Material>();
                                long id = await _materialRepos.InsertAndGetIdAsync(insertInput);
                                insertList.Add(insertInput.MapTo<MaterialDto>());
                            }
                            else
                            {
                                var citizenDupli = obj.MapTo<MaterialDto>();
                                duplicateList.Add(citizenDupli);
                            }
                        }
                    }

                }
                await CurrentUnitOfWork.SaveChangesAsync();
                mb.statisticMetris(t1, 0, "admin_islist_obj");
                var result = new { duplicateList, insertList };
                var data = DataResult.ResultSuccess(result, "Insert success !");
                return data;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> MaterialExcelExportAsync(MaterialExelExportInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    var queryInput = new QueryMaterialDto()
                    {
                        BuildingId = input.BuildingId,
                        CategoryType = input.CategoryType,
                        FormQuery = 1,
                        Keyword = input.Keyword,
                        UrbanId = input.UrbanId,
                        TypeId = input.TypeId,
                        Status = input.Status,
                        LocationId = input.LocationId,

                    };
                    var query = await QueryMaterialAsync(queryInput);

                    if (input.LocationId > 0)
                    {
                        query = query.Where(x => x.LocationId == input.LocationId);
                    }
                    else if (input.LocationId == 0)
                    {
                        query = query.Where(x => x.LocationId == null);
                    }

                    var result = await query.WhereIf(input.Ids.Count > 0, x => input.Ids.Contains(x.Id)).ToListAsync();
                    var list = _materialExcelExport.ExportToFile(result);
                    return DataResult.ResultSuccess(list, "Export Success");
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message, e);
                throw;
            }

        }
    }
}