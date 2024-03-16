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
    public interface IERPBranchAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetListERPBranchInput input);
        Task<DataResult> GetByIdAsync(long id);
        Task<DataResult> CreateAsync(CreateERPBranchDto input);
        Task<DataResult> UpdateAsync(UpdateERPBranchDto input);
        Task<DataResult> DeleteAsync(long id);
    }
    public class ERPBranchAppService : YootekAppServiceBase, IERPBranchAppService
    {
        private readonly IRepository<ERPBranch, long> _eRPBranchRepository;
        public ERPBranchAppService(
             IRepository<ERPBranch, long> eRPBranchRepository
            )
        {
            _eRPBranchRepository = eRPBranchRepository;
        }
        public async Task<DataResult> GetAllAsync(GetListERPBranchInput input)
        {
            try
            {
                IQueryable<ERPBranchDto> query = (from eRPBranch in _eRPBranchRepository.GetAll()
                                                  select new ERPBranchDto
                                                  {
                                                      Id = eRPBranch.Id,
                                                      IsDefault = eRPBranch.IsDefault,
                                                      Title = eRPBranch.Title,
                                                      Phone = eRPBranch.Phone,
                                                      Code = eRPBranch.Code,
                                                      SellerId = eRPBranch.SellerId,
                                                      ZipCode = eRPBranch.ZipCode,
                                                      Address = eRPBranch.Address,
                                                      ProvinceCode = eRPBranch.ProvinceCode,
                                                      DistrictCode = eRPBranch.DistrictCode,
                                                      WardCode = eRPBranch.WardCode,
                                                      TenantId = eRPBranch.TenantId,
                                                  })
                    .ApplySearchFilter(input.Keyword, x => x.Title, x => x.Phone, x => x.Code)
                    ;
                List<ERPBranchDto> result = await query
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> CreateAsync(CreateERPBranchDto input)
        {
            try
            {
                ERPBranch eRPBranch = ObjectMapper.Map<ERPBranch>(input);
                eRPBranch.TenantId = AbpSession.TenantId;
                await _eRPBranchRepository.InsertAsync(eRPBranch);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateAsync(UpdateERPBranchDto input)
        {
            try
            {
                ERPBranch updateData = await _eRPBranchRepository.FirstOrDefaultAsync(x => x.Id == input.Id)
                                         ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                   "ERPBranch not found!");
                ObjectMapper.Map(input, updateData);
                await _eRPBranchRepository.UpdateAsync(updateData);
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
                ERPBranch eRPBranch = await _eRPBranchRepository.FirstOrDefaultAsync(x => x.Id == id)
                                        ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                  "ERPBranch not found!");
                await _eRPBranchRepository.DeleteAsync(id);
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
                ERPBranch eRPBranch = await _eRPBranchRepository.FirstOrDefaultAsync(x => x.Id == id)
                                         ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                   "ERPBranch not found!");
                ERPBranchDto eRPBranchDto = ObjectMapper.Map<ERPBranchDto>(eRPBranch);
                return DataResult.ResultSuccess(eRPBranchDto, "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
