using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.UI;
using Yootek.Authorization;
using Yootek.Yootek.EntityDb;
using Yootek.Yootek.Services.Yootek.ImageConfigs.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Yootek.Services
{
    public class ImageConfigAppService : YootekAppServiceBase, IApplicationService
    {
        private readonly IRepository<ImageConfig, long> _imageConfigRepository;

        public ImageConfigAppService(IRepository<ImageConfig, long> imageConfigRepository)
        {
            _imageConfigRepository = imageConfigRepository;
        }

        [AbpAuthorize(IOCPermissionNames.Pages_Settings_Images_GetAll)]
        public async Task<List<ImageConfigDto>> GetList(GetListImageConfigInput input)
        {
            var tenantId = CurrentUnitOfWork.GetTenantId();
            using (CurrentUnitOfWork.SetTenantId(tenantId ?? input.TenantId))
            {
                var result = _imageConfigRepository.GetAll().AsEnumerable()
                    .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                    .WhereIf(input.Scope.HasValue, x => x.Scope == input.Scope)
                    .Select(x => new ImageConfigDto
                    {
                        TenantId = x.TenantId,
                        Id = x.Id,
                        ImageUrl = x.ImageUrl,
                        Type = x.Type,
                        Scope = x.Scope,
                        Properties = x.Properties
                    })
                    .OrderByDescending(x => x.LastModificationTime)
                    .ToList();

                return result;
            }
        }

        [AbpAuthorize(IOCPermissionNames.Pages_Settings_Images_Create)]
        public async Task<object> Create(CreateImageConfigInput input)
        {
            var imageConfig = input.MapTo<ImageConfig>();
            var tenantId = CurrentUnitOfWork.GetTenantId();

            if (tenantId != null)
            {
                imageConfig.TenantId = tenantId;
            }
            else
            {
                imageConfig.TenantId = input.TenantId;
            }

            var result = await _imageConfigRepository.InsertAsync(imageConfig);

            await CurrentUnitOfWork.SaveChangesAsync();

            return result;
        }

        [AbpAuthorize(IOCPermissionNames.Pages_Settings_Images_Edit)]
        public async Task<object> Update(UpdateImageConfigInput input)
        {
            using (CurrentUnitOfWork.SetTenantId(input.TenantId))
            {
                var imageConfig = await _imageConfigRepository.GetAsync(input.Id);
                if (imageConfig == null)
                {
                    throw new UserFriendlyException("No data found!");
                }

                input.MapTo(imageConfig);
                var result = await _imageConfigRepository.UpdateAsync(imageConfig);

                await CurrentUnitOfWork.SaveChangesAsync();

                return result;
            }
        }

        [AbpAuthorize(IOCPermissionNames.Pages_Settings_Images_Delete)]
        public async void Delete(int id)
        {
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                await _imageConfigRepository.DeleteAsync(id);
            }
        }

        [HttpGet]
        public async Task<List<ImageConfigDto>> UserGetList(UserGetListImageConfigInput input)
        {
            var tenantId = CurrentUnitOfWork.GetTenantId();
            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var query = _imageConfigRepository.GetAll()
                    .Select(x => new ImageConfigDto
                    {
                        TenantId = x.TenantId,
                        Id = x.Id,
                        ImageUrl = x.ImageUrl,
                        Type = x.Type,
                        Scope = x.Scope,
                        Properties = x.Properties
                    }).AsEnumerable()
                    .WhereIf(input.Type.HasValue, x => x.Type == input.Type);

                if (input.Scope == ImageConfigScope.Global)
                {
                    query = query.Where(x => x.TenantId == null);
                }
                else if (input.Scope == ImageConfigScope.Tenant)
                {
                    query = query.Where(x => x.TenantId == tenantId);
                }
                else
                {
                    query = query.Where(x => x.TenantId == null || x.TenantId == tenantId);
                }

                var result = query
                    .OrderByDescending(x => x.LastModificationTime)
                    .ToList();

                return result;
            }
        }
    }
}