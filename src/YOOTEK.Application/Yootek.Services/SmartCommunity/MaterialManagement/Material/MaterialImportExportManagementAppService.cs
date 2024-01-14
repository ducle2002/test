using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Common.DataResult;
using System;
using System.Threading.Tasks;
using System.Linq;
using Abp.Linq.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Bibliography;
using Yootek.EntityDb;
using Yootek.SmartCommunity;
using Yootek.Organizations.OrganizationStructure;
using Microsoft.AspNetCore.Mvc;
using Yootek.Yootek.Services.Yootek.SmartCommunity.MaterialManagement.Material.ExcelExporter;

namespace Yootek.Services
{
    public interface IMaterialImportExportManagementAppService : IApplicationService
    {
        Task<object> GetAllAsync(GetAllInventoryImportExportInput input);
        Task<object> Create(InventoryImportExportDto input);
        Task<object> Update(InventoryImportExportDto input);
        Task<object> Delete(string id);

        Task<DataResult> DeleteMultiple([FromBody] List<string> ids);

    }

    [AbpAuthorize]
    public class MaterialImportExportManagementAppService : YootekAppServiceBase, IMaterialImportExportManagementAppService
    {

        private readonly IRepository<InventoryImportExport, long> _inventoryImportExportRepos;
        private readonly IRepository<Material, long> _materialRepos;
        private readonly IRepository<MaterialCategory, long> _materialCategoryRepos;
        private readonly IRepository<OrganizationStructureDept, long> _orgRepos;
        private readonly IMaterialExportImportExcelExporter _excelExporter;
        private readonly IRepository<Inventory, long> _inventoryRepos;

        public MaterialImportExportManagementAppService(
            IRepository<InventoryImportExport, long> inventoryImportExportRepos,
            IRepository<Material, long> materialRepos,
            IRepository<MaterialCategory, long> materialCategoryRepos,
            IRepository<OrganizationStructureDept, long> orgRepos,
            IMaterialExportImportExcelExporter excelExporter,
            IRepository<Inventory, long> inventoryRepos
            )
        {
            _inventoryImportExportRepos = inventoryImportExportRepos;
            _materialRepos = materialRepos;
            _materialCategoryRepos = materialCategoryRepos;
            _orgRepos = orgRepos;
            _excelExporter = excelExporter;
            _inventoryRepos = inventoryRepos;
        }

