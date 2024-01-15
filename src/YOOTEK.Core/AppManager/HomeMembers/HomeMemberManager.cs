using Abp.Domain.Repositories;
using Yootek.EntityDb;
using Yootek.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Yootek.AppManager.HomeMembers
{
    public class HomeMemberManager : YootekDomainServiceBase, IHomeMemberManager
    {
        private readonly IRepository<HomeMember, long> _memberRepos;
        private readonly ICloudMessagingManager _cloudMessagingManager;
        private readonly IRepository<FcmGroups, long> _fcmGroupRepos;

        public HomeMemberManager(
            IRepository<HomeMember, long> memberRepos,
            ICloudMessagingManager cloudMessagingManager,
            IRepository<FcmGroups, long> fcmGroupRepos)
        {
            _memberRepos = memberRepos;
            _cloudMessagingManager = cloudMessagingManager;
            _fcmGroupRepos = fcmGroupRepos;
        }


        public async Task UpdateCitizenInHomeMember(Citizen citizen, bool isActive, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(citizen.ApartmentCode) || citizen.AccountId == null) return;
                    var member = _memberRepos.FirstOrDefault(x =>
                        x.UserId == citizen.AccountId && x.ApartmentCode == citizen.ApartmentCode);
                    if (member == null) return;

                    member.IsActive = isActive;
                    await _memberRepos.UpdateAsync(member);
                    await CurrentUnitOfWork.SaveChangesAsync();

                    var tokens
                        = await _cloudMessagingManager.GetTokensOfUser((long)citizen.AccountId, tenantId);

                    var apartmentGroupName =
                        string.Format("apartment_{0}_{1}", tenantId, citizen.ApartmentCode);
                    var apartmentGroup =
                        await _fcmGroupRepos.FirstOrDefaultAsync(x => x.GroupName == apartmentGroupName);
                    var fcmGroupKey = await _cloudMessagingManager.FcmGetGroupNotificationKey(apartmentGroupName);
                    if (isActive)
                    {
                        if (string.IsNullOrEmpty(fcmGroupKey))
                        {
                            fcmGroupKey =
                                await _cloudMessagingManager.FcmCreateDeviceGroup(apartmentGroupName, tokens);
                        }

                        if (apartmentGroup == null && !fcmGroupKey.IsNullOrEmpty())
                        {
                            var groupCloudMessaging = new FcmGroups()
                            {
                                GroupName = apartmentGroupName,
                                NotificationKey = fcmGroupKey,
                                TenantId = tenantId
                            };
                            await _fcmGroupRepos.InsertAsync(groupCloudMessaging);
                        }
                        else if (apartmentGroup != null && !fcmGroupKey.IsNullOrEmpty())
                        {
                            apartmentGroup.NotificationKey = fcmGroupKey;
                            await _fcmGroupRepos.UpdateAsync(apartmentGroup);
                        }
                    }
                    else
                    {
                        // Case unactive citizen
                        if (apartmentGroup != null)
                        {
                            await _cloudMessagingManager.FcmRemoveDevicesFromGroup(new FcmRemoveDevicesFromGroupInput
                            {
                                Tokens = tokens,
                                NotificationKey = apartmentGroup.NotificationKey,
                                Name = apartmentGroupName,
                            });
                            var newFcmGroupKey =
                                await _cloudMessagingManager.FcmGetGroupNotificationKey(apartmentGroupName);
                            if (newFcmGroupKey == null)
                            {
                                await _fcmGroupRepos.DeleteAsync(apartmentGroup);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }

        public async Task<string> GetApartmentCodeOfUser(long userId, int? tenantId)
        {
            using (CurrentUnitOfWork.SetTenantId(tenantId))
            {
                try
                {
                    var member = _memberRepos.FirstOrDefault(x => x.UserId == userId);
                    if (member != null)
                    {
                        var apartmentCode = member.ApartmentCode;
                        return apartmentCode;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }
    }
}