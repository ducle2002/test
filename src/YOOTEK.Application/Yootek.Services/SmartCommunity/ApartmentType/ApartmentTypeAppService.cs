using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Service.Dto;
using Yootek.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services.ApartmentTypeService
{
    public interface IApartmentTypeAppService : IApplicationService
    {
        Task<DataResult> GetAllApartmentType(GetAllApartmentTypeInput input);
        Task<DataResult> CreateApartmentType(CreateApartmentTypeInput input);
        Task<DataResult> UpdateApartmentType(UpdateApartmentTypeInput input);
        Task<DataResult> DeleteApartmentType(long id);
        Task<DataResult> DeleteListApartmentTypes([FromBody] List<long> ids);
    }
    [AbpAuthorize]
    public class ApartmentTypeAppService : YootekAppServiceBase, IApplicationService
    {
        private readonly IRepository<ApartmentType, long> _apartmentTypeRepository;

        public ApartmentTypeAppService(IRepository<ApartmentType, long> apartmentTypeRepository)
        {
            _apartmentTypeRepository = apartmentTypeRepository;
        }
        public async Task<DataResult> GetAllApartmentType(GetAllApartmentTypeInput input)
        {
            try
            {
                IQueryable<ApartmentTypeDto> query = (from apartmentType in _apartmentTypeRepository.GetAll()
                                                      select new ApartmentTypeDto
                                                      {
                                                          Id = apartmentType.Id,
                                                          Name = apartmentType.Name,
                                                          Code = apartmentType.Code,
                                                          TenantId = apartmentType.TenantId
                                                      });
                List<ApartmentTypeDto> result = await query
                    .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Code)
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByApartmentType.CODE)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> CreateApartmentType(CreateApartmentTypeInput input)
        {
            try
            {
                ApartmentType? apartmentTypeOrg = await _apartmentTypeRepository.FirstOrDefaultAsync(x => x.Code == input.Code);
                if (apartmentTypeOrg != null) throw new UserFriendlyException(409, "ApartmentType is exist");
                ApartmentType apartmentTypeInsert = input.MapTo<ApartmentType>();
                apartmentTypeInsert.TenantId = AbpSession.TenantId;
                await _apartmentTypeRepository.InsertAsync(apartmentTypeInsert);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> UpdateApartmentType(UpdateApartmentTypeInput input)
        {
            try
            {
                ApartmentType? apartmentTypeOrg = await _apartmentTypeRepository.FirstOrDefaultAsync(x =>
                    x.Id == input.Id) ?? throw new UserFriendlyException("ApartmentType not found");
                ApartmentType? apartmentTypeTemp = await _apartmentTypeRepository.FirstOrDefaultAsync(x =>
                    x.Id != input.Id && x.Code == input.Code);
                if (apartmentTypeTemp != null) throw new UserFriendlyException("Code is existed");
                ObjectMapper.Map(input, apartmentTypeOrg);

                await _apartmentTypeRepository.UpdateAsync(apartmentTypeOrg);
                return DataResult.ResultSuccess(true, "Update success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteApartmentType(long id)
        {
            try
            {
                await _apartmentTypeRepository.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> DeleteListApartmentTypes([FromBody] List<long> ids)
        {
            try
            {
                await _apartmentTypeRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
