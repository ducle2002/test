using Abp;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using CorePush.Google;
using Yootek.EntityDb;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Abp.Extensions;
using Microsoft.EntityFrameworkCore;
using Abp.Runtime.Session;
using System.Threading;

namespace Yootek.Notifications
{
    public class CloudMessagingManager : YootekDomainServiceBase, ICloudMessagingManager
    {
        private readonly IRepository<FcmTokens, long> _fcmTokenRepos;
        private readonly IRepository<FcmGroups, long> _fcmGroupRepos;
        private readonly FcmNotificationSetting _fcmNotificationSetting;
        private readonly FcmNotificationDojilandSetting _fcmNotificationDojiSetting;
        private readonly HttpClient _httpClient;
        private IAbpSession _abpSession;

        public CloudMessagingManager(
            IOptions<FcmNotificationSetting> settings,
             IOptions<FcmNotificationDojilandSetting> dojisettings,
            IRepository<FcmGroups, long> fcmGroupRepos,
            IRepository<FcmTokens, long> fcmTokenRepos,
            IHttpClientFactory httpClientFactory,
            IAbpSession abpSession
        )
        {
            _fcmNotificationSetting = settings.Value;
            _fcmNotificationDojiSetting = dojisettings.Value;
            _fcmTokenRepos = fcmTokenRepos;
            _fcmGroupRepos = fcmGroupRepos;
            _httpClient = httpClientFactory.CreateClient();
            _abpSession = abpSession;
            CreateHttpRequestMessage(abpSession);
        }

