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
    public interface IERPSellerAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetListERPSellerInput input);
        Task<DataResult> GetByIdAsync(long id);
        Task<DataResult> CreateAsync(CreateERPSellerDto input);
        Task<DataResult> UpdateAsync(UpdateERPSellerDto input);
        Task<DataResult> DeleteAsync(long id);
    }
    public class ERPSellerAppService : YootekAppServiceBase, IERPSellerAppService
    {
        private readonly IRepository<ERPSeller, long> _eRPSellerRepository;
        public ERPSellerAppService(
             IRepository<ERPSeller, long> eRPSellerRepository
            )
        {
            _eRPSellerRepository = eRPSellerRepository;
        }
        public async Task<DataResult> GetAllAsync(GetListERPSellerInput input)
        {
            try
            {
                IQueryable<ERPSellerDto> query = (from eRPSeller in _eRPSellerRepository.GetAll()
                                                  select new ERPSellerDto
                                                  {
                                                      Id = eRPSeller.Id,
                                                      Title = eRPSeller.Title,
                                                      Phone = eRPSeller.Phone,
                                                      Email = eRPSeller.Email,
                                                      Types = eRPSeller.Types,
                                                      BusinessTypeId = eRPSeller.BusinessTypeId,
                                                      CurrencyUnitId = eRPSeller.CurrencyUnitId,
                                                      Address = eRPSeller.Address,
                                                      ProvinceCode = eRPSeller.ProvinceCode,
                                                      DistrictCode = eRPSeller.DistrictCode,
                                                      WardCode = eRPSeller.WardCode,
                                                      TenantId = eRPSeller.TenantId,
                                                  })
                    .ApplySearchFilter(input.Keyword, x => x.Title, x => x.Phone, x => x.Email)
                    ;
                List<ERPSellerDto> result = await query
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> CreateAsync(CreateERPSellerDto input)
        {
            try
            {
                ERPSeller eRPSeller = ObjectMapper.Map<ERPSeller>(input);
                eRPSeller.TenantId = AbpSession.TenantId;
                await _eRPSellerRepository.InsertAsync(eRPSeller);
                return DataResult.ResultSuccess(true, "Insert success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateAsync(UpdateERPSellerDto input)
        {
            try
            {
                ERPSeller updateData = await _eRPSellerRepository.FirstOrDefaultAsync(x => x.Id == input.Id)
                                         ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                   "ERPSeller not found!");
                ObjectMapper.Map(input, updateData);
                await _eRPSellerRepository.UpdateAsync(updateData);
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
                ERPSeller eRPSeller = await _eRPSellerRepository.FirstOrDefaultAsync(x => x.Id == id)
                                        ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                  "ERPSeller not found!");
                await _eRPSellerRepository.DeleteAsync(id);
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
                ERPSeller eRPSeller = await _eRPSellerRepository.FirstOrDefaultAsync(x => x.Id == id)
                                         ?? throw new UserFriendlyException((int)HttpStatusCode.NotFound,
                                   "ERPSeller not found!");
                ERPSellerDto eRPSellerDto = ObjectMapper.Map<ERPSellerDto>(eRPSeller);
                return DataResult.ResultSuccess(eRPSellerDto, "Get success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