        public async Task<object> GetAllAsync(GetAllInventoryImportExportInput input)
        {
            try
            {
                var query = _inventoryImportExportRepos.GetAll()
                    .Where(x => x.IsImport == input.IsImport)
                    .WhereIf(input.IsApproved.HasValue, x => x.IsApproved == input.IsApproved)
                    .AsQueryable();

                var codes = query.Select(x => x.Code).Distinct();
                var resultCodes = await codes.PageBy(input).ToListAsync();

                var result = new List<InventoryImportExportDto>();
                foreach (var code in resultCodes)
                {
                    var materials = await (from p in query
                                           join pc in _materialRepos.GetAll() on p.MaterialId equals pc.Id into tb_pc
                                           from pc in tb_pc.DefaultIfEmpty()
                                           join c in _materialCategoryRepos.GetAll() on pc.UnitId equals c.Id into tb_c
                                           from c in tb_c.DefaultIfEmpty()
                                           join c1 in _materialCategoryRepos.GetAll() on p.FromInventoryId equals c1.Id into tb_c1
                                           from c1 in tb_c1.DefaultIfEmpty()
                                           select new ImportExportViewDto
                                           {
                                               TenantId = p.TenantId,
                                               FromInventoryId = p.FromInventoryId,
                                               ImportExportDate = p.ImportExportDate,
                                               MaterialId = p.MaterialId,
                                               StaffId = p.StaffId,
                                               ToInventoryId = p.ToInventoryId,
                                               Description = p.Description,
                                               Code = p.Code,
                                               IsImport = p.IsImport,
                                               TotalPrice = pc.Price > 0 ? p.Amount * pc.Price : 0,
                                               Key = p.Key,
                                               UnitName = c.Name,
                                               Id = p.MaterialId,
                                               Amount = p.Amount,
                                               InventoryName = c1.Name,
                                               MaterialCode = pc.MaterialCode,
                                               MaterialName = pc.MaterialName,
                                               Price = pc.Price,
                                               IsApproved = p.IsApproved,

                                           }).Where(x => x.Code == code).ToListAsync();
                    if (materials.Any())
                    {
                        var dv = materials[0].MapTo<InventoryImportExportDto>();
                        dv.Id = dv.Code;
                        dv.MaterialViews = materials;
                        dv.TotalAmount = materials.Sum(x => x.Amount);
                        dv.AllPrice = materials.Sum(x => x.TotalPrice);
                        dv.CountMaterial = materials.Count();
                        result.Add(dv);
                    }

                }



                var data = DataResult.ResultSuccess(result, "Get success!", query.Count());
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Create(InventoryImportExportDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                var code = "NXK-" + GetUniqueKey();
                var check = _inventoryImportExportRepos.FirstOrDefaultAsync(x => x.Code == code);
                if (check != null)
                {
                    code = code.Replace("NXK", GetUniqueKey(3));
                }
                input.Code = code;
                input.TenantId = AbpSession.TenantId;

                if (input.Materials != null)
                {
                    foreach (var item in input.Materials)
                    {
                        var insertInput = input.MapTo<InventoryImportExport>();
                        insertInput.Amount = item.Amount.Value;
                        insertInput.MaterialId = item.Id;
                        insertInput.Key = item.Key.Value;
                        insertInput.ImportExportDate = input.ImportExportDate;
                        long id = await _inventoryImportExportRepos.InsertAndGetIdAsync(insertInput);
                    }
                }

                //var insertInput = input.MapTo<InventoryImportExport>();
                //long id = await _inventoryImportExportRepos.InsertAndGetIdAsync(insertInput);
                var data = DataResult.ResultSuccess(input, "Insert success !");
                mb.statisticMetris(t1, 0, "is_inventoryIEX");

                return data;

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> Update(InventoryImportExportDto input)
        {
            try
            {

                long t1 = TimeUtils.GetNanoseconds();

                if(input.DeleteElements != null && input.DeleteElements.Count > 0)
                {
                    foreach(var item in input.DeleteElements)
                    {
                        await _inventoryImportExportRepos.DeleteAsync(x => x.Key == item);
                    }
                }

                if (input.UpdateElements != null && input.UpdateElements.Count > 0 && input.Materials != null)
                {
                    foreach (var item in input.UpdateElements)
                    {
                        var updateData = await _inventoryImportExportRepos.FirstOrDefaultAsync(x => x.Key == item);
                        var inputData = input.Materials.FirstOrDefault(x => x.Key == item);
                        if (updateData != null && inputData != null)
                        {
                            input.Materials.Remove(inputData);
                            updateData.Amount = inputData.Amount.Value;
                            updateData.MaterialId = inputData.Id;
                            updateData.Key = inputData.Key.Value;
                            updateData.Description = input.Description;
                            updateData.StaffId = input.StaffId;
                            updateData.ImportExportDate = input.ImportExportDate;
                            updateData.FromInventoryId = input.FromInventoryId;
                            updateData.ToInventoryId = input.ToInventoryId;

                            await _inventoryImportExportRepos.UpdateAsync(updateData);
                        }
                    }
                }

                if (input.AddElements != null&& input.AddElements.Count > 0 && input.Materials != null && input.Materials.Count > 0)
                {
                    foreach (var addKey in input.AddElements)
                    {
                        var item = input.Materials.FirstOrDefault(x => x.Key == addKey);
                        if (item == null) continue;
                        input.Id = null;
                        var insertInput = input.MapTo<InventoryImportExport>();
                        insertInput.Amount = item.Amount.Value;
                        insertInput.MaterialId = item.Id;
                        insertInput.Key = item.Key.Value;
                        long id = await _inventoryImportExportRepos.InsertAndGetIdAsync(insertInput);
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
                await _inventoryImportExportRepos.DeleteAsync(x => x.Code == id);
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
                    var tk = _inventoryImportExportRepos.DeleteAsync(x => x.Code == id);
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
        [Obsolete]
        public async Task<object> ApproveMaterialImportExport(ApproveMaterialImportExportInputDto input)
        {
            try
            {
                var exportInventory = await _inventoryImportExportRepos.GetAll().Where(x => x.Code == input.Id).ToListAsync();
                foreach(var card in exportInventory)
                {
                    if (card.FromInventoryId.HasValue)
                    {
                        var exportMaterial = await _inventoryRepos.FirstOrDefaultAsync(x => x.InventoryId == card.FromInventoryId.Value && x.MaterialId == card.MaterialId);
                        if (exportMaterial != null)
                        {
                            var inventoryExport = exportMaterial.MapTo<Inventory>();
                            if (exportMaterial.Amount >= card.Amount) inventoryExport.Amount -= card.Amount;
                            else throw new UserFriendlyException("Amount input is bigger than existing value!");
                            if (inventoryExport.Amount != 0) await _inventoryRepos.UpdateAsync(inventoryExport);
                            else await _inventoryRepos.DeleteAsync(inventoryExport.Id);
                        }
                    }
                    if (card.ToInventoryId.HasValue)
                    {
                        var importMaterial = await _inventoryRepos.FirstOrDefaultAsync(x => x.InventoryId == card.ToInventoryId.Value && x.MaterialId == card.MaterialId);
                        if (importMaterial != null)
                        {
                            var inventoryImport = new Inventory();
                            inventoryImport = importMaterial.MapTo<Inventory>();
                            inventoryImport.Amount += card.Amount;
                            await _inventoryRepos.UpdateAsync(inventoryImport);
                        }
                        else
                        {
                            var item = await _materialRepos.FirstOrDefaultAsync(card.MaterialId);
                            var inventoryImport = new Inventory()
                            {
                                Amount = card.Amount,
                                InventoryId = card.ToInventoryId.Value,
                                MaterialId = card.MaterialId,
                                Price = item.Price ?? 0,
                                TotalPrice = item.Price.HasValue ? item.Price * card.Amount : 0,
                                TenantId = AbpSession.TenantId,
                            };
                            await _inventoryRepos.InsertAsync(inventoryImport);
                        }
                    }
                    else throw new UserFriendlyException("No destination inventory");
                    card.IsApproved = true;
                    await _inventoryImportExportRepos.UpdateAsync(card);
                }
                
                
                var data = DataResult.ResultSuccess(exportInventory, "Delete success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                return data;
            }
        }
        [Obsolete]
        public async Task<object> ExportImportExportToExcelAsync(ExportInventoryImportExportInput input)
        {
            try
            {
                var query = _inventoryImportExportRepos.GetAll()
                    .Where(x => x.IsImport == input.IsImport)
                    .WhereIf(input.Ids != null && input.Ids.Count > 0, x => input.Ids.Contains(x.Code))
                    .AsQueryable();

                var codes = query.Select(x => x.Code).Distinct();
                var resultCodes = await codes.ToListAsync();

                var result = new List<InventoryImportExportDto>();
                foreach (var code in resultCodes)
                {
                    var materials = await (from p in query
                                           join pc in _materialRepos.GetAll() on p.MaterialId equals pc.Id into tb_pc
                                           from pc in tb_pc.DefaultIfEmpty()
                                           join c in _materialCategoryRepos.GetAll() on pc.UnitId equals c.Id into tb_c
                                           from c in tb_c.DefaultIfEmpty()
                                           join c1 in _materialCategoryRepos.GetAll() on p.FromInventoryId equals c1.Id into tb_c1
                                           from c1 in tb_c1.DefaultIfEmpty()
                                           select new ImportExportViewDto
                                           {
                                               TenantId = p.TenantId,
                                               FromInventoryId = p.FromInventoryId,
                                               ImportExportDate = p.ImportExportDate,
                                               MaterialId = p.MaterialId,
                                               StaffId = p.StaffId,
                                               ToInventoryId = p.ToInventoryId,
                                               Description = p.Description,
                                               Code = p.Code,
                                               IsImport = p.IsImport,
                                               TotalPrice = pc.Price > 0 ? p.Amount * pc.Price : 0,
                                               Key = p.Key,
                                               UnitName = c.Name,
                                               Id = p.MaterialId,
                                               Amount = p.Amount,
                                               InventoryName = c1.Name,
                                               MaterialCode = pc.MaterialCode,
                                               MaterialName = pc.MaterialName,
                                               Price = pc.Price

                                           }).Where(x => x.Code == code).ToListAsync();
                    if (materials.Any())
                    {
                        var dv = materials[0].MapTo<InventoryImportExportDto>();
                        dv.Id = dv.Code;
                        dv.MaterialViews = materials;
                        dv.TotalAmount = materials.Sum(x => x.Amount);
                        dv.AllPrice = materials.Sum(x => x.TotalPrice);
                        dv.CountMaterial = materials.Count();
                        result.Add(dv);
                    }

                }

                var excelList = _excelExporter.ExportToFile(result);

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
