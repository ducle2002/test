using Abp.Runtime.Session;
using Yootek.App.ServiceHttpClient.Dto;
using Yootek.App.ServiceHttpClient.Dto.Business;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Yootek.App.ServiceHttpClient.Business
{
    public interface IHttpNotificationService
    {
        #region Notifications
        Task<MicroserviceResultDto<ResponseGetAllNotifications>> GetAllNotificationsAsync(GetAllNotificationsDto input);
        Task<MicroserviceResultDto<NotificationDto>> GetNotificationByIdAsync(GetNotificationByIdDto input);
        Task<MicroserviceResultDto<bool>> SetNotificationAsReadAsync(SetNotificationAsReadDto input);
        Task<MicroserviceResultDto<bool>> SetNotificationAsUnreadAsync(SetNotificationAsUnreadDto input);
        Task<MicroserviceResultDto<bool>> SetAllNotificationsAsReadAsync(SetAllNotificationsAsReadDto input);
        Task<MicroserviceResultDto<bool>> CreateNotificationToOneUserAsync(CreateNotificationToOneUserDto input);
        Task<MicroserviceResultDto<bool>> CreateNotificationToListUserAsync(CreateNotificationToListUserDto input);
        Task<MicroserviceResultDto<bool>> CreateNotificationTopicAsync(CreateNotificationTopicDto input);
        Task<MicroserviceResultDto<bool>> CreateNotificationToUserFirebaseAsync(CreateNotificationToOneUserDto input);
        Task<MicroserviceResultDto<bool>> CreateNotificationTopicFirebaseAsync(CreateNotificationTopicDto input);
        Task<MicroserviceResultDto<bool>> CreateNotificationToUserDatabaseAsync(CreateNotificationToOneUserDto input);
        Task<MicroserviceResultDto<bool>> CreateNotificationTopicDatabaseAsync(CreateNotificationTopicDto input);
        Task<MicroserviceResultDto<bool>> UpdateNotificationAsync(UpdateNotificationDto input);
        Task<MicroserviceResultDto<bool>> DeleteNotificationAsync(DeleteNotificationDto input);
        Task<MicroserviceResultDto<bool>> DeleteAllNotificationsAsync(DeleteAllNotificationsDto input);
        #endregion

        #region FcmToken
        Task<MicroserviceResultDto<bool>> CreateFcmTokenAsync(CreateFcmTokenDto input);
        Task<MicroserviceResultDto<bool>> DeleteFcmTokenAsync(DeleteFcmTokenDto input);
        #endregion

        #region FcmGroup
        Task<MicroserviceResultDto<bool>> FcmAddUserToTopic(AddUserToFcmGroup input);
        Task<MicroserviceResultDto<bool>> FcmRemoveUserFromTopic(RemoveUserFromFcmGroup input);
        #endregion
    }
    public class HttpNotificationService : IHttpNotificationService
    {
        private readonly HttpClient _client;
        private readonly IAbpSession _session;
        public HttpNotificationService(HttpClient client, IAbpSession session)
        {
            _client = client;
            _session = session;
        }

        #region Notification 
        public async Task<MicroserviceResultDto<ResponseGetAllNotifications>> GetAllNotificationsAsync(GetAllNotificationsDto input)
        {
            var query = input.GetStringQueryUri();
            using (var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/Notifications/get-all{query}"))
            {
                request.HandleGet(_session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<ResponseGetAllNotifications>>();
            }
        }
        public async Task<MicroserviceResultDto<NotificationDto>> GetNotificationByIdAsync(GetNotificationByIdDto input)
        {
            string query = input.GetStringQueryUri();
            using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/Notifications/get-detail/{query}");
            request.HandleGet(_session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<NotificationDto>>();
        }
        public async Task<MicroserviceResultDto<bool>> SetNotificationAsReadAsync(SetNotificationAsReadDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/userNotifications/set-as-read"))
            {
                request.HandlePostAsJson<SetNotificationAsReadDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> SetNotificationAsUnreadAsync(SetNotificationAsUnreadDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/userNotifications/set-as-unread"))
            {
                request.HandlePostAsJson<SetNotificationAsUnreadDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> SetAllNotificationsAsReadAsync(SetAllNotificationsAsReadDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/userNotifications/set-all-as-read"))
            {
                request.HandlePostAsJson<SetAllNotificationsAsReadDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        #region Fully
        public async Task<MicroserviceResultDto<bool>> CreateNotificationToOneUserAsync(CreateNotificationToOneUserDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/Notifications/create-notification-to-user"))
            {
                request.HandlePostAsJson<CreateNotificationToOneUserDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        
        public async Task<MicroserviceResultDto<bool>> CreateNotificationToListUserAsync(CreateNotificationToListUserDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/Notifications/create-notification-to-user"))
            {
                request.HandlePostAsJson<CreateNotificationToListUserDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> CreateNotificationTopicAsync(CreateNotificationTopicDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/Notifications/create-notification-to-topic"))
            {
                request.HandlePostAsJson<CreateNotificationTopicDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        #endregion
        #region Only Firebase
        public async Task<MicroserviceResultDto<bool>> CreateNotificationToUserFirebaseAsync(CreateNotificationToOneUserDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/Notifications/create-notification-to-user-firebase"))
            {
                request.HandlePostAsJson<CreateNotificationToOneUserDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> CreateNotificationTopicFirebaseAsync(CreateNotificationTopicDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/Notifications/create-notification-to-topic-firebase"))
            {
                request.HandlePostAsJson<CreateNotificationTopicDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        #endregion
        #region Only Database
        public async Task<MicroserviceResultDto<bool>> CreateNotificationToUserDatabaseAsync(CreateNotificationToOneUserDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/Notifications/create-notification-to-user-database"))
            {
                request.HandlePostAsJson<CreateNotificationToOneUserDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> CreateNotificationTopicDatabaseAsync(CreateNotificationTopicDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/Notifications/create-notification-to-topic-database"))
            {
                request.HandlePostAsJson<CreateNotificationTopicDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        #endregion
        public async Task<MicroserviceResultDto<bool>> UpdateNotificationAsync(UpdateNotificationDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Put, $"api/v1/Notifications"))
            {
                request.HandlePutAsJson<UpdateNotificationDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> DeleteNotificationAsync(DeleteNotificationDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/Notifications/{input.Id}"))
            {
                request.HandleDeleteAsJson<DeleteNotificationDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> DeleteAllNotificationsAsync(DeleteAllNotificationsDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/Notifications/delete-all"))
            {
                request.HandleDeleteAsJson<DeleteAllNotificationsDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        #endregion

        #region FcmToken
        public async Task<MicroserviceResultDto<List<FcmTokenDto>>> GetAllFcmTokensAsync(GetAllFcmTokensDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/FcmTokens/get-all"))
            {
                request.HandlePostAsJson<GetAllFcmTokensDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<List<FcmTokenDto>>>();
            }
        }
        public async Task<MicroserviceResultDto<bool>> CreateFcmTokenAsync(CreateFcmTokenDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/FcmTokens"))
            {
                request.HandlePostAsJson<CreateFcmTokenDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }

        public async Task<MicroserviceResultDto<bool>> DeleteFcmTokenAsync(DeleteFcmTokenDto input)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Delete, $"api/v1/FcmTokens"))
            {
                request.HandlePostAsJson<DeleteFcmTokenDto>(input, _session);
                var response = await _client.SendAsync(request);
                return await response.ReadContentAs<MicroserviceResultDto<bool>>();
            }
        }
        #endregion

        #region FcmGroup
        public async Task<MicroserviceResultDto<bool>> FcmAddUserToTopic(AddUserToFcmGroup input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/FcmGroups/add-user");
            request.HandlePostAsJson<AddUserToFcmGroup>(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        public async Task<MicroserviceResultDto<bool>> FcmRemoveUserFromTopic(RemoveUserFromFcmGroup input)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/FcmGroups/remove-user");
            request.HandlePostAsJson<RemoveUserFromFcmGroup>(input, _session);
            var response = await _client.SendAsync(request);
            return await response.ReadContentAs<MicroserviceResultDto<bool>>();
        }
        #endregion
    }
}
