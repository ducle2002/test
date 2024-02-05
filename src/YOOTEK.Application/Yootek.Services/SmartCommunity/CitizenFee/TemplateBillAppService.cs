using Abp.Application.Services;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.SmartCommunity.Phidichvu;
using Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee.Dto;
using Yootek.Organizations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.SmartCommunity.CitizenFee
{
    public interface ITemplateBillAppService : IApplicationService
    {
        Task<DataResult> GetAllTemplateBillAsync(GetAllTemplateBillInput input);
        DataResult GetTemplateOfTenant(GetTemplateOfTenantInput input);
        Task<DataResult> GetAllTemplateBillWithoutTenant(GetAllTemplateBillWithoutTenantInput input);
        Task<DataResult> GetTemplateBillByIdAsync(long id);
        Task<DataResult> CreateTemplateBill([FromForm] CreateTemplateBillInput input);
        Task<DataResult> UpdateTemplateBill([FromForm] UpdateTemplateBillInput input);
        Task<DataResult> DeleteTemplateBill(long id);
        Task<DataResult> DeleteManyTemplateBills([FromBody] List<long> ids);
        // helpers
        string GetContentOfTemplateBill(GetTemplateOfTenantInput input);
    }
    public class TemplateBillAppService : YootekAppServiceBase, ITemplateBillAppService
    {
        private readonly IRepository<TemplateBill, long> _templateBillRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnit;

        public TemplateBillAppService(IRepository<TemplateBill, long> templateBillRepository, IRepository<AppOrganizationUnit, long> organizationUnit)
        {
            _templateBillRepository = templateBillRepository;
            _organizationUnit = organizationUnit;
        }

        // template của tenant account
        public async Task<DataResult> GetAllTemplateBillAsync(GetAllTemplateBillInput input)
        {
            try
            {
                IQueryable<TemplateBillDto> query = (from templatebill in _templateBillRepository.GetAll()
                                                     select new TemplateBillDto
                                                     {
                                                         Id = templatebill.Id,
                                                         UrbanId = templatebill.UrbanId,
                                                         BuildingId = templatebill.BuildingId,
                                                         BuildingName = _organizationUnit.GetAll().Where(x => x.Id == templatebill.BuildingId).Select(x => x.DisplayName).FirstOrDefault(),
                                                         UrbanName = _organizationUnit.GetAll().Where(x => x.Id == templatebill.UrbanId).Select(x => x.DisplayName).FirstOrDefault(),
                                                         Name = templatebill.Name,
                                                         Content = templatebill.Content,
                                                         Type = templatebill.Type,
                                                         TenantId = templatebill.TenantId,
                                                         Code = templatebill.Code,
                                                     })
                                 .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                 .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                 .WhereIf(input.TenantId.HasValue, x => x.TenantId == input.TenantId)
                                 .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                                 .ApplySearchFilter(input.Keyword, x => x.Name, x=> x.Code);
                List<TemplateBillDto> result = await query.ApplySort(input.OrderBy, input.SortBy)
                                                .ApplySort(OrderByTemplateBill.NAME)
                                                .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        // template của 1 tenant bất kì
        public DataResult GetTemplateOfTenant(GetTemplateOfTenantInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    TemplateBill templateBill = _templateBillRepository.GetAll().FirstOrDefault(x => x.Type == input.Type);
                    return DataResult.ResultSuccess(ObjectMapper.Map<TemplateBillDto>(templateBill), "Get Success");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        [RemoteService(false)]
        public string GetContentOfTemplateBill(GetTemplateOfTenantInput input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                TemplateBill templateBill = _templateBillRepository.GetAll().FirstOrDefault(x => x.Type == input.Type);
                return templateBill?.Content ?? "";
            }
        }

        // tất cả các template của nhiều tenant khác nhau
        public async Task<DataResult> GetAllTemplateBillWithoutTenant(GetAllTemplateBillWithoutTenantInput input)
        {
            try
            {
                return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                    {
                        List<TemplateBillDto> result = _templateBillRepository.GetAll()
                            .Select(x => new TemplateBillDto()
                            {
                                Id = x.Id,
                                BuildingId = x.BuildingId,
                                UrbanId = x.UrbanId,
                                Name = x.Name,
                                Content = x.Content,
                                Type = x.Type,
                            })
                            .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                            .ToList();
                        return DataResult.ResultSuccess(result, "Get Success");
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetTemplateBillByIdAsync(long id)
        {
            try
            {
                TemplateBill templateBill = _templateBillRepository.FirstOrDefault(id);
                return DataResult.ResultSuccess(ObjectMapper.Map<TemplateBillDto>(templateBill), "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> CreateTemplateBill([FromForm] CreateTemplateBillInput input)
        {
            try
            {
                var result = _templateBillRepository.GetAll().Where(x => x.Code == input.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                if (result != null)
                {
                    throw new UserFriendlyException("Mã mẫu hóa đơn: " + input.Code + " đã tồn tại");
                }
                TemplateBill templateBill = new TemplateBill();
                templateBill.Code = input.Code;
                templateBill.BuildingId = input.BuildingId;
                templateBill.UrbanId = input.UrbanId;
                templateBill.Name = input.Name;
                templateBill.Type = input.Type;
                templateBill.TenantId = AbpSession.TenantId;
                if (input.FileHTML != null || input.FileHTML.Length != 0)
                {
                    using (var reader = new StreamReader(input.FileHTML.OpenReadStream()))
                    {
                        string content = await reader.ReadToEndAsync();
                        templateBill.Content = content;
                    }
                }                
                await _templateBillRepository.InsertAsync(templateBill);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        //public async Task<DataResult> CreateTemplateBill([FromForm] CreateTemplateBillInput input)
        //{
        //    try
        //    {
        //        var result = _templateBillRepository.GetAll().Where(x => x.Code == input.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
        //        if (result != null)
        //        {
        //            throw new UserFriendlyException("Mã mẫu hóa đơn: " + input.Code + " đã tồn tại");
        //        }
        //        return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
        //        {
        //            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
        //            {
        //                TemplateBill templateBill = ObjectMapper.Map<TemplateBill>(input);
        //                if (input.FileHTML != null || input.FileHTML.Length != 0)
        //                {
        //                    using (var reader = new StreamReader(input.FileHTML.OpenReadStream()))
        //                    {
        //                        string content = await reader.ReadToEndAsync();
        //                        templateBill.Content = content;
        //                    }
        //                }
        //                if(!templateBill.TenantId.HasValue)
        //                {
        //                    templateBill.TenantId = AbpSession.TenantId;
        //                }    
        //                await _templateBillRepository.InsertAsync(templateBill);
        //                return DataResult.ResultSuccess(true, "Insert success!");
        //            }
        //        });
        //    }
        //    catch (Exception e)
        //    {
        //        Logger.Fatal(e.Message);
        //        throw;
        //    }
        //}

        [HttpPost]
        public async Task<DataResult> UpdateTemplateBill([FromForm] UpdateTemplateBillInput input)
        {
            try
            {
                TemplateBill? templateBillOrg = await _templateBillRepository.FirstOrDefaultAsync(input.Id)
                         ?? throw new UserFriendlyException("TemplateBill not found!");

                if(templateBillOrg!=null)
                {
                    if (templateBillOrg.Code != input.Code)
                    {
                        var result = _templateBillRepository.GetAll().Where(x => x.Code == input.Code && x.TenantId == AbpSession.TenantId).FirstOrDefault();
                        if (result != null)
                        {
                            throw new UserFriendlyException("Mã mẫu hóa đơn : " + input.Code + " đã tồn tại");
                        }
                    }
                }    
                ObjectMapper.Map(input, templateBillOrg);

                if (input.FileHTML != null || input.FileHTML.Length != 0)
                {
                    using (var reader = new StreamReader(input.FileHTML.OpenReadStream()))
                    {
                        string content = await reader.ReadToEndAsync();
                        templateBillOrg.Content = content;
                    }
                }
                templateBillOrg.TenantId = AbpSession.TenantId;
                await _templateBillRepository.UpdateAsync(templateBillOrg);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteTemplateBill(long id)
        {
            try
            {
                return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                    {
                        await _templateBillRepository.DeleteAsync(id);
                        return DataResult.ResultSuccess("Delete success!");
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteManyTemplateBills([FromBody] List<long> ids)
        {
            try
            {
                return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
                    {
                        await _templateBillRepository.DeleteAsync(x => ids.Contains(x.Id));
                        return DataResult.ResultSuccess("Delete success!");
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
