using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.UI;
using IMAX.App.ServiceHttpClient.Dto;
using IMAX.App.ServiceHttpClient.Dto.Imax.Business;
using IMAX.App.ServiceHttpClient.Imax.Business;
using IMAX.App.ServiceHttpClient.IMAX.SmartCommunity;
using IMAX.Application.Protos.Business.Providers;
using IMAX.Common.DataResult;
using IMAX.Notifications;
using IMAX.Services.Notifications;
using IMAX.Services.SmartSocial.Orders.Dto;
using IMAX.Services.SmartSocial.Providers;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using NPOI.Util.Collections;
using static IMAX.Notifications.AppNotifier;
using static IMAX.Services.Notifications.AppNotifyBusiness;
using InvestmentItemDto = IMAX.App.ServiceHttpClient.Dto.Imax.Business.InvestmentItemDto;

namespace IMAX.IMAX.Services.IMAX.SmartSocial.Business.Item
{
    public interface IInvestmentPackageAppService
    {
        #region Investment Package

        Task<DataResult> GetListInvestmentPackageByPartnerAsync([FromQuery] GetAllInvestmentPackagesDto input);
        Task<DataResult> GetListInvestmentPackageByUserAsync([FromQuery] GetAllInvestmentPackagesDto input);
        Task<DataResult> GetInvestmentPackageByIdAsync([FromQuery] long id);
        Task<DataResult> CreateInvestmentPackageAsync([FromBody] CreateInvestmentPackageDto input);
        Task<DataResult> UpdateInvestmentPackageAsync([FromBody] UpdateInvestmentPackageDto input);
        Task<DataResult> UpdateStateInvestmentPackageAsync([FromBody] UpdateStateInvestmentPackageDto input);
        Task<DataResult> DeleteInvestmentPackageAsync([FromQuery] DeleteInvestmentPackageDto input);
        Task<DataResult> DeleteManyInvestmentPackageAsync([FromQuery] DeleteManyInvestmentPackageDto input);

        #endregion

        #region Investment Register

        Task<DataResult> GetListInvestmentRegisterByPartnerAsync([FromQuery] GetAllInvestmentRegistersDto input);
        Task<DataResult> GetListInvestmentRegisterByUserAsync([FromQuery] GetAllInvestmentRegistersDto input);
        Task<DataResult> GetInvestmentRegisterByIdAsync([FromQuery] long id);
        Task<DataResult> CreateInvestmentRegisterAsync([FromBody] CreateInvestmentRegisterDto input);
        Task<DataResult> UpdateInvestmentRegisterAsync([FromBody] UpdateInvestmentRegisterDto input);
        Task<DataResult> UpdateStateInvestmentRegisterAsync([FromBody] UpdateStateInvestmentRegisterDto input);
        Task<DataResult> DeleteInvestmentRegisterAsync([FromQuery] DeleteInvestmentRegisterDto input);
        Task<DataResult> DeleteManyInvestmentRegisterAsync([FromQuery] DeleteManyInvestmentRegistersDto input);

        #endregion

        #region Investment Payment

        /*Task<DataResult> GetListInvestmentPaymentsAsync([FromQuery] GetAllInvestmentPaymentsDto input);
        Task<DataResult> GetInvestmentPaymentByIdAsync([FromQuery] long id);*/
        Task<DataResult> CreateInvestmentPaymentAsync([FromBody] CreateInvestmentPaymentDto input);
        // Task<DataResult> UpdateStateInvestmentPaymentAsync([FromBody] UpdateStateInvestmentPaymentDto input);

        #endregion

        #region Investment Item

        Task<DataResult> GetListInvestmentItemsAsync([FromQuery] GetAllInvestmentItemsDto input);

        #endregion
    }

    public class InvestmentAppService : IMAXAppServiceBase, IInvestmentPackageAppService
    {
        private readonly IHttpInvestmentService _httpInvestmentService;
        private readonly IAppNotifyBusiness _appNotifyBusiness;
        private readonly IProviderAppService _providerAppService;
        private readonly IHttpPaymentService _paymentService;