        private void CreateHttpRequestMessage(IAbpSession session)
        {
            FcmSettings settings = new FcmSettings();
            if (session.TenantId == 19)
            {
                settings.SenderId = _fcmNotificationDojiSetting.SenderId;
                settings.ServerKey = _fcmNotificationDojiSetting.ServerKey;
                         
            }
            else
            {
                settings.SenderId = _fcmNotificationSetting.SenderId;
                settings.ServerKey = _fcmNotificationSetting.ServerKey;
            }
            string authorizationKey = string.Format("key={0}", settings.ServerKey);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("project_id", settings.SenderId);
            _httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        public async Task<object> FcmAddDevicesToGroup(FcmAddDevicesToGroupInput input)
        {
            try
            {

                FcmSettings settings = new FcmSettings()
                {
                    SenderId = _fcmNotificationSetting.SenderId,
                    ServerKey = _fcmNotificationSetting.ServerKey
                };
                HttpClient httpClient = new HttpClient();

                string authorizationKey = string.Format("key={0}", settings.ServerKey);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("project_id", settings.SenderId);
                httpClient.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var data = new FcmDeviceGroupDto()
                {
                    Operation = "add",
                    NotificationKeyName = input.Name,
                    NotificationKey = input.NotificationKey
                };
                data.RegistrationIds.AddRange(input.Tokens);
                var json = JsonConvert.SerializeObject(data);
                var request = new StringContent(json, Encoding.UTF8, "application/json");
                dynamic response = await httpClient.PostAsync("https://fcm.googleapis.com/fcm/notification", request);
                if (response.IsSuccessStatusCode)
                {
                    var resJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(resJson);
                    return result.notification_key;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<object> FcmRemoveDevicesFromGroup(FcmRemoveDevicesFromGroupInput input)
        {
            try
            {
                FcmSettings settings = new FcmSettings()
                {
                    SenderId = _fcmNotificationSetting.SenderId,
                    ServerKey = _fcmNotificationSetting.ServerKey
                };
                HttpClient httpClient = new HttpClient();

                string authorizationKey = string.Format("key={0}", settings.ServerKey);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("project_id", settings.SenderId);
                httpClient.DefaultRequestHeaders.Accept
                    .Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var data = new FcmDeviceGroupDto()
                {
                    Operation = "remove",
                    NotificationKeyName = input.Name,
                    NotificationKey = input.NotificationKey
                };
                data.RegistrationIds.AddRange(input.Tokens);
                var json = JsonConvert.SerializeObject(data);
                var request = new StringContent(json, Encoding.UTF8, "application/json");
                dynamic response = await httpClient.PostAsync("https://fcm.googleapis.com/fcm/notification", request);
                if (response.IsSuccessStatusCode)
                {
                    var resJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(resJson);
                    return result.notification_key;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> FcmCreateDeviceGroup(string name, List<string> tokens)
        {
            CancellationToken cancellationToken = default(CancellationToken);
            try
            {
                FcmSettings settings = new FcmSettings()
                {
                    SenderId = _fcmNotificationSetting.SenderId,
                    ServerKey = _fcmNotificationSetting.ServerKey
                };
                //HttpClient httpClient = new HttpClient();


                using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/notification");
                httpRequest.Headers.Add("Authorization", "key = " + settings.ServerKey);
                if (!string.IsNullOrEmpty(settings.SenderId))
                {
                    httpRequest.Headers.Add("Sender", "id = " + settings.SenderId);
                }

                
                var data = new FcmDeviceGroupDto()
                {
                    Operation = "create",
                    NotificationKeyName = name
                };
                data.RegistrationIds.AddRange(tokens);
                var json = JsonConvert.SerializeObject(data);
               // var request = new StringContent(json, Encoding.UTF8, "application/json");
                httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
                using HttpResponseMessage response = await _httpClient.SendAsync(httpRequest, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var resJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(resJson);
                    return result.notification_key;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> FcmGetGroupNotificationKey(string name)
        {
            try
            {
                FcmSettings settings = new FcmSettings()
                {
                    SenderId = _fcmNotificationSetting.SenderId,
                    ServerKey = _fcmNotificationSetting.ServerKey
                };
                HttpClient httpClient = new HttpClient();

                string authorizationKey = string.Format("key={0}", settings.ServerKey);

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("project_id", settings.SenderId);
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://fcm.googleapis.com/fcm/notification?notification_key_name=" + name),
                    Content = new StringContent("", Encoding.UTF8, MediaTypeNames.Application.Json),
                };
                var response =
                    await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var resJson = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(resJson);
                    return result.notification_key;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<string>> GetTokensOfUser(long userId, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var tokens = await _fcmTokenRepos.GetAllListAsync(x => x.CreatorUserId == userId);
                return tokens.Select(x => x.Token).ToList();
            }
        }

        public async Task<List<long>> GetTokenIdsOfUser(long userId, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var tokens = await _fcmTokenRepos.GetAllListAsync(x => x.CreatorUserId == userId);
                return tokens.Select(x => x.Id).ToList();
            }
        }

        public async Task<long> GetTokenIdOfToken(string token, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                var tokenObj = await _fcmTokenRepos.FirstOrDefaultAsync(x => x.Token == token);
                return tokenObj.Id;
            }
        }

        [UnitOfWork]
        public async Task<List<FcmTokens>> GetUserDeviceCloudMessageOrNull(UserIdentifier user)
        {
            using (CurrentUnitOfWork.SetTenantId(user.TenantId))
            {
                return await _fcmTokenRepos.GetAllListAsync(x => x.CreatorUserId == user.UserId);
            }
        }

        [UnitOfWork]
        public async Task<List<FcmTokens>> GetAllDeviceCloudMessageOrNull(int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                return await _fcmTokenRepos.GetAllListAsync();
            }
        }

        [UnitOfWork]
        public async Task<object> FcmSendToMultiDevice(FcmMultiSendToDeviceInput input)
        {
            try
            {
                FcmSettings settings = new FcmSettings();
                if (_abpSession.TenantId == 19)
                {
                    settings.SenderId = _fcmNotificationDojiSetting.SenderId;
                    settings.ServerKey = _fcmNotificationDojiSetting.ServerKey;

                }
                else
                {
                    settings.SenderId = _fcmNotificationSetting.SenderId;
                    settings.ServerKey = _fcmNotificationSetting.ServerKey;
                }
                //FcmSettings settings = new FcmSettings()
                //{
                //    SenderId = _fcmNotificationSetting.SenderId,
                //    ServerKey = _fcmNotificationSetting.ServerKey
                //};
                //HttpClient httpClient = new HttpClient();

                //string authorizationKey = string.Format("key={0}", settings.ServerKey);

                //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
                //httpClient.DefaultRequestHeaders.Accept
                //    .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                FcmNotificationPayload fcmNotificationPayload = new FcmNotificationPayload
                {
                    Title = input.Title,
                    SubTitle = input.SubTitle,
                    Body = input.Body,
                    Icon = input.Icon,
                    ClickAction = input.ClickAction,
                };
                var fcmNotification = new FcmNotification
                {
                    Notification = fcmNotificationPayload,
                };
                if (!input.Data.IsNullOrEmpty())
                {
                    fcmNotification.Data = JsonConvert.DeserializeObject(input.Data!);
                }

                if (!input.GroupName.IsNullOrEmpty())
                {
                    var fcmKey = "";
                    var fcmGroup = await _fcmGroupRepos.FirstOrDefaultAsync(x =>x.GroupName == input.GroupName);
                    if (fcmGroup != null)
                    {
                        fcmKey = fcmGroup.NotificationKey ?? await FcmGetGroupNotificationKey(fcmGroup.GroupName);
                        fcmNotification.To = fcmKey;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    // Send without existing group
                    fcmNotification.RegistrationIds = input.Tokens;
                }

                var fcm = new FcmSender(settings, _httpClient);
                var fcmSendResponse = await fcm.SendAsync(fcmNotification);
                return fcmSendResponse;
            }

            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task SendMessagesToTopic(string topic, FcmMultiSendToDeviceInput input)
        {
            //var client = new RestClient("https://fcm.googleapis.com/fcm/send");
            //var request = new RestRequest();


            FcmSettings settings = new FcmSettings()
            {
                SenderId = _fcmNotificationSetting.SenderId,
                ServerKey = _fcmNotificationSetting.ServerKey
            };

            //request.AddHeader("Content-Type", "application/json");
            //string authorizationKey = string.Format("key={0}", _fcmNotificationSetting.ServerKey);
            //request.AddHeader("Authorization", authorizationKey);

            HttpClient httpClient = new HttpClient();

            string authorizationKey = string.Format("key={0}", settings.ServerKey);
            //string deviceToken = notificationModel.TokenIds;

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
            httpClient.DefaultRequestHeaders.Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var dataSend = new
            {
                to = topic,
                notification = new
                {
                    title = input.Title,
                    body = input.Body,
                    icon = input.Icon,
                    sound = "assets/imaxsound.mp3"
                }
            };

            var fcm = new FcmSender(settings, httpClient);
            var fcmSendResponse = await fcm.SendAsync(dataSend);

            //  request.AddJsonBody(dataSend);
            ////  request.AddParameter("application/json", dataSend, ParameterType.RequestBody);
            //  var ress = await client.PostAsync(request);
        }
    }
}