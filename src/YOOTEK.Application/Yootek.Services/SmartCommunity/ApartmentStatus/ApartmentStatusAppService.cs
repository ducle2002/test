using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.UI;
using Yootek.Application;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Yootek.Services
{
    public interface IApartmentStatusAppService : IApplicationService
    {
        DataResult GetAllApartmentStatus(GetAllApartmentStatusInput input);
        Task<DataResult> GetApartmentStatusById(long id);
        Task<DataResult> CreateApartmentStatus(CreateApartmentStatusInput input);
        Task<DataResult> UpdateApartmentStatus(UpdateApartmentStatusInput input);
        Task<DataResult> DeleteApartmentStatus(long id);
        Task<DataResult> DeleteManyApartmentStatus([FromBody] List<long> ids);
    }
    [AbpAuthorize]
    public class ApartmentStatusAppService : YootekAppServiceBase, IApartmentStatusAppService
    {
        private readonly IRepository<ApartmentStatus, long> _apartmentStatusRepos;
        public ApartmentStatusAppService(IRepository<ApartmentStatus, long> apartmentStateRepos)
        {
            _apartmentStatusRepos = apartmentStateRepos;
        }

        public DataResult GetAllApartmentStatus(GetAllApartmentStatusInput input)
        {
            try
            {
                IQueryable<ApartmentStatusDto> query = (from apartmentStatus in _apartmentStatusRepos.GetAll()
                                                        select new ApartmentStatusDto
                                                        {
                                                            Id = apartmentStatus.Id,
                                                            TenantId = apartmentStatus.TenantId,
                                                            Name = apartmentStatus.Name,
                                                            Code = apartmentStatus.Code,
                                                            ColorCode = apartmentStatus.ColorCode
                                                        })
                                                        .ApplySearchFilter(input.Keyword, x => x.Name, x => x.Code);
                List<ApartmentStatusDto> apartmentStatusDtos = query
                    .ApplySort(input.OrderBy, input.SortBy)
                    .ApplySort(OrderByApartmentStatus.CODE)
                    .Skip(input.SkipCount).Take(input.MaxResultCount).ToList();
                return DataResult.ResultSuccess(apartmentStatusDtos, "Get success!", query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> GetApartmentStatusById(long id)
        {
            try
            {
                ApartmentStatus apartmentStatus = await _apartmentStatusRepos.FirstOrDefaultAsync(id);
                return DataResult.ResultSuccess(apartmentStatus, "Get by id success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> CreateApartmentStatus(CreateApartmentStatusInput input)
        {
            try
            {
                ApartmentStatus? apartmentStatusOrg = await _apartmentStatusRepos.FirstOrDefaultAsync(x => x.Code == input.Code);
                if (apartmentStatusOrg != null) throw new UserFriendlyException(409, "ApartmentStatus is exist");
                ApartmentStatus apartmentStatusInsert = input.MapTo<ApartmentStatus>();
                apartmentStatusInsert.TenantId = AbpSession.TenantId;
                await _apartmentStatusRepos.InsertAsync(apartmentStatusInsert);
                return DataResult.ResultSuccess(true, "Insert success !");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> UpdateApartmentStatus(UpdateApartmentStatusInput input)
        {
            try
            {
                // Retrieve the original ApartmentStatus entity
                ApartmentStatus apartmentStatusOrg = await _apartmentStatusRepos.FirstOrDefaultAsync(x => x.Id == input.Id);
                if (apartmentStatusOrg == null)
                {
                    throw new UserFriendlyException("ApartmentStatus not found");
                }

                // Check if another entity with the same code exists
                ApartmentStatus apartmentStatusTemp = await _apartmentStatusRepos.FirstOrDefaultAsync(x => x.Id != input.Id && x.Code == input.Code);
                if (apartmentStatusTemp != null)
                {
                    throw new UserFriendlyException("Code already exists");
                }

                ObjectMapper.Map(input, apartmentStatusOrg);

                // Save the changes to the repository
                await _apartmentStatusRepos.UpdateAsync(apartmentStatusOrg);

                return DataResult.ResultSuccess(true, "Update success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteApartmentStatus(long id)
        {
            try
            {
                await _apartmentStatusRepos.DeleteAsync(id);
                return DataResult.ResultSuccess("Delete success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> DeleteManyApartmentStatus([FromBody] List<long> ids)
        {
            try
            {
                await _apartmentStatusRepos.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list apartment success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
    }
}