        public InvestmentAppService(
            IHttpInvestmentService httpInvestmentService,
            IAppNotifyBusiness appNotifyBusiness,
            IHttpPaymentService paymentService,
            IProviderAppService providerAppService
        )
        {
            _httpInvestmentService = httpInvestmentService;
            _appNotifyBusiness = appNotifyBusiness;
            _providerAppService = providerAppService;
            _paymentService = paymentService;
        }

        #region Investment Package

        public async Task<DataResult> GetListInvestmentPackageByPartnerAsync(
            [FromQuery] GetAllInvestmentPackagesDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<InvestmentPackageDto>> listResult =
                    await _httpInvestmentService.GetListInvestmentPackageByPartner(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message,
                    listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> GetListInvestmentPackageByUserAsync([FromQuery] GetAllInvestmentPackagesDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<InvestmentPackageDto>> listResult =
                    await _httpInvestmentService.GetListInvestmentPackageByUser(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message,
                    listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> GetInvestmentPackageByIdAsync([FromQuery] long id)
        {
            try
            {
                MicroserviceResultDto<InvestmentPackageDto> result =
                    await _httpInvestmentService.GetInvestmentPackageById(id);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> CreateInvestmentPackageAsync([FromBody] CreateInvestmentPackageDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.CreateInvestmentPackage(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateInvestmentPackageAsync([FromBody] UpdateInvestmentPackageDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.UpdateInvestmentPackage(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateStateInvestmentPackageAsync(
            [FromBody] UpdateStateInvestmentPackageDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.UpdateStateInvestmentPackage(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteInvestmentPackageAsync([FromQuery] DeleteInvestmentPackageDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.DeleteInvestmentPackage(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteManyInvestmentPackageAsync([FromBody] DeleteManyInvestmentPackageDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.DeleteManyInvestmentPackage(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        #endregion

        #region Investment Register

        public async Task<DataResult> GetListInvestmentRegisterByPartnerAsync(
            [FromQuery] GetAllInvestmentRegistersDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>> listResult =
                    await _httpInvestmentService.GetListInvestmentRegisterByPartner(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message,
                    listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> GetListInvestmentRegisterByUserAsync(
            [FromQuery] GetAllInvestmentRegistersDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>> listResult =
                    await _httpInvestmentService.GetListInvestmentRegisterByUser(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message,
                    listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> GetInvestmentRegisterByIdAsync([FromQuery] long id)
        {
            try
            {
                MicroserviceResultDto<InvestmentRegisterDto> result =
                    await _httpInvestmentService.GetInvestmentRegisterById(id);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> CreateInvestmentRegisterAsync([FromBody] CreateInvestmentRegisterDto input)
        {
            try
            {
                MicroserviceResultDto<long> result = await _httpInvestmentService.CreateInvestmentRegister(input);
                if (result.Result <= 0) throw new UserFriendlyException("Create investment register fail");
                PProvider? provider =
                    await _providerAppService.GetProviderById(new() { Id = input.ProviderId, IsDataStatic = false })
                    ?? throw new UserFriendlyException("Provider not found");
                await PushNotificationRegisterPackage(new()
                {
                    UserId = (long)AbpSession.UserId,
                    PartnerId = provider.CreatorUserId,
                    TenantIdUser = AbpSession.TenantId ?? 0,
                    TenantIdPartner = provider.TenantId,
                    ProviderId = provider.Id,
                    ProviderName = provider.Name,
                    TransactionId = result.Result,
                    ImageUrl = input.ImageUrlList[0] ?? null
                });
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateInvestmentRegisterAsync([FromBody] UpdateInvestmentRegisterDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.UpdateInvestmentRegister(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateStateInvestmentRegisterAsync(
            [FromBody] UpdateStateInvestmentRegisterDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.UpdateStateInvestmentRegister(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteInvestmentRegisterAsync([FromQuery] DeleteInvestmentRegisterDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.DeleteInvestmentRegister(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteManyInvestmentRegisterAsync(
            [FromBody] DeleteManyInvestmentRegistersDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.DeleteManyInvestmentRegister(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        #endregion

        #region Investment Payment

        /*public async Task<DataResult> GetListInvestmentPaymentsAsync([FromQuery] GetAllInvestmentPaymentsDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<InvestmentPaymentDto>> listResult = await _httpInvestmentService.GetListInvestmentPayments(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message, listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }
        public async Task<DataResult> GetInvestmentPaymentByIdAsync([FromQuery] long id)
        {
            try
            {
                MicroserviceResultDto<InvestmentPaymentDto> result = await _httpInvestmentService.GetInvestmentPaymentById(id);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }*/
        public async Task<DataResult> CreateInvestmentPaymentAsync([FromBody] CreateInvestmentPaymentDto input)
        {
            try
            {
                if (input.Method != PaymentMethod.DIRECT)
                {
                    // MicroserviceResultDto<bool> result = await _httpInvestmentService.CreateInvestmentPayment(input);
                    string resultPayment = await _paymentService.CreatePaymentAsync(new()
                    {
                        TransactionId = input.InvestmentPackageId,
                        Method = (EPaymentMethod)input.Method,
                        TenantId = AbpSession.TenantId,
                        Type = EPaymentType.INVESTMENT,
                        Description = input.Note,
                        Amount = input.Amount,
                        Properties = "",
                    });
                    return DataResult.ResultSuccess(resultPayment, "Create success");
                }

                return DataResult.ResultSuccess(false, "Cannot payment by direct");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        /*public async Task<DataResult> UpdateStateInvestmentPaymentAsync([FromBody] UpdateStateInvestmentPaymentDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result = await _httpInvestmentService.UpdateStateInvestmentPayment(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }*/

        #endregion

        #region Investment Items

        public async Task<DataResult> GetListInvestmentItemsAsync([FromQuery] GetAllInvestmentItemsDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<InvestmentItemDto>> listResult =
                    await _httpInvestmentService.GetListInvestmentItems(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message,
                    listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        #endregion

        #region Investment Package Activities

        public async Task<DataResult> GetListInvestmentPackageActivitiesAsync(
            [FromQuery] GetAllInvestmentPackageActivitiesDto input)
        {
            try
            {
                MicroserviceResultDto<PagedResultDto<InvestmentPackageActivityDto>> listResult =
                    await _httpInvestmentService.GetListInvestmentPackageActivity(input);
                return DataResult.ResultSuccess(listResult.Result.Items, listResult.Message,
                    listResult.Result.TotalCount);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> CreateInvestmentPackageActivityAsync(
            [FromBody] CreateInvestmentPackageActivitiesDto input)
        {
            try
            {
                MicroserviceResultDto<long>
                    result = await _httpInvestmentService.CreateInvestmentPackageActivity(input);
                if (result.Result <= 0) throw new UserFriendlyException("Create investment register fail");
                PProvider? provider =
                    await _providerAppService.GetProviderById(new() { Id = input.ProviderId, IsDataStatic = false })
                    ?? throw new UserFriendlyException("Provider not found");
                MicroserviceResultDto<PagedResultDto<InvestmentRegisterDto>> list = await _httpInvestmentService.GetListInvestmentRegisterByPackage(input.InvestmentPackageId);
                List<long?> listUser = list.Result.Items.Select(obj => obj.CreatorUserId).Distinct().ToList();

                await PushNotificationCreatePackageActivity(new()
                {
                    UserId = listUser,
                    PartnerId = provider.CreatorUserId,
                    TenantIdUser = AbpSession.TenantId ?? 0,
                    TenantIdPartner = provider.TenantId,
                    ProviderId = provider.Id,
                    ProviderName = provider.Name,
                    TransactionId = result.Result,
                    ImageUrl = input.ImageUrlList[0] ?? null
                });
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> UpdateInvestmentPackageActivityAsync(
            [FromBody] UpdateInvestmentPackageActivityDto input)
        {
            try
            {
                MicroserviceResultDto<bool>
                    result = await _httpInvestmentService.UpdateInvestmentPackageActivity(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteInvestmentPackageActivityAsync(
            [FromQuery] DeleteInvestmentPackageActivityDto input)
        {
            try
            {
                MicroserviceResultDto<bool>
                    result = await _httpInvestmentService.DeleteInvestmentPackageActivity(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        public async Task<DataResult> DeleteManyInvestmentPackageActivitiesAsync(
            [FromBody] DeleteManyInvestmentPackageActivityDto input)
        {
            try
            {
                MicroserviceResultDto<bool> result =
                    await _httpInvestmentService.DeleteManyInvestmentPackageActivities(input);
                return DataResult.ResultSuccess(result.Result, result.Message);
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw new AbpException(e.Message);
            }
        }

        #endregion

        #region notification

        private async Task PushNotificationRegisterPackage(NotificationMessage message) // Đăng ký gói
        {
            // user
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdUser,
                Title = "Đăng ký gói đầu tư thành công",
                Message = "Đơn đăng ký đang được xử lý bởi chủ sở hữu gói",
                Action = AppNotificationAction.RegisterPackageSuccess,
                Icon = AppNotificationIcon.RegisterPackageSuccessIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                PageId = (int)TAB_ID.NOTIFICATION_USER_REGISTER_PACKAGE,
                ProviderId = message.ProviderId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
            });

            // partner
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdPartner,
                Title = "Bạn có một đơn đăng ký mới của dự án " + message.ProviderName,
                Message = $"Đơn đăng ký được tạo vào lúc {FormatDateTime(DateTime.Now)}",
                Action = AppNotificationAction.RegisterPackageSuccess,
                Icon = AppNotificationIcon.RegisterPackageSuccessIcon,
                AppType = (int)AppType.APP_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.PartnerId,
                PageId = (int)TAB_ID.NOTIFICATION_PARTNER,
                ProviderId = message.ProviderId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
            });
        }

        private async Task
            PushNotificationCreatePackageActivity(NotificationMessageToListUser message) // Tạo sự kiện cho package
        {
            // user
            await _appNotifyBusiness.SendNotifyFullyToListUser(new()
            {
                TenantId = (int)message.TenantIdUser,
                Title = "Hoạt động mới cửa gói",
                Message = "Gói của bạn vừa được thêm hoạt động mới",
                Action = AppNotificationAction.CreatePackageActivitySuccess,
                Icon = AppNotificationIcon.CreatePackageActivitySuccessIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = message.UserId,
                PageId = (int)TAB_ID.NOTIFICATION_USER_ACTIVITY_PACKAGE,
                ProviderId = message.ProviderId,
                TransactionId = message.TransactionId,
                TransactionType = (int)TYPE_TRANSACTION.ACTIVITY,
                ImageUrl = message.ImageUrl,
            });

            // partner
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdPartner,
                Title = "Hoạt động mới đã được tạo thành công ",
                Message = "Bạn đã tạo thành công hoạt động mới",
                Action = AppNotificationAction.CreatePackageActivitySuccess,
                Icon = AppNotificationIcon.CreatePackageActivitySuccessIcon,
                AppType = (int)AppType.APP_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.PartnerId,
                PageId = (int)TAB_ID.NOTIFICATION_PARTNER,
                ProviderId = message.ProviderId,
                TransactionId = message.TransactionId,
                TransactionType = (int)TYPE_TRANSACTION.ACTIVITY,
                ImageUrl = message.ImageUrl,
            });
        }

        private async Task PushNotificationCancelPackage(NotificationMessage message) // Hủy gói
        {
            // user
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdUser,
                Title = "Hủy gói đầu tư thành công",
                Message = "Yêu cầu hủy gói thành công",
                Action = AppNotificationAction.CancelPackageSuccess,
                Icon = AppNotificationIcon.CancelPackageSuccessIcon,
                AppType = (int)AppType.APP_USER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.UserId,
                PageId = (int)TAB_ID.NOTIFICATION_USER_REGISTER_PACKAGE,
                ProviderId = message.ProviderId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
            });

            // partner
            await _appNotifyBusiness.SendNotifyFullyToUser(new()
            {
                TenantId = (int)message.TenantIdPartner,
                Title = "Bạn có một yêu cầu hủy gói của dự án " + message.ProviderName,
                Message = $"Yêu cầu hủy gói được tạo vào lúc {FormatDateTime(DateTime.Now)}",
                Action = AppNotificationAction.RegisterPackageSuccess,
                Icon = AppNotificationIcon.RegisterPackageSuccessIcon,
                AppType = (int)AppType.APP_PARTNER,
                Type = (int)TYPE_NOTIFICATION.SOCIAL,
                UserId = (long)message.PartnerId,
                PageId = (int)TAB_ID.NOTIFICATION_PARTNER,
                ProviderId = message.ProviderId,
                TransactionId = message.TransactionId,
                ImageUrl = message.ImageUrl,
            });
        }

        #endregion

        #region helpers methods

        #endregion
    }
}