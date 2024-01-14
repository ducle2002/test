using Abp.Application.Services;
using Abp.UI;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Business;
using Yootek.App.ServiceHttpClient.Business;
using Yootek.Common.DataResult;
using Yootek.Services.Notifications.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Yootek.Services.Notifications
{
    public interface INotificationNewAppService : IApplicationService
    {
        Task<object> GetAllNotificationsAsync(GetAllNotificationsInputDto input);
        Task<object> GetNotificationsByIdAsync(GetNotificationByIdInputDto input);
        /*Task<object> CreateNotificationToUserAsync(CreateNotificationToUserInputDto input);
        Task<object> CreateNotificationToTopicAsync(CreateNotificationToTopicInputDto input);*/
        Task<object> UpdateNotificationAsync(UpdateNotificationInputDto input);
        Task<object> SetNotificationAsReadNewAsync(SetNotificationAsReadInputDto input);
        Task<object> SetNotificationAsUnreadNewAsync(SetNotificationAsUnreadInputDto input);
        Task<object> SetAllNotificationAsReadNewAsync(SetAllNotificationAsReadInputDto input);
        Task<object> DeleteNotificationBusinessAsync(DeleteNotificationInputDto input);
    }
    public class NotificationNewAppService : YootekAppServiceBase, INotificationNewAppService
    {
        private readonly IHttpNotificationService _httpNotificationService;
        private readonly IAppNotifyBusiness _appNotifyBusiness;
        public NotificationNewAppService(IHttpNotificationService httpNotificationService,
            IAppNotifyBusiness appNotifyBusiness)
        {
            _httpNotificationService = httpNotificationService;
            _appNotifyBusiness = appNotifyBusiness;
        }
        public async Task<object> GetAllNotificationsAsync(GetAllNotificationsInputDto input)
        {
            try
            {
                GetAllNotificationsDto request = new()
                {
                    State = input.State,
                    AppType = input.AppType,
                    FormId = input.FormId,
                    ProviderId = input.ProviderId,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                    MaxResultCount = input.MaxResultCount,
                    SkipCount = input.SkipCount,
                };
                MicroserviceResultDto<ResponseGetAllNotifications> data = await _httpNotificationService.GetAllNotificationsAsync(request);
                return DataResult.ResultSuccess(data.Result, "Get all notifications success");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<object> GetNotificationsByIdAsync(GetNotificationByIdInputDto input)
        {
            try
            {
                GetNotificationByIdDto request = new()
                {
                    Id = input.Id,
                };
                var data = await _httpNotificationService.GetNotificationByIdAsync(request);

                return DataResult.ResultSuccess(data.Result, "Get notifications detail success");
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
        public async Task<object> SetNotificationAsReadNewAsync(SetNotificationAsReadInputDto input)
        {
            try
            {
                SetNotificationAsReadDto request = new()
                {
                    Id = input.Id,
                };
                var data = await _httpNotificationService.SetNotificationAsReadAsync(request);

                return DataResult.ResultSuccess(data.Result, "Set notification as read success");
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
        public async Task<object> SetNotificationAsUnreadNewAsync(SetNotificationAsUnreadInputDto input)
        {
            try
            {
                SetNotificationAsUnreadDto request = new()
                {
                    Id = input.Id,
                };
                var data = await _httpNotificationService.SetNotificationAsUnreadAsync(request);

                return DataResult.ResultSuccess(data.Result, "Set notification as un-read success");
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
        public async Task<object> SetAllNotificationAsReadNewAsync([FromQuery] SetAllNotificationAsReadInputDto input)
        {
            try
            {
                var data = await _httpNotificationService.SetAllNotificationsAsReadAsync(new SetAllNotificationsAsReadDto()
                {
                    AppType = input.AppType,
                    FormId = input.FormId,
                    ProviderId = input.ProviderId,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate,
                });

                return DataResult.ResultSuccess(data.Result, "Set all notifications as read success");
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
        /*public async Task<object> CreateNotificationToUserAsync(CreateNotificationToUserInputDto input)
        {
            try
            {
                await _appNotifyBusiness.SendNotifyFullyToUser(new AppNotifyBusiness.MessageNotifyToUserDto()
                {
                    TenantId = (int)input.TenantId,
                    Title = input.NotificationName,
                    Message = input.Data.Message,
                    Action = input.Data.Action,
                    Icon = input.Data.Action,
                    AppType = input.AppType,
                    UserId = input.UserId,
                    Type = (int)TypeNotification.SOCIAL, // loại của thông báo - xử lý sau
                });
                return DataResult.ResultSuccess(true, "Create notification to user success");
            }
            catch (UserFriendlyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }*/

        /*public async Task<object> CreateNotificationToTopicAsync(CreateNotificationToTopicInputDto input)
        {
            try
            {
                await _appNotifyBusiness.SendNotifyFullyToTopic(new AppNotifyBusiness.MessageNotifyToTopicDto()
                {
                    TenantId = (int)input.TenantId,
                    Title = input.NotificationName,
                    Message = input.Data.Message,
                    Action = input.Data.Action,
                    Icon = input.Data.Action,
                    AppType = input.AppType,
                    Type = (int)TypeNotification.SOCIAL, // loại của thông báo - xử lý sau
                    BookingId = input.Data.BookingId,
                    OrderId = input.Data.OrderId,
                    ImageUrl = input.Data.ImageUrl,
                    PageId = input.Data.PageId,
                    ProviderId = input.Data.ProviderId,
                    TopicName = "",
                });
                return DataResult.ResultSuccess(true, "Create notification to topic success");
            }
            catch (UserFriendlyException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw;
            }
        }*/

        public async Task<object> UpdateNotificationAsync(UpdateNotificationInputDto input)
        {
            try
            {
                UpdateNotificationDto request = new()
                {
                    Id = input.Id,
                    Data = input.Data,
                    DataTypeName = input.DataTypeName,
                    EntityId = input.EntityId,
                    EntityTypeAssemblyQualifiedName = input.EntityTypeAssemblyQualifiedName,
                    EntityTypeName = input.EntityTypeName,
                    ExcludedUserIds = input.ExcludedUserIds,
                    NotificationName = input.NotificationName,
                    Severity = input.Severity,
                    TenantId = input.TenantId,
                    UserId = input.UserId
                };
                var data = await _httpNotificationService.UpdateNotificationAsync(request);
                return DataResult.ResultSuccess(data.Result, "Update notification success");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<object> DeleteNotificationBusinessAsync(DeleteNotificationInputDto input)
        {
            try
            {
                var data = await _httpNotificationService.DeleteNotificationAsync(new DeleteNotificationDto()
                {
                    Id = input.Id
                });
                return DataResult.ResultSuccess(data.Result, "Delete notification success");
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

        public async Task<object> DeleteAllNotificationsBusinessAsync(DeleteAllNotificationsInputDto input)
        {
            try
            {
                var data = await _httpNotificationService.DeleteAllNotificationsAsync(new DeleteAllNotificationsDto()
                {
                    State = input.State,
                    ProviderId = input.ProviderId,
                    AppType = input.AppType,
                    FormId = input.FormId,
                    StartDate = input.StartDate,
                    EndDate = input.EndDate
                });
                return DataResult.ResultSuccess(data.Result, "Delete all notifications success");
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
