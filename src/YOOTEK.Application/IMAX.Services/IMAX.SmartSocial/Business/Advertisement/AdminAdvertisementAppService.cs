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
    public interface IAdminAdvertisementAppService : IApplicationService
    {
        Task<DataResult> GetAllAdvertisementsByAdminAsync(GetAllAdvertisementsInputDto input);
        Task<DataResult> ApprovalAdvertisementAsync(ApprovalAdvertisementInputDto input);
        Task<DataResult> DeleteAdvertisementAsync(DeleteAdvertisementInputDto input);
    }
    public class AdminAdvertisementAppService : IMAXAppServiceBase, IAdminAdvertisementAppService
    {
        private readonly IHttpAdvertisementService _httpAdvertisementService;
        public AdminAdvertisementAppService(IHttpAdvertisementService httpAdvertisementService)
        {
            _httpAdvertisementService = httpAdvertisementService;
        }
        public async Task<DataResult> GetAllAdvertisementsByAdminAsync(GetAllAdvertisementsInputDto input)
        {
            try
            {
                GetAllAdvertisementsDto request = new()
                {
                    ProviderId = input.ProviderId,
                    ItemId = input.ItemId,
                    DateFrom = input.DateFrom,
                    DateTo = input.DateTo,
                    CategoryId = input.CategoryId,
                    TypeBusiness = input.TypeBusiness,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount,
                    PartnerId = input.PartnerId,
                };
                var data = await _httpAdvertisementService.GetAllAdvertisementsByAdmin(request);

                return DataResult.ResultSuccess(data.Result, "Get advertisements success");
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
                var data = await _httpAdvertisementService.ApprovalAdvertisement(new ApprovalAdvertisementDto()
                {
                    Id = input.Id,
                    UpdateStatus = (int)ADVERTISEMENT_STATUS.ACTIVE,
                });
                return DataResult.ResultSuccess(data, "Approval advertisement success");
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
                return DataResult.ResultSuccess(data, "Delete advertisement success");
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
