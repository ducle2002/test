using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Common.DataResult;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Yootek.EntityDb;
using Yootek.SmartCommunity;
using Abp.Collections.Extensions;
using Abp.Linq.Extensions;
using System.Collections.Generic;
using Yootek.Organizations.OrganizationStructure;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Yootek.Yootek.Services.Yootek.SmartCommunity.MaterialManagement.Material.ExcelExporter;

namespace Yootek.Services
{
    public interface IMaterialDeliveryManagementAppService : IApplicationService
    {
        Task<object> GetAllAsync(GetAllDeliveryInput input);
        Task<object> Create(MaterialDeliveryDto input);
        Task<object> Update(MaterialDeliveryDto input);
        Task<object> Delete(string id);
        Task<DataResult> DeleteMultiple([FromBody] List<string> ids);

    }

    [AbpAuthorize]
    public class MaterialDeliveryManagementAppService : YootekAppServiceBase, IMaterialDeliveryManagementAppService
    {

     
        private readonly IRepository<MaterialDelivery, long> _materialDeliveryRepos;
        private readonly IRepository<Material, long> _materialRepos;
        private readonly IRepository<MaterialCategory, long> _materialCategoryRepos;
        private readonly IRepository<OrganizationStructureDept, long> _orgRepos;
        private readonly IMaterialDeliveryExcelExporter _excelExporter;
        private readonly IRepository<Inventory, long> _inventoryRepos;

        public MaterialDeliveryManagementAppService(
         
            IRepository<MaterialDelivery, long> materialDeliveryRepos,
            IRepository<Material, long> materialRepos,
            IRepository<MaterialCategory, long> materialCategoryRepos,
            IRepository<OrganizationStructureDept, long> orgRepos,
            IRepository<Inventory, long> inventoryRepos,
            IMaterialDeliveryExcelExporter excelExporter
            )
        {
            _materialDeliveryRepos = materialDeliveryRepos;
            _materialRepos = materialRepos;
            _materialCategoryRepos = materialCategoryRepos;
            _orgRepos = orgRepos;
            _inventoryRepos = inventoryRepos;
            _excelExporter = excelExporter;
        }

