/*
using Abp.Application.Services;
using Abp.UI;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.Common.DataResult;
using Yootek.Services.SmartSocial.Advertisements.Dto;
using System;
using System.Threading.Tasks;

namespace Yootek.Services.SmartSocial.Advertisements
{
    public interface IAdminAdvertisementAppService : IApplicationService
    {
        Task<DataResult> GetListAsync(GetAllAdvertisementsInputDto input);
        Task<DataResult> ApprovalAsync(ApprovalAdvertisementInputDto input);
        Task<DataResult> DeleteAsync(DeleteAdvertisementInputDto input);
    }
    public class AdminAdvertisementAppService : YootekAppServiceBase, IAdminAdvertisementAppService
    {
        private readonly IHttpAdvertisementService _httpAdvertisementService;
        public AdminAdvertisementAppService(IHttpAdvertisementService httpAdvertisementService)
        {
            _httpAdvertisementService = httpAdvertisementService;
        }
        public async Task<DataResult> GetListAsync(GetAllAdvertisementsInputDto input)
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
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<DataResult> ApprovalAsync(ApprovalAdvertisementInputDto input)
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
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<DataResult> DeleteAsync(DeleteAdvertisementInputDto input)
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
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
*/
