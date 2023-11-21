using Abp.Application.Services;
using Abp.UI;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using IMAX.App.ServiceHttpClient.Imax.Business;
using IMAX.Common.DataResult;
using IMAX.Services.Notifications.Dto;
using System;
using System.Threading.Tasks;

namespace IMAX.Services.Notifications
{
    public interface IFcmTokenAppService : IApplicationService
    {
        Task<object> CreateFcmTokenAsync(CreateFcmTokenInputDto input);
        Task<object> LogoutFcmTokenAsync(LogoutFcmTokenInputDto input);
    }
    public class FcmTokenAppService : IMAXAppServiceBase, IFcmTokenAppService
    {
        private readonly IHttpNotificationService _httpNotificationService;
        public FcmTokenAppService(IHttpNotificationService httpNotificationService)
        {
            _httpNotificationService = httpNotificationService;
        }
        public async Task<object> CreateFcmTokenAsync(CreateFcmTokenInputDto input)
        {
            try
            {
                CreateFcmTokenDto request = new()
                {
                    AppType = input.AppType,
                    DeviceId = input.DeviceId ?? String.Empty,
                    DeviceType = input.DeviceType ?? 0,
                    TenantId = input.TenantId,
                    Token = input.Token,
                };
                var data = await _httpNotificationService.CreateFcmTokenAsync(request);
                return DataResult.ResultSuccess(data.Result, "Create fcm token success");
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
        public async Task<object> LogoutFcmTokenAsync(LogoutFcmTokenInputDto input)
        {
            try
            {
                DeleteFcmTokenDto request = new()
                {
                    Token = input.Token,
                };
                var data = await _httpNotificationService.DeleteFcmTokenAsync(request);
                return DataResult.ResultSuccess(data.Result, "Logout fcm token success");
            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex.Message);
            }
        }
    }
}
