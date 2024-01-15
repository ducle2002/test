using Abp.Domain.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yootek.Notifications
{
    public interface ICloudMessagingManager : IDomainService
    {
        Task<object> FcmSendToMultiDevice(FcmMultiSendToDeviceInput input);

        Task<string> FcmCreateDeviceGroup(string name, List<string> tokens);

        Task<string> FcmGetGroupNotificationKey(string name);

        Task<object> FcmAddDevicesToGroup(FcmAddDevicesToGroupInput input);
        Task<object> FcmRemoveDevicesFromGroup(FcmRemoveDevicesFromGroupInput input);

        Task SendMessagesToTopic(string topic, FcmMultiSendToDeviceInput input);

        Task<List<string>> GetTokensOfUser(long userId, int? tenantId);
        Task<List<long>> GetTokenIdsOfUser(long userId, int? tenantId);
        Task<long> GetTokenIdOfToken(string token, int? tenantId);
    }
}