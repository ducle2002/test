using Abp.Application.Services;
using Abp.UI;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using IMAX.App.ServiceHttpClient.Imax.Business;
using IMAX.Common.DataResult;
using IMAX.Services.SmartSocial.Advertisements.Dto;
using System;
using System.Threading.Tasks;

namespace IMAX.Services.SmartSocial.Advertisements
{
    public interface IAdvertisementAppService : IApplicationService
    {
        Task<DataResult> GetAllAdvertisementsByPartnerAsync(GetAllAdvertisementsByPartnerInputDto input);
        Task<DataResult> GetAllAdvertisementsByUserAsync(GetAllAdvertisementsInputDto input);
        Task<DataResult> CreateAdvertisementAsync(CreateAdvertisementInputDto input);
        Task<DataResult> ApprovalAdvertisementAsync(ApprovalAdvertisementInputDto input);
        Task<DataResult> DeleteAdvertisementAsync(DeleteAdvertisementInputDto input);
    }
    public class AdvertisementAppService : IMAXAppServiceBase, IAdvertisementAppService
    {
        private readonly IHttpAdvertisementService _httpAdvertisementService;
        public AdvertisementAppService(IHttpAdvertisementService httpAdvertisementService)
        {
            _httpAdvertisementService = httpAdvertisementService;
        }
        public async Task<DataResult> GetAllAdvertisementsByPartnerAsync(GetAllAdvertisementsByPartnerInputDto input)
        {
            try
            {
                GetAllAdvertisementsDto request = new()
                {
                    ProviderId = input.ProviderId,
                    ItemId = input.ItemId,
                    DateFrom = input.DateFrom,
                    DateTo = input.DateTo,
                    MaxResultCount = input.MaxResultCount,
                    FormId = (int)input.FormId,
                    CategoryId = input.CategoryId,
                    TypeBusiness = input.TypeBusiness,
                    SkipCount = input.SkipCount,
                    PartnerId = AbpSession.UserId,
                };
                var data = await _httpAdvertisementService.GetAllAdvertisementsByPartner(request);

                return DataResult.ResultSuccess(data.Result, "Get advertisements by partner success");
            }
            catch (UserFriendlyException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<DataResult> GetAllAdvertisementsByUserAsync(GetAllAdvertisementsInputDto input)
        {
            try
            {
                GetAllAdvertisementsDto request = new()
                {
                    ProviderId = input.ProviderId,
                    ItemId = input.ItemId,
                    DateFrom = input.DateFrom,
                    DateTo = input.DateTo,
                    FormId = (int)FORM_ID_USER.ACTIVED,
                    CategoryId = input.CategoryId,
                    TypeBusiness = input.TypeBusiness,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount,
                    PartnerId = input.PartnerId,
                };
                var data = await _httpAdvertisementService.GetAllAdvertisementsByUser(request);

                return DataResult.ResultSuccess(data.Result, "Get advertisements by user success");
            }
            catch (UserFriendlyException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<DataResult> CreateAdvertisementAsync(CreateAdvertisementInputDto input)
        {
            try
            {
                CreateAdvertisementDto request = new()
                {
                    Link = input.Link,
                    Descriptions = input.Descriptions,
                    ImageUrl = input.ImageUrl,
                    ItemId = input.ItemId,
                    ProviderId = input.ProviderId,
                    CategoryId = input.CategoryId,
                    TypeBusiness = input.TypeBusiness,
                    TenantId = input.TenantId,
                    PartnerId = (long)AbpSession.UserId,
                };
                var data = await _httpAdvertisementService.CreateAdvertisement(request);
                return DataResult.ResultSuccess(data.Result, "Create advertisement success");
            }
            catch (UserFriendlyException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<DataResult> ApprovalAdvertisementAsync(ApprovalAdvertisementInputDto input)
        {
            try
            {
                ApprovalAdvertisementDto request = new()
                {
                    Id = input.Id,
                    UpdateStatus = (int)ADVERTISEMENT_STATUS.ACTIVE,
                };
                var data = await _httpAdvertisementService.ApprovalAdvertisement(request);
                return DataResult.ResultSuccess(data.Result, "Approval advertisement success");
            }
            catch (UserFriendlyException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<DataResult> DeleteAdvertisementAsync(DeleteAdvertisementInputDto input)
        {
            try
            {
                var data = await _httpAdvertisementService.DeleteAdvertisement(new DeleteAdvertisementDto()
                {
                    Id = input.Id
                });
                return DataResult.ResultSuccess(data.Result, "Delete advertisement success");
            }
            catch (UserFriendlyException ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
