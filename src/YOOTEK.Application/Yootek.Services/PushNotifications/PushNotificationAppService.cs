using Abp.Application.Services;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.RealTime;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Services.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Extensions;
using Abp.Runtime.Session;
using Yootek.AppManager.HomeMembers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Abp;
using Abp.UI;
using Abp.Json;
using System.Linq;
using Abp.Domain.Uow;

namespace Yootek.Services
{
    public interface IPushNotificationAppService : IApplicationService
    {
        Task<object> RegisterToTenant(RegisterToTenantDto input);
        Task<object> PushNotification(PushNotificationDto input);
        Task<object> LogoutFcm(LogoutFcmDto input);
    }

    public class PushNotificationAppService : YootekAppServiceBase, IPushNotificationAppService
    {
        private readonly IRepository<FcmGroups, long> _fcmGroupRepos;
        private readonly IRepository<FcmTokens, long> _fcmTokenRepos;
        private readonly ICloudMessagingManager _cloudMessagingManager;
        private readonly IHomeMemberManager _homeMemberManager;

        public PushNotificationAppService(
            IRepository<FcmGroups, long> fcmGroupRepos,
            ICloudMessagingManager cloudMessagingManager,
            IRepository<FcmTokens, long> fcmTokenRepos,
            IHomeMemberManager homeMemberManager)
        {
            _fcmGroupRepos = fcmGroupRepos;
            _fcmTokenRepos = fcmTokenRepos;
            _homeMemberManager = homeMemberManager;
            _cloudMessagingManager = cloudMessagingManager;
        }

        protected async Task RegisterToSocial(string key)
        {
            try
            {
                using(CurrentUnitOfWork.SetTenantId(null))
                {
                    var tenantGroupName = "social";

                    var tenantGroup =
                        await _fcmGroupRepos.FirstOrDefaultAsync(x => x.GroupName == tenantGroupName);
                    var fcmGroupKey = await _cloudMessagingManager.FcmGetGroupNotificationKey(tenantGroupName);

                    if (tenantGroup == null)
                    {
                        // Add to fcm group
                        if (fcmGroupKey == null)
                        {
                            fcmGroupKey = await _cloudMessagingManager.FcmCreateDeviceGroup(tenantGroupName,
                                new List<string>() { key });
                        }

                        // Add to db
                        var fcmGroup = new FcmGroups()
                        {
                            GroupName = tenantGroupName,
                            NotificationKey = fcmGroupKey,
                            TenantId = AbpSession.TenantId
                        };
                        await _fcmGroupRepos.InsertAsync(fcmGroup);
                    }
                    else
                    {
                        // Da co group
                        // Add to fcm
                        await _cloudMessagingManager.FcmAddDevicesToGroup(new FcmAddDevicesToGroupInput
                        {
                            Tokens = new List<string>() { key },
                            NotificationKey = tenantGroup.NotificationKey,
                            Name = tenantGroupName
                        });
                    }
                }

            }
            catch (Exception e)
            {
            }
        }


        public async Task<object> RegisterToTenant(RegisterToTenantDto input)
        {
            try
            {
                long t1 = TimeUtils.GetNanoseconds();
                //await RegisterToSocial(input.Token);
                input.TenantId = AbpSession.TenantId;
                var tenantGroupName = "tenant_" + AbpSession.TenantId;
                var fcmTokenObj = await _fcmTokenRepos.FirstOrDefaultAsync(x => x.Token == input.Token);
                if (fcmTokenObj == null)
                {
                    var obj = input.MapTo<FcmTokens>();
                    await _fcmTokenRepos.InsertAndGetIdAsync(obj);
                    await CurrentUnitOfWork.SaveChangesAsync();

                  
                }

                var tenantGroup =
                    await _fcmGroupRepos.FirstOrDefaultAsync(x => x.GroupName == tenantGroupName);
                var fcmGroupKey = await _cloudMessagingManager.FcmGetGroupNotificationKey(tenantGroupName);
                if (tenantGroup == null)
                {
                    // Add to fcm group
                    if (fcmGroupKey == null)
                    {
                        fcmGroupKey = await _cloudMessagingManager.FcmCreateDeviceGroup(tenantGroupName,
                            new List<string>() { input.Token });
                    }

                    // Add to db
                    var fcmGroup = new FcmGroups()
                    {
                        GroupName = tenantGroupName,
                        NotificationKey = fcmGroupKey,
                        TenantId = AbpSession.TenantId
                    };
                    await _fcmGroupRepos.InsertAsync(fcmGroup);
                }
                else
                {
                    // Da co group
                    // Add to fcm
                    await _cloudMessagingManager.FcmAddDevicesToGroup(new FcmAddDevicesToGroupInput
                    {
                        Tokens = new List<string>() { input.Token },
                        NotificationKey = tenantGroup.NotificationKey,
                        Name = tenantGroupName
                    });
                }

                var apartmentCode =
                    await _homeMemberManager.GetApartmentCodeOfUser((long)AbpSession.UserId!, AbpSession.TenantId);
                if (!apartmentCode.IsNullOrEmpty())
                {
                    await RegisterToApartment(new RegisterToApartmentDto
                    {
                        Tokens = new List<string>() { input.Token },
                        ApartmentCode = apartmentCode,
                        TenantId = AbpSession.TenantId
                    });
                }

               // await _cloudMessagingManager.SubscribeTopic("/topics/all", new List<string>() { input.Token });
                mb.statisticMetris(t1, 0, "register_CM");
                return DataResult.ResultSuccess("Register success!");
            }
            catch (Exception e)
            {
                throw;
            }
        }

        protected async Task<object> RegisterToApartment(RegisterToApartmentDto input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                var apartmentGroupName =
                    string.Format("apartment_{0}_{1}", AbpSession.GetTenantId(), input.ApartmentCode);
                var apartmentGroup =
                    await _fcmGroupRepos.FirstOrDefaultAsync(x => x.GroupName == apartmentGroupName);
                var fcmGroupKey = await _cloudMessagingManager.FcmGetGroupNotificationKey(apartmentGroupName);
                if (fcmGroupKey.IsNullOrEmpty())
                {
                    await _cloudMessagingManager.FcmCreateDeviceGroup(apartmentGroupName, input.Tokens);
                }

                if (apartmentGroup == null)
                {
                    var groupCloudMessaging = new FcmGroups()
                    {
                        GroupName = apartmentGroupName,
                        NotificationKey = fcmGroupKey,
                        TenantId = AbpSession.TenantId
                    };
                    await _fcmGroupRepos.InsertAsync(groupCloudMessaging);
                }
                else
                {
                    await _cloudMessagingManager.FcmAddDevicesToGroup(new FcmAddDevicesToGroupInput()
                    {
                        Tokens = input.Tokens,
                        NotificationKey = apartmentGroup.NotificationKey,
                        Name = apartmentGroupName
                    });
                }

                return DataResult.ResultSuccess("Success");
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<object> PushNotification(PushNotificationDto input)
        {
            try
            {
                return await _cloudMessagingManager.FcmSendToMultiDevice(new FcmMultiSendToDeviceInput
                {
                    Title = input.Title,
                    SubTitle = input.Subtitle,
                    Body = input.Body,
                    Data = input.Data,
                    Tokens = input.Tokens,
                    Icon = input.Icon,
                    ClickAction = input.ClickAction,
                    TenantId = AbpSession.TenantId,
                    GroupName = input.GroupName
                });
            }
            catch (Exception e)
            {
                throw new UserFriendlyException(e.ToJsonString());
            }
        }

        public async Task<object> LogoutFcm(LogoutFcmDto input)
        {
            try
            {
                var fcmToken = await _fcmTokenRepos.FirstOrDefaultAsync(x =>
                    x.Token == input.Token && x.CreatorUserId == AbpSession.UserId);
                if (fcmToken != null)
                {
                    await _fcmTokenRepos.HardDeleteAsync(fcmToken);
                    // Remove from tenant group
                    var tenantGroupKey =
                        await _cloudMessagingManager.FcmGetGroupNotificationKey("tenant_" + AbpSession.TenantId);
                    await _cloudMessagingManager.FcmRemoveDevicesFromGroup(new FcmRemoveDevicesFromGroupInput()
                    {
                        Name = "tenant_" + AbpSession.TenantId,
                        Tokens = new List<string>() { input.Token },
                        NotificationKey = tenantGroupKey
                    });

                    // Remove from apartment group
                    var apartmentCode =
                        await _homeMemberManager.GetApartmentCodeOfUser((long)AbpSession.UserId!, AbpSession.TenantId);
                    if (!apartmentCode.IsNullOrEmpty())
                    {
                        var apartmentGroupKey =
                            await _cloudMessagingManager.FcmGetGroupNotificationKey(
                                string.Format("apartment_{0}_{1}", AbpSession.GetTenantId(), apartmentCode));
                        await _cloudMessagingManager.FcmRemoveDevicesFromGroup(new FcmRemoveDevicesFromGroupInput()
                        {
                            Name = "apartment_" + AbpSession.TenantId + "_" + apartmentCode,
                            Tokens = new List<string>() { input.Token },
                            NotificationKey = apartmentGroupKey
                        });
                    }
                }

                return DataResult.ResultSuccess("Logout success!");
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}