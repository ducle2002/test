using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos;
using Yootek.App.ServiceHttpClient.Yootek.SmartCommunity;
using Yootek.Application;
using Yootek.Authorization.Users;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Organizations;
using Microsoft.EntityFrameworkCore;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Yootek.Common.Enum.CommonENum;
namespace Yootek.Services
{
    public interface IDigitalServicesAppService : IApplicationService
    {
        Task<DataResult> GetAllAsync(GetAllDigitalServicesInputDto input);
        Task<DataResult> GetById(long id);
        Task<DataResult> Create(DigitalServicesDto input);
        Task<DataResult> Update(DigitalServicesDto input);
        Task<DataResult> Delete(long id);
    }
    [AbpAuthorize]
    public class DigitalServicesAppService : YootekAppServiceBase, IDigitalServicesAppService
    {
        private readonly IRepository<DigitalServices, long> _repository;
        private readonly IRepository<DigitalServiceDetails, long> _repositoryServiceDetails;
        private readonly IRepository<AppOrganizationUnit, long> _abpOrganizationUnitsRepository;
        private readonly IRepository<DigitalServiceCategory, long> _digitalServiceCategoryRepository;
        private readonly IHttpWorkAssignmentService _httpWorkAssignmentService;
        public DigitalServicesAppService(
            IRepository<DigitalServices, long> repository,
            IRepository<AppOrganizationUnit, long> abpOrganizationUnitsRepository,
            IRepository<DigitalServiceCategory, long> digitalServiceCategoryRepository, 
            IRepository<DigitalServiceDetails, long> repositoryServiceDetails,
            IHttpWorkAssignmentService httpWorkAssignmentService
            )
        {
            _repository = repository;
            _abpOrganizationUnitsRepository = abpOrganizationUnitsRepository;
            _digitalServiceCategoryRepository = digitalServiceCategoryRepository;
            _repositoryServiceDetails = repositoryServiceDetails;
            _httpWorkAssignmentService = httpWorkAssignmentService;
        }