        public async Task<object> GetAllAsync(GetAllDeliveryInput input)
        {
            try
            {
                var query = _materialDeliveryRepos.GetAll()
                    .Where(x => x.IsDelivery == input.IsDelivery)
                    .WhereIf(input.StaffId.HasValue, x => x.DeliverStaffId1 == input.StaffId ||
                              x.DeliverStaffId2 == input.StaffId || x.DeliverStaffId3 == input.StaffId || x.ReceiverStaffId1 == input.StaffId ||
                               x.ReceiverStaffId2 == input.StaffId || x.ReceiverStaffId3 == input.StaffId).AsQueryable();

                var codes  = query.Select(x => x.Code).Distinct();
                var resultCodes = await codes.PageBy(input).ToListAsync();

                var result = new List<MaterialDeliveryDto>();
                foreach( var code in resultCodes )
                {
                    var materials = await (from p in query
                                     join pc in _materialRepos.GetAll() on p.MaterialId equals pc.Id into tb_pc
                                     from pc in tb_pc.DefaultIfEmpty()
                                     join c in _materialCategoryRepos.GetAll() on pc.UnitId equals c.Id into tb_c
                                     from c in tb_c.DefaultIfEmpty()
                                     join ou in _orgRepos.GetAll() on p.ReceiverDepartmentId equals ou.Id into tb_o
                                     from ou in tb_o.DefaultIfEmpty()
                                     select new MaterialDeliveryViewDto
                                     {
                                         TenantId = p.TenantId,
                                         State = p.State,
                                         DeliverStaffId1 = p.DeliverStaffId1,
                                         ReceiverStaffId1 = p.ReceiverStaffId1,
                                         DeliverStaffId2 = p.DeliverStaffId2,
                                         ReceiverStaffId2 = p.ReceiverStaffId2,
                                         DeliverStaffId3 = p.DeliverStaffId3,
                                         ReceiverStaffId3 = p.ReceiverStaffId3,
                                         DeliverDepartmentId = p.DeliverDepartmentId,
                                         ReceiverDepartmentId = p.ReceiverDepartmentId,
                                         UrbanId = p.UrbanId,
                                         BuildingId = p.BuildingId,
                                         ReceptionDate = p.ReceptionDate,
                                         DeliveryDate = p.DeliveryDate,
                                         Description = p.Description,
                                         Code = p.Code,
                                         IsDelivery = p.IsDelivery,
                                         TotalPrice = pc.Price > 0 ? p.Amount * pc.Price : 0,
                                         Key = p.Key,
                                         UnitName = c.Name,
                                         Id = p.MaterialId,
                                         Amount = p.Amount,
                                         DepartmentName = ou.DisplayName,
                                         MaterialCode = pc.MaterialCode,
                                         MaterialName = pc.MaterialName,
                                         Price = pc.Price,
                                         ApartmentCode = p.ApartmentCode,
                                         CustomerName = p.CustomerName,
                                         RecipientType = p.RecipientType,
                                         FromInventoryId = p.FromInventoryId,
                                         StaffId = p.StaffId
                                     }).Where(x => x.Code == code).ToListAsync();
                    if(materials.Any())
                    {
                        var dv = materials[0].MapTo<MaterialDeliveryDto>();
                        dv.Id = dv.Code;
                        dv.MaterialViews = materials;
                        dv.TotalAmount = materials.Sum(x => x.Amount);
                        dv.AllPrice = materials.Sum(x => x.TotalPrice);
                        dv.CountMaterial = materials.Count();
                        dv.CustomerName = materials[0].CustomerName;
                        dv.DeliverDepartmentId = materials[0].DeliverDepartmentId;
                        dv.DeliverStaffId1 = materials[0].DeliverStaffId1;
                        dv.DeliverStaffId2 = materials[0].DeliverStaffId2;
                        dv.DeliverStaffId3 = materials[0].DeliverStaffId3;
                        dv.ReceiverDepartmentId = materials[0].ReceiverDepartmentId;
                        dv.ReceiverStaffId1 = materials[0].ReceiverStaffId1;
                        dv.ReceiverStaffId2 = materials[0].ReceiverStaffId2;
                        dv.ReceiverStaffId3 = materials[0].ReceiverStaffId3;
                        dv.DeliveryDate = materials[0].DeliveryDate;
                        dv.Description = materials[0].Description;
                        dv.FromInventoryId = materials[0].FromInventoryId;
                        dv.RecipientType = materials[0].RecipientType;
                        dv.ApartmentCode = materials[0].ApartmentCode;
                        
                        result.Add(dv);
                    }

                }        

                var data = DataResult.ResultSuccess(result, "Get success!", codes.Count());
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [Obsolete]
        public async Task<object> Create(MaterialDeliveryDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var code = "GN-" + GetUniqueKey();
                var check = await _materialDeliveryRepos.FirstOrDefaultAsync(x => x.Code == code);
                if(check != null)
                {
                    code = code.Replace("GN",GetUniqueKey(2));
                }
                input.Code = code;
                input.TenantId = AbpSession.TenantId;

                if(input.Materials != null)
                {
                    foreach(var item in input.Materials)
                    {
                        var insertInput = input.MapTo<MaterialDelivery>();
                        insertInput.Amount = item.Amount;
                        insertInput.MaterialId = item.Id;
                        insertInput.Key = item.Key.Value;
                        long id = await _materialDeliveryRepos.InsertAndGetIdAsync(insertInput);
                    }
                }

             
                var data = DataResult.ResultSuccess(input, "Insert success !");
                mb.statisticMetris(t1, 0, "is_delivery");

                return data;

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        public async Task<object> Update(MaterialDeliveryDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                if (input.DeleteElements != null && input.DeleteElements.Count > 0)
                {
                    foreach (var item in input.DeleteElements)
                    {
                        await _materialDeliveryRepos.DeleteAsync(x => x.Key == item);
                    }
                }

                if (input.UpdateElements != null && input.UpdateElements.Count > 0 && input.Materials != null)
                {
                    foreach (var item in input.UpdateElements)
                    {
                        var updateData = await _materialDeliveryRepos.FirstOrDefaultAsync(x => x.Key == item);
                        var inputData = input.Materials.FirstOrDefault(x => x.Key == item);
                        if (updateData != null && inputData != null)
                        {
                            input.Materials.Remove(inputData);
                            updateData.Amount = inputData.Amount.Value;
                            updateData.MaterialId = inputData.Id;
                            updateData.Key = inputData.Key.Value;
                            updateData.Description = input.Description;
                            updateData.StaffId = input.StaffId;
                            updateData.DeliveryDate = input.DeliveryDate;
                            updateData.FromInventoryId = input.FromInventoryId;
                            updateData.DeliverStaffId1 = input.DeliverStaffId1;
                            updateData.DeliverStaffId2 = input.DeliverStaffId2;
                            updateData.DeliverStaffId3 = input.DeliverStaffId3;
                            updateData.ReceiverStaffId1 = input.ReceiverStaffId1;
                            updateData.ReceiverStaffId2 = input.ReceiverStaffId2;
                            updateData.ReceiverStaffId3 = input.ReceiverStaffId3;


                            await _materialDeliveryRepos.UpdateAsync(updateData);
                        }
                    }
                }

                if (input.AddElements != null && input.AddElements.Count > 0 && input.Materials != null && input.Materials.Count > 0)
                {
                    foreach (var addKey in input.AddElements)
                    {
                        var item = input.Materials.FirstOrDefault(x => x.Key == addKey);
                        if (item == null) continue;
                        input.Id = null;
                        var insertInput = input.MapTo<MaterialDelivery>();
                        insertInput.Amount = item.Amount.Value;
                        insertInput.MaterialId = item.Id;
                        insertInput.Key = item.Key.Value;
                        long id = await _materialDeliveryRepos.InsertAndGetIdAsync(insertInput);
                    }
                }
                mb.statisticMetris(t1, 0, "Ud_inventoryIEX");

                var data = DataResult.ResultSuccess(input, "Update success !");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Delete(string id)
        {
            try
            {
                await _materialDeliveryRepos.DeleteAsync(x => x.Code == id);
                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteMultiple([FromBody] List<string> ids)
        {
            try
            {

                if (ids.Count == 0) return DataResult.ResultError("Err", "input empty");
                var tasks = new List<Task>();
                foreach (var id in ids)
                {
                    var tk = _materialDeliveryRepos.DeleteAsync(x => x.Code == id);
                    tasks.Add(tk);
                }
                Task.WaitAll(tasks.ToArray());

                var data = DataResult.ResultSuccess("Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return data;
            }

        }
        public async Task<object> GetDeliveryStatisticsAsync(MaterialStatisticsDto input)
        {
            try
            {
                DateTime now = DateTime.Now;
                int currentMonth = now.Month;
                int currentYear = now.Year;

                Dictionary<string, (int sendCount, int receiveCount)> dataResult = new Dictionary<string, (int, int)>();

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
                                var query = _materialDeliveryRepos.GetAll();
                                query = query.Where(x => x.CreationTime.Month == index && x.CreationTime.Year == year);

                                // Tổng số phiếu giao theo tháng
                                int sendCount = await query.CountAsync(x => x.IsDelivery);

                                // Tổng số phiếu nhận theo tháng
                                int receiveCount = await query.CountAsync(x => !x.IsDelivery);

                                string key = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index)} {year}";
                                dataResult.Add(key, (sendCount, receiveCount));
                            }

                            // đến tháng cuối cùng của năm hiện tại
                            // đặt lại giá trị ban đầu của startMonth để xử lý các năm sau
                            startMonth = 1;
                        }
                        break;

                    case QueryCaseMaterial.ByYear:
                        break;
                }

                var data = DataResult.ResultSuccess(dataResult.ToDictionary(kvp => kvp.Key, kvp => new { receiveCount = kvp.Value.receiveCount, sendCount = kvp.Value.sendCount }), "Get success!");
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
        public async Task<object> ApproveDelivery(ApproveDeliveryInputDto input)
        {
            try
            {
                var delivery = await _materialDeliveryRepos.GetAll().Where(x => x.Code == input.Id).ToListAsync();
                foreach(var d in delivery)
                {
                    if (d.FromInventoryId.HasValue)
                    {
                        var exportMaterial = await _inventoryRepos.FirstOrDefaultAsync(x => x.InventoryId == d.FromInventoryId.Value && x.MaterialId == d.MaterialId);
                        if (exportMaterial != null)
                        {
                            var inventoryExport = exportMaterial.MapTo<Inventory>();
                            if (exportMaterial.Amount >= d.Amount) inventoryExport.Amount -= d.Amount;
                            else throw new UserFriendlyException("Amount input is bigger than existing value!");
                            if (inventoryExport.Amount != 0) await _inventoryRepos.UpdateAsync(inventoryExport);
                            else await _inventoryRepos.DeleteAsync(inventoryExport.Id);
                        }
                    }
                    d.State = DeliveryState.APPROVED;
                    await _materialDeliveryRepos.UpdateAsync(d);
                }
                var data = DataResult.ResultSuccess(delivery, "Delete success!");
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
        public async Task<object> ExportMaterialDeliveryToExcel(InventoryDeliveryExportInput input)
        {
            try
            {
                var query = _materialDeliveryRepos.GetAll()
                    .Where(x => x.IsDelivery == input.IsDelivery);
                var codes = query.Select(x => x.Code).Distinct();
                var resultCodes = await codes.ToListAsync();

                var result = new List<MaterialDeliveryDto>();
                foreach (var code in resultCodes)
                {
                    var materials = await (from p in query
                                           join pc in _materialRepos.GetAll() on p.MaterialId equals pc.Id into tb_pc
                                           from pc in tb_pc.DefaultIfEmpty()
                                           join c in _materialCategoryRepos.GetAll() on pc.UnitId equals c.Id into tb_c
                                           from c in tb_c.DefaultIfEmpty()
                                           join ou in _orgRepos.GetAll() on p.ReceiverDepartmentId equals ou.Id into tb_o
                                           from ou in tb_o.DefaultIfEmpty()
                                           select new MaterialDeliveryViewDto
                                           {
                                               TenantId = p.TenantId,
                                               State = p.State,
                                               DeliverStaffId1 = p.DeliverStaffId1,
                                               ReceiverStaffId1 = p.ReceiverStaffId1,
                                               DeliverStaffId2 = p.DeliverStaffId2,
                                               ReceiverStaffId2 = p.ReceiverStaffId2,
                                               DeliverStaffId3 = p.DeliverStaffId3,
                                               ReceiverStaffId3 = p.ReceiverStaffId3,
                                               DeliverDepartmentId = p.DeliverDepartmentId,
                                               ReceiverDepartmentId = p.ReceiverDepartmentId,
                                               UrbanId = p.UrbanId,
                                               BuildingId = p.BuildingId,
                                               ReceptionDate = p.ReceptionDate,
                                               DeliveryDate = p.DeliveryDate,
                                               Description = p.Description,
                                               Code = p.Code,
                                               IsDelivery = p.IsDelivery,
                                               TotalPrice = pc.Price > 0 ? p.Amount * pc.Price : 0,
                                               Key = p.Key,
                                               UnitName = c.Name,
                                               Id = p.MaterialId,
                                               Amount = p.Amount,
                                               DepartmentName = ou.DisplayName,
                                               MaterialCode = pc.MaterialCode,
                                               MaterialName = pc.MaterialName,
                                               Price = pc.Price

                                           }).Where(x => x.Code == code).ToListAsync();
                    if (materials.Any())
                    {
                        var dv = materials[0].MapTo<MaterialDeliveryDto>();
                        dv.Id = dv.Code;
                        dv.MaterialViews = materials;
                        dv.TotalAmount = materials.Sum(x => x.Amount);
                        dv.AllPrice = materials.Sum(x => x.TotalPrice);
                        dv.CountMaterial = materials.Count();
                        result.Add(dv);
                    }
                }

                var excelList = _excelExporter.ExportToFile(result, input.IsDelivery);

                var data = DataResult.ResultSuccess(excelList, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


    }


}
