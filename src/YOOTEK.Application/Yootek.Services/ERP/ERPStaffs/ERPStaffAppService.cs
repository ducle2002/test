using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;
using System.Net;
namespace Yootek.Services
{
    public interface IERPStaffAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetListERPStaffInput input);
        Task<DataResult> GetByIdAsync(long id);
        Task<DataResult> CreateAsync(CreateERPStaffDto input);
        Task<DataResult> UpdateAsync(UpdateERPStaffDto input);
        Task<DataResult> DeleteAsync(long id);
    }
    public class ERPStaffAppService : YootekAppServiceBase, IERPStaffAppService
    {
        private readonly IRepository<ERPStaff, long> _eRPStaffRepository;
        public ERPStaffAppService(
             IRepository<ERPStaff, long> eRPStaffRepository
            )
        {
            _eRPStaffRepository = eRPStaffRepository;
        }
        public async Task<DataResult> GetAllAsync(GetListERPStaffInput input)
        {
            try
            {
                IQueryable<ERPStaffDto> query = (from eRPStaff in _eRPStaffRepository.GetAll()
                                                 select new ERPStaffDto
                                                 {
                                                     Id = eRPStaff.Id,
                                                     Fullname = eRPStaff.Fullname,
                                                     Phone = eRPStaff.Phone,
                                                     SellerId = eRPStaff.SellerId,
                                                     Notes = eRPStaff.Notes,
                                                     TenantId = eRPStaff.TenantId,
                                                 })
                    .ApplySearchFilter(input.Keyword, x => x.Fullname, x => x.Phone)
                    ;
                List<ERPStaffDto> result = await query
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> CreateAsync(CreateERPStaffDto input)
        {
            try
            {
                ERPStaff eRPStaff = ObjectMapper.Map<ERPStaff>(input);
                eRPStaff.TenantId = AbpSession.TenantId;
                await _eRPStaffRepository.InsertAsync(eRPStaff);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateAsync(UpdateERPStaffDto input)
        {
            try
            {
                ERPStaff updateData = await _eRPStaffRepository.FirstOrDefaultAsync(x => x.Id == input.Id)
                                         ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                   "ERPStaff not found!");
                ObjectMapper.Map(input, updateData);
                await _eRPStaffRepository.UpdateAsync(updateData);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteAsync(long id)
        {
            try
            {
                ERPStaff eRPStaff = await _eRPStaffRepository.FirstOrDefaultAsync(x => x.Id == id)
                                        ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                  "ERPStaff not found!");
                await _eRPStaffRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetByIdAsync(long id)
        {
            try
            {
                ERPStaff eRPStaff = await _eRPStaffRepository.FirstOrDefaultAsync(x => x.Id == id)
                                         ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                   "ERPStaff not found!");
                ERPStaffDto eRPStaffDto = ObjectMapper.Map<ERPStaffDto>(eRPStaff);
                return DataResult.ResultSuccess(eRPStaffDto, "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