        public async Task<DataResult> GetAllAsync(GetAllDigitalServicesInputDto input)
        {
            try
            {
                IQueryable<DigitalServicesDto> query = (from o in _repository.GetAll()
                                                        select new DigitalServicesDto
                                                        {
                                                            Id = o.Id,
                                                            Title = o.Title,
                                                            Description = o.Description,
                                                            Code = o.Code,
                                                            UrbanId = o.UrbanId,
                                                            Category = o.Category,
                                                            Image = o.Image,
                                                            ImageDescription = o.ImageDescription,
                                                            Address = o.Address,
                                                            Coordinates = o.Coordinates,
                                                            Contacts = o.Contacts,
                                                            TenantId = o.TenantId,
                                                            WorkTypeId = o.WorkTypeId,
                                                            UrbanText = _abpOrganizationUnitsRepository.GetAll().Where(x => x.Id == o.UrbanId).Select(x => x.DisplayName).FirstOrDefault(),
                                                            CategoryText = _digitalServiceCategoryRepository.GetAll().Where(x => x.Id == o.Category).Select(x => x.Title).FirstOrDefault(),
                                                        })
                                                        .WhereIf(!string.IsNullOrEmpty(input.Keyword),
                                                        x => x.Title.ToLower().Contains(input.Keyword.ToLower()) 
                                                        || x.Code.ToLower().Contains(input.Keyword.ToLower()) 
                                                        || x.Address.ToLower().Contains(input.Keyword.ToLower()))
                                                        .WhereIf(input.UrbanId > 0, x => x.UrbanId == input.UrbanId)
                                                        .WhereIf(input.Category > 0, x => x.Category == input.Category);
                List<DigitalServicesDto> result = await query.ApplySort(input.OrderBy, (SortBy)input.SortBy).Skip(input.SkipCount).Take(input.MaxResultCount).ToListAsync();
                return DataResult.ResultSuccess(result, Common.Resource.QuanLyChung.GetAllSuccess, query.Count());
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetById(long id)
        {
            try
            {
                var item = await _repository.GetAsync(id);
                var data = item.MapTo<DigitalServicesDto>();
                var details = await _repositoryServiceDetails.GetAllListAsync(x => x.ServicesId == id);
                data.ServiceDetails = details.Select(x => new DigitalServiceDetailsDto
                {
                    Code = x.Code,
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Price = x.Price,
                    Image = x.Image,
                }).ToList();
                return DataResult.ResultSuccess(data, Common.Resource.QuanLyChung.Success);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> Create(DigitalServicesDto dto)
        {
            try
            {
                #region Thêm mới danh mục công việc
                CreateWorkTypeDto oCreateWorkTypeDto = new CreateWorkTypeDto
                {
                    Name = dto.Title,
                    Type = (int)TypeWork.DigitalServices,
                    Description = dto.Title,
                    WorkDetails = dto.ServiceDetails != null && dto.ServiceDetails.Count > 0 ? dto.ServiceDetails.Select(x =>
                    {
                        return new WorkDetainInCreateWorkTypeDto
                        {
                            Name = x.Title,
                            Description = x.Description
                        };
                    }).ToList() : null,
                };
                MicroserviceResultDto<long> result = await _httpWorkAssignmentService.CreateWorkType(oCreateWorkTypeDto);
                if (result.Result > 0)
                    dto.WorkTypeId = result.Result;
                #endregion
                DigitalServices item = dto.MapTo<DigitalServices>();
                item.TenantId = AbpSession.TenantId;
                var serviceId = await _repository.InsertAndGetIdAsync(item);
                if (dto.ServiceDetails != null && dto.ServiceDetails.Count > 0)
                {
                    await InsertOrUpdateServiceDetails(serviceId, dto.ServiceDetails);

                }

                return DataResult.ResultSuccess(Common.Resource.QuanLyChung.InsertSuccess);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Update(DigitalServicesDto dto)
        {
            try
            {
                DigitalServices item = await _repository.GetAsync(dto.Id);
                if (item != null)
                {
                    dto.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    await _repository.UpdateAsync(item);
                    await InsertOrUpdateServiceDetails(dto.Id, dto.ServiceDetails);
                    return DataResult.ResultSuccess(true, Common.Resource.QuanLyChung.UpdateSuccess);
                }
                else
                {
                    throw new UserFriendlyException(Common.Resource.QuanLyChung.UpdateFail);
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<DataResult> Delete(long id)
        {
            try
            {
                DigitalServices item = await _repository.GetAsync(id);
                await _repository.DeleteAsync(id);
                if (item.WorkTypeId.HasValue && item.WorkTypeId.Value > 0)
                    await _httpWorkAssignmentService.DeleteWorkType(new DeleteWorkTypeDto { Id = item.WorkTypeId.Value });
                var data = DataResult.ResultSuccess(Common.Resource.QuanLyChung.DeleteSuccess);
                return data;
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        private async Task<bool> InsertOrUpdateServiceDetails(long idService, List<DigitalServiceDetailsDto> details)
        {
            #region lay id cu va xoa bo nhung cai ko co trong danh sach moi
            var lstDetailsOld = await _repositoryServiceDetails.GetAllListAsync(x => x.ServicesId == idService);
            var lstId = lstDetailsOld.Where(x => details.Count(y => y.Id == x.Id) == 0).Select(x => x.Id);
            foreach (var item in lstId)
            {
                await _repositoryServiceDetails.DeleteAsync(item);
            }
            #endregion
            foreach (var odetails in details)
            {
                if (odetails.Id > 0)
                {
                    DigitalServiceDetails item = lstDetailsOld.FirstOrDefault(x => x.Id == odetails.Id);
                    odetails.MapTo(item);
                    item.TenantId = AbpSession.TenantId;
                    item.ServicesId = idService;
                    await _repositoryServiceDetails.UpdateAsync(item);
                }
                else
                {
                    DigitalServiceDetails item = odetails.MapTo<DigitalServiceDetails>();
                    item.TenantId = AbpSession.TenantId;
                    item.ServicesId = idService;
                    await _repositoryServiceDetails.InsertAsync(item);
                }
            }
            return true;
        }
    }
}