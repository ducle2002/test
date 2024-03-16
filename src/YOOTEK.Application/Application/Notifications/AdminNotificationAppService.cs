using Abp.Application.Services.Dto;
using Abp.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Authorization;
using Abp.Domain.Repositories;
using Yootek.Yootek.EntityDb.Yootek.DichVu.Business;
using Yootek.Authorization.Users;
using Yootek.MultiTenancy;
using Yootek.Common.DataResult;
using Yootek.EntityDb;
using Microsoft.EntityFrameworkCore;
using Yootek.Application.Notifications.Dto;
using Abp.Linq.Extensions;
using Abp;
using Yootek.Organizations.AppOrganizationUnits;
using Yootek.Organizations;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Abp.Domain.Uow;
using NPOI.SS.Formula.Functions;

namespace Yootek.Notifications
{
    public interface IAdminNotificationAppService : IApplicationService
    {
        Task SchedulerDayCreateNotificationAsync();
        Task SchedulerMonthCreateNotificationAsync();
        Task SchedulerYearCreateNotificationAsync();
    }

    [AbpAuthorize(IOCPermissionNames.Pages_SmartSocial_Notification)]
    public class AdminNotificationAppService : YootekAppServiceBase, IAdminNotificationAppService
    {
        private readonly INotificationDefinitionManager _notificationDefinitionManager;
        private readonly IUserNotificationManager _userNotificationManager;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IRepository<Provider, long> _providerRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Tenant> _tenantRepository;
        private readonly IRepository<Citizen, long> _citizenRepository;
        private readonly IRepository<Apartment, long> _apartmentRepository;
        private readonly IRepository<AppOrganizationUnit, long> _organizationUnitRepository;
        private readonly IRepository<SchedulerNotification, long> _schedulerNotificationRepository;

        public AdminNotificationAppService(
            INotificationDefinitionManager notificationDefinitionManager,
            IUserNotificationManager userNotificationManager,
            IAppNotifier appNotifier,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IRepository<Provider, long> providerRepository,
            IRepository<User, long> userRepository,
            IRepository<Tenant> tenantRepository,
            IRepository<Citizen, long> citizenRepository,
            IRepository<AppOrganizationUnit, long> organizationUnitRepository,
            IRepository<SchedulerNotification, long> schedulerNotificationRepository,
            IRepository<Apartment, long> apartmentRepository
            )
        {
            _notificationDefinitionManager = notificationDefinitionManager;
            _userNotificationManager = userNotificationManager;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _providerRepository = providerRepository;
            _userRepository = userRepository;
            _tenantRepository = tenantRepository;
            _citizenRepository = citizenRepository;
            _organizationUnitRepository = organizationUnitRepository;
            _schedulerNotificationRepository = schedulerNotificationRepository;
            _apartmentRepository = apartmentRepository;
        }

        public async Task<object> GetAllTenantAsync()
        {
            try
            {
                var result = await _tenantRepository.GetAll().Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                }).ToListAsync();
                return DataResult.ResultSuccess(result, "Get  success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetBuildingAsync(GetBuildingInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    IQueryable<AppOrganizationUnitDto> query = (from tp in _organizationUnitRepository.GetAll()
                                                                join ub in _organizationUnitRepository.GetAll() on tp.ParentId.Value equals ub.Id into tbl_ub
                                                                from ub in tbl_ub.DefaultIfEmpty()
                                                                where tp.Type == APP_ORGANIZATION_TYPE.BUILDING
                                                                select new AppOrganizationUnitDto()
                                                                {
                                                                    Type = tp.Type,
                                                                    Id = tp.ParentId.Value,
                                                                    ImageUrl = tp.ImageUrl,
                                                                    DisplayName = tp.DisplayName,
                                                                    ParentId = tp.ParentId,
                                                                    ProjectCode = tp.ProjectCode,
                                                                    TenantId = tp.TenantId,
                                                                    UrbanId = ub.ParentId
                                                                });

                    List<AppOrganizationUnitDto> result = await query.ToListAsync();
                    return DataResult.ResultSuccess(result, "Get  success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetUrbanAsync(GetBuildingInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    IQueryable<AppOrganizationUnitDto> query = (from tp in _organizationUnitRepository.GetAll()
                                                                join ub in _organizationUnitRepository.GetAll() on tp.ParentId.Value equals ub.Id into tbl_ub
                                                                from ub in tbl_ub.DefaultIfEmpty()
                                                                where tp.Type == APP_ORGANIZATION_TYPE.URBAN
                                                                select new AppOrganizationUnitDto()
                                                                {
                                                                    Type = tp.Type,
                                                                    Id = tp.ParentId.Value,
                                                                    ImageUrl = tp.ImageUrl,
                                                                    DisplayName = tp.DisplayName,
                                                                    ParentId = tp.ParentId,
                                                                    ProjectCode = tp.ProjectCode,
                                                                    TenantId = tp.TenantId,
                                                                    UrbanId = ub.ParentId
                                                                });

                    List<AppOrganizationUnitDto> result = await query.ToListAsync();
                    return DataResult.ResultSuccess(result, "Get  success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetApartmentAsync(GetApartmentInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    var query = _apartmentRepository.GetAll()
                        .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                        .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                        .Select(x => new ApartmentGetBySocialAdminDto()
                        {
                            Id = x.Id,
                            UrbanId = x.UrbanId,
                            ApartmentCode = x.ApartmentCode,
                            BuildingId = x.BuildingId,
                            TenantId = x.TenantId,
                        });

                    var result = await query.ToListAsync();
                    return DataResult.ResultSuccess(result, "Get  success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }
        public async Task<object> GetAllCitizenAsync(GetAllCitizenInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                {
                    var citizens = await _citizenRepository.GetAll().Select(x => new
                    {
                        Id = x.CreatorUserId,
                        FullName = x.FullName,
                        UserId = x.CreatorUserId,
                        State = x.State,
                        UrbanId = x.UrbanId,
                        BuildingId = x.BuildingId,
                        TenantId = x.TenantId,
                    })
                                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                .Where(x => x.State == STATE_CITIZEN.ACCEPTED)
                                .ToListAsync();
                    return DataResult.ResultSuccess(citizens, "Get  success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> GetAllProviderAsync(GetAllProviderInput input)
        {
            try
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    var result = await _providerRepository.GetAll().Select(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        UserId = x.CreatorUserId,
                        Type = x.Type,
                        PhoneNumber = x.PhoneNumber,
                        IsDataStatic = x.IsDataStatic,
                        GroupType = x.GroupType
                    })
                        .Where(x => x.IsDataStatic != true)
                        .WhereIf(input.GroupType.HasValue, x => x.GroupType == input.GroupType)
                        .WhereIf(input.Type.HasValue, x => x.Type == input.Type)
                        .ToListAsync();
                    return DataResult.ResultSuccess(result, "Get  success!");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<object> CreateNotificationAsync(CreateNotificationInput input)
        {
            try
            {
                var message = new UserMessageNotificationDataBase(
                    AppNotificationAction.AdminSendNotification,
                    AppNotificationIcon.AdminSendNotification,
                    TypeAction.Detail,
                    input.Content,
                    AppRouterLinks.GetEnumValue(input.TypeNotification),
                    "",
                    input.Icon
                );
                var users = new List<UserIdentifier>();
                users.Add(AbpSession.ToUserIdentifier());

                bool isSoical = false;
                if (input.IsTenant)
                {
                    using (CurrentUnitOfWork.SetTenantId(input.TenantId))
                    {
                        if (input.UserIds != null && input.UserIds.Count > 0)
                        {
                            foreach (var id in input.UserIds)
                            {
                                users.Add(new UserIdentifier(input.TenantId, id));
                            }
                        }
                        else if(input.IsOnlyCitizen.HasValue && input.IsOnlyCitizen.Value)
                        {
                            var citizens = await _citizenRepository.GetAll().Select(x => new
                            {
                                Id = x.Id,
                                FullName = x.FullName,
                                UserId = x.CreatorUserId,
                                State = x.State,
                                UrbanId = x.UrbanId,
                                BuildingId = x.BuildingId,
                                TenantId = x.TenantId,
                            })
                                .WhereIf(input.BuildingId.HasValue, x => x.BuildingId == input.BuildingId)
                                .WhereIf(input.UrbanId.HasValue, x => x.UrbanId == input.UrbanId)
                                .Where(x => x.State == STATE_CITIZEN.ACCEPTED)
                                .ToListAsync();

                            var us = citizens.Select(x => new UserIdentifier(x.TenantId, x.UserId ?? 0)).ToList();
                            users.AddRange(us);
                        }
                        else
                        {
                            var us = _userRepository.GetAll().Where(x => x.IsActive == true).Select(x => new UserIdentifier(x.TenantId, x.Id)).ToList();
                            users.AddRange(us);
                        }

                    }
                }
                else if (input.IsProvider)
                {
                    using (CurrentUnitOfWork.SetTenantId(null))
                    {

                        if (input.UserIds != null && input.UserIds.Count > 0)
                        {
                            foreach (var id in input.UserIds)
                            {
                                users.Add(new UserIdentifier(null, id));
                            }
                        }
                        else
                        {
                            var providers = await _providerRepository.GetAll().Select(x => new
                            {
                                Id = x.Id,
                                Name = x.Name,
                                UserId = x.CreatorUserId,
                                Type = x.Type,
                                PhoneNumber = x.PhoneNumber,
                                IsDataStatic = x.IsDataStatic,
                                GroupType = x.GroupType
                            })
                             .Where(x => x.IsDataStatic != true)
                             .WhereIf(input.BusinessType.HasValue, x => x.Type == input.BusinessType)
                             .ToListAsync();

                            var us = providers.Select(x => new UserIdentifier(null, x.UserId ?? 0)).Distinct().ToList();
                            users.AddRange(us);

                        }
                    }
                }
                else
                {
                    isSoical = true;
                }

                if(input.ListTimes != null && input.ListTimes.Count > 0)
                {
                    var times = new List<DateTime>();
                    foreach(var time in input.ListTimes)
                    {
                        var check = new DateTime(time.Year, time.Month, time.Day, time.Hour, 0, 0);
                        times.Add(check);
                    }
                    input.ListTimes = times;
                }

                var scheduler = new SchedulerNotification()
                {
                    Content = input.Content,
                    Header = input.Content,
                    IsScheduler = input.IsScheduler,
                    IsOnlyFirebase = input.IsOnlyFirebase,
                    IsSocial = isSoical,
                    ListTimes = input.ListTimes,
                    Time = input.Time,
                    DueDate = input.DueDate,
                    TypeScheduler = input.TypeScheduler,
                    TypeNotification = input.TypeNotification,
                    Message = JsonConvert.SerializeObject(message),
                    Users = JsonConvert.SerializeObject(users),
                    IsCompleted = !input.IsScheduler
                };

                await _schedulerNotificationRepository.InsertAsync(scheduler);

                if (input.IsScheduler) return DataResult.ResultSuccess("Create notification success!");

                await SendNotification(isSoical, input.IsOnlyFirebase, message, input.Header, users);
                return DataResult.ResultSuccess("Create notification success!");

            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }

        public async Task<DataResult> GetAllSchedulerNotificationAsync(GetAllSchedulerNotificationInput input)
        {
            var query = _schedulerNotificationRepository.GetAll().OrderByDescending(x => x.Id).AsQueryable();
            return DataResult.ResultSuccess(query.PageBy(input).ToList(), "", query.Count());
        }

        public async Task<DataResult> DeleteSchedulerAsync(long id)
        {
            await _schedulerNotificationRepository.DeleteAsync(id);
            return DataResult.ResultSuccess("Delete success");
        }

        public async Task<DataResult> DeleteManyScheduler([FromBody] List<long> ids)
        {
            try
            {
                await _schedulerNotificationRepository.DeleteAsync(x => ids.Contains(x.Id));
                return DataResult.ResultSuccess("Delete list success!");
            }
            catch (Exception e)
            {
                Logger.Fatal(e.Message);
                throw;
            }
        }


        protected async Task SendNotification(bool isSocial, bool isOnlyFirebase, UserMessageNotificationDataBase message, string title, List<UserIdentifier> users)
        {
            if (isSocial)
            {
                await _appNotifier.SendUserMessageNotifyToAllUserSocialAsync(
                    title,
                    message.Message,
                    message,
                    message.DetailUrlApp,
                    message.DetailUrlApp
                    );
            }
            else if (isOnlyFirebase)
            {
                await _appNotifier.SendUserMessageNotifyOnlyFirebaseAsync(
                     title,
                    message.Message,
                    users.ToArray(),
                    message,
                    message.DetailUrlApp,
                    message.DetailUrlApp
                    );
            }
            else
            {
                await _appNotifier.SendUserMessageNotifyFullyAsync(
                      title,
                     message.Message,
                     message.DetailUrlApp,
                     message.DetailUrlApp,
                     users.ToArray(),
                     message
                     );
            }
        }


        public async Task DeleteNotification(EntityDto<Guid> input)
        {
            var notification = await _userNotificationManager.GetUserNotificationAsync(AbpSession.TenantId, input.Id);
            if (notification == null)
            {
                return;
            }

            if (notification.UserId != AbpSession.GetUserId())
            {
                throw new UserFriendlyException(L("ThisNotificationDoesntBelongToYou"));
            }

            await _userNotificationManager.DeleteUserNotificationAsync(AbpSession.TenantId, input.Id);
        }

        [AbpAllowAnonymous]
        [RemoteService]
        public async Task SchedulerDayCreateNotificationAsync()
        {
            try
            {
                await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    using (CurrentUnitOfWork.SetTenantId(null))
                    {
                        var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                        var schedulers = await _schedulerNotificationRepository.GetAllListAsync(x => x.IsScheduler == true && x.IsCompleted == false);

                        foreach (var scheduler in schedulers)
                        {
                            if (scheduler.DueDate < now)
                            {
                                scheduler.IsCompleted = true;
                                await _schedulerNotificationRepository.UpdateAsync(scheduler);
                                continue;
                            }

                            var users = JsonConvert.DeserializeObject<List<UserIdentifier>>(scheduler.Users);
                            var message = JsonConvert.DeserializeObject<UserMessageNotificationDataBase>(scheduler.Message);
                            bool isSend = false;

                            if (scheduler.TypeScheduler == TypeScheduler.Normal
                                && scheduler.ListTimes != null
                                && scheduler.ListTimes.Count > 0)
                            {
                                foreach (var time in scheduler.ListTimes)
                                {
                                    if (time == now)
                                    {
                                        isSend = true;
                                        scheduler.ListTimes.Remove(time);
                                        if (scheduler.ListTimes.Count == 0)
                                        {
                                            scheduler.IsCompleted = true;

                                        }
                                        await _schedulerNotificationRepository.UpdateAsync(scheduler);
                                        continue;
                                    }
                                }

                            }

                            if (scheduler.Time.HasValue)
                            {
                                switch (scheduler.TypeScheduler)
                                {
                                    case TypeScheduler.LoopDay:
                                        if (scheduler.Time.Value.Hour == now.Hour)
                                        {
                                            isSend = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (isSend)
                            {
                                await SendNotification(scheduler.IsSocial, scheduler.IsOnlyFirebase, message, scheduler.Header, users);
                                //await _appNotifier.SendUserMessageNotifyFullyAsync(scheduler.Header,
                                //               message.Description,
                                //               message.DetailUrlApp,
                                //               message.DetailUrlApp,
                                //               users.ToArray(),
                                //               message);
                            }
                        }
                    }
                });
            }
            catch (Exception e)
            {

            }
        }

        [AbpAllowAnonymous]
        [RemoteService]
        public async Task SchedulerMonthCreateNotificationAsync()
        {
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {
                    var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                    var schedulers = await _schedulerNotificationRepository.GetAllListAsync(x => x.IsScheduler == true && x.IsCompleted == false && x.TypeScheduler == TypeScheduler.LoopMonth);

                    foreach (var scheduler in schedulers)
                    {
                        if (scheduler.DueDate < now)
                        {
                            scheduler.IsCompleted = true;
                            await _schedulerNotificationRepository.UpdateAsync(scheduler);
                            continue;
                        }

                        var users = JsonConvert.DeserializeObject<List<UserIdentifier>>(scheduler.Users);
                        var message = JsonConvert.DeserializeObject<UserMessageNotificationDataBase>(scheduler.Message);
                        if (scheduler.Time.HasValue && scheduler.Time.Value.Day == now.Day)
                        {
                            //await _appNotifier.SendUserMessageNotifyFullyAsync(scheduler.Header,
                            //             message.Description,
                            //             message.DetailUrlApp,
                            //             message.DetailUrlApp,
                            //             users.ToArray(),
                            //             message);

                            await SendNotification(scheduler.IsSocial, scheduler.IsOnlyFirebase, message, scheduler.Header, users);
                        }

                    }
                }
            });

        }

        [AbpAllowAnonymous]
        [RemoteService]
        public async Task SchedulerYearCreateNotificationAsync()
        {
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(null))
                {

                    var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 0, 0);
                    var schedulers = await _schedulerNotificationRepository.GetAllListAsync(x => x.IsScheduler == true && x.IsCompleted == false && x.TypeScheduler == TypeScheduler.LoopYear);

                    foreach (var scheduler in schedulers)
                    {
                        if (scheduler.DueDate < now)
                        {
                            scheduler.IsCompleted = true;
                            await _schedulerNotificationRepository.UpdateAsync(scheduler);
                            continue;
                        }

                        var users = JsonConvert.DeserializeObject<List<UserIdentifier>>(scheduler.Users);
                        var message = JsonConvert.DeserializeObject<UserMessageNotificationDataBase>(scheduler.Message);
                        if (scheduler.Time.HasValue && scheduler.Time.Value.Month == now.Month)
                        {
                            //await _appNotifier.SendUserMessageNotifyFullyAsync(scheduler.Header,
                            //             message.Description,
                            //             message.DetailUrlApp,
                            //             message.DetailUrlApp,
                            //             users.ToArray(),
                            //             message);
                            await SendNotification(scheduler.IsSocial, scheduler.IsOnlyFirebase, message, scheduler.Header, users);
                        }

                    }
                }
            });

        }
    }

}

