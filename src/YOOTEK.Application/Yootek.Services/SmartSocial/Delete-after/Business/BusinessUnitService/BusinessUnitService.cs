using Abp;
using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Yootek.Common.DataResult;
using Yootek.Common.Enum.RegBusinessEnums;
using Yootek.Yootek.EntityDb.Yootek.DichVu.BusinessReg;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Yootek.Services.Yootek.DichVu.Business.BusinessRegisterService
{
    public interface IBusinessUnitService : IApplicationService
    {
        Task<object> CreateOrUpdateBusinessUnitAsync(BusinessUnitDto input);
        Task<object> DeleteBusinessUnitAsync(long id);
        Task<object> GetBusinessUnitAsync(FindBusinessUnit input);
        Task<object> CreateListAsync(List<BusinessUnitDto> input);
    }
    public class BusinessUnitService : YootekAppServiceBase, IBusinessUnitService
    {
        private readonly IRepository<BusinessUnit, long> _businessUnitRepo;
        public BusinessUnitService(IRepository<BusinessUnit, long> businessRegisterRepo)
        {
            _businessUnitRepo = businessRegisterRepo;
        }

      
        public async Task<object> CreateOrUpdateBusinessUnitAsync(BusinessUnitDto input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    var updateData = await _businessUnitRepo.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _businessUnitRepo.UpdateAsync(updateData);
                    }
                    var data = DataResult.ResultSuccess(updateData, "Updated successfully!");
                    return data;
                }
                else
                {
                    var inputData = input.MapTo<BusinessUnit>();
                    var data = await _businessUnitRepo.InsertAsync(inputData);
                    var result = DataResult.ResultSuccess(inputData, "Inserted sucessfully!");
                    return result;
                }
            }
            catch (Exception e)
            {
                var result = DataResult.ResultError(e.Message, "Failed!");
                Logger.Fatal(e.StackTrace);
                return result;
            }
        }

        public async Task<object> DeleteBusinessUnitAsync(long id)
        {
            try
            {
                await _businessUnitRepo.DeleteAsync(id);
                var data = DataResult.ResultSuccess("Deleted!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.Message, "Failed!");
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        protected IQueryable<BusinessUnitDto> GetAllUnits()
        {
            var query = (from reg in _businessUnitRepo.GetAll()
                         select new BusinessUnitDto
                         {
                             Id = reg.Id,
                             BusinessName = reg.BusinessName,
                             BusinessAddress = reg.BusinessAddress,
                             BusinessEmail = reg.BusinessEmail,
                             BusinessOwnerName = reg.BusinessOwnerName,
                             BusinessPhone = reg.BusinessPhone,
                             BusinessWebsite = reg.BusinessWebsite,
                             BusinessDesc = reg.BusinessDesc,
                             BusinessIMG = reg.BusinessIMG,
                             BusinessType = reg.BusinessType,
                             DistrictId = reg.DistrictId,
                             ProvinceId = reg.ProvinceId,
                             WardId = reg.WardId,
                             CreationTime = reg.CreationTime,
                             CreatorUserId = reg.CreatorUserId,
                             DeleterUserId = reg.DeleterUserId,
                             DeletionTime = reg.DeletionTime,
                             IsDeleted = reg.IsDeleted,
                             LastModificationTime = reg.LastModificationTime,
                             LastModifierUserId = reg.LastModifierUserId,
                             OrganizationUnitId = reg.OrganizationUnitId,
                             TenantId = reg.TenantId
                         }).AsQueryable();
            return query;
        }

        protected IQueryable<BusinessUnitDto> QueryFormId(IQueryable<BusinessUnitDto> query, FindBusinessUnit input, int? FormId)
        {
            switch (FormId)
            {
                case (int)BusinessUnitEnums.FORM_ID_OBJECTS.FORM_ADMIN_GET_OBJECT_GETALL:
                    query = query.OrderBy(x => x.CreationTime);
                    break;
                case (int)BusinessUnitEnums.FORM_ID_OBJECTS.FORM_PARTNER_OBJECT_GETALL:
                    query = query.Where(x => x.CreatorUserId == input.CreatorUserId).OrderBy(x => x.CreationTime);
                    break;
                case (int)BusinessUnitEnums.FORM_ID_OBJECTS.FORM_USER_OBJECT_GETALL:
                    query = query.Where(x => x.CreatorUserId == AbpSession.UserId).OrderBy(x => x.CreationTime);
                    break;
                case (int)BusinessUnitEnums.FORM_ID_OBJECTS.FORM_SEARCHING_OBJECT:
                    query = query.Where(x => x.BusinessName.Contains(input.Keyword)).OrderBy(x => x.CreationTime);
                    break;
                case (int)BusinessUnitEnums.FORM_ID_OBJECTS.FORM_GET_ALL_UNIT_ID_OBJECT:
                    query = query.Where(x => x.OrganizationUnitId == input.OrganizationUnitId).OrderBy(x => x.CreationTime);
                    break;
                case (int)BusinessUnitEnums.FORM_ID_OBJECTS.FORM_SEARCHING_UNIT_ID_OBJECT:
                    query = query.Where(x => x.BusinessName.Contains(input.Keyword) && x.OrganizationUnitId == input.OrganizationUnitId).OrderBy(x => x.CreationTime);
                    break;
            }
            return query;
        }

        public async Task<object> GetBusinessUnitAsync(FindBusinessUnit input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(AbpSession.TenantId))
                {
                    long t1 = TimeUtils.GetNanoseconds();
                    var obj = new Object();
                    var query = QueryFormId(GetAllUnits(), input, input.FormId);
                    var list = await query.Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                    var count = query.Count();
                    obj = list;
                    //var list = query.ToList();
                    var data = DataResult.ResultSuccess(obj, "Success", count);
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.Message, "Error");
                Logger.Error(e.Message);
                return data;
            }
        }

      
        public async Task<object> CreateListAsync(List<BusinessUnitDto> input)
        {
            try
            {
                ConcurrentDictionary<int, Task<long>> concurrentTasks = new ConcurrentDictionary<int, Task<long>>();
                ConcurrentDictionary<int, Task<long>> taskRates = new ConcurrentDictionary<int, Task<long>>();
                long t1 = TimeUtils.GetNanoseconds();
                if (input != null)
                {
                    var index = 0;
                    foreach (var obj in input)
                    {
                        index++;
                        if ((obj.BusinessName != null) && obj.BusinessType > 0 && obj.BusinessAddress != null && obj.BusinessOwnerName != null)
                        {
                            var userInput = obj.MapTo<BusinessUnitDto>();
                            userInput.TenantId = AbpSession.TenantId;
                            await _businessUnitRepo.InsertAsync(userInput);
                        }
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
                mb.statisticMetris(t1, 0, "admin_islist_obj");
                var data = DataResult.ResultSuccess("Insert success !");
                return data;
            }
            catch (Exception e)
            {
                throw new AbpException(e.Message);
            }
        }
    }
}
