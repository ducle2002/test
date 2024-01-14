using Abp;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using Yootek.EntityDb;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Yootek.Authorization.Users.Cache
{
    public interface IUserReminderCache
    {
        List<ReminderCacheItem> GetAllReminderCache();
        UserWithReminderCacheItem GetUserCacheItem(UserIdentifier userIdentifier);

        UserWithReminderCacheItem GetUserCacheItemOrNull(UserIdentifier userIdentifier);
        UserWithReminderCacheItem UserGetReminderCacheItemInternal(UserIdentifier userIdentifier);
        List<ReminderCacheItem> GetAllReminderCacheInternal();

        void UserAddReminder(UserIdentifier userIdentifier, ReminderCacheItem reminder);

        void UserRemoveReminder(UserIdentifier userIdentifier, ReminderCacheItem reminder);

        void UserUpdateReminder(UserIdentifier userIdentifier, ReminderCacheItem reminder);
    }

    public class UserReminderCache : IUserReminderCache, ISingletonDependency
    {
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Reminder, long> _reminderRepos;
        private readonly ITenantCache _tenantCache;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        private readonly object _syncObj = new object();

        public UserReminderCache(
            IRepository<Reminder, long> reminderRepos,
            ICacheManager cacheManager,
            ITenantCache tenantCache,
            UserManager userManager,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _reminderRepos = reminderRepos;
            _cacheManager = cacheManager;
            _tenantCache = tenantCache;
            _userManager = userManager;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public virtual List<ReminderCacheItem> GetAllReminderCache()
        {
            var result = _cacheManager.GetCache<string, List<ReminderCacheItem>>(ReminderCacheItem.CacheName).Get(ReminderCacheItem.GetAll, f => GetAllReminderCacheInternal());
            return result;
        }

        public virtual List<ReminderCacheItem> GetAllReminderCacheInternal()
        {
            var reminderCacheItems =
                    (from reminder in _reminderRepos.GetAll()
                     select new ReminderCacheItem
                     {
                         Id = reminder.Id,
                         CreatorUserId = reminder.CreatorUserId,
                         CreationTime = reminder.CreationTime,
                         IsLoop = reminder.IsLoop,
                         IsNotify = reminder.IsNotify,
                         Name = reminder.Name,
                         Note = reminder.Note,
                         LoopDay = reminder.LoopDay,
                         State = reminder.State,
                         TenantId = reminder.TenantId,
                         TimeFinish = reminder.TimeFinish,
                         Level = reminder.Level

                     }).ToList();

            return reminderCacheItems;
        }
        public virtual UserWithReminderCacheItem GetUserCacheItem(UserIdentifier userIdentifier)
        {
            return _cacheManager.GetCache<string, UserWithReminderCacheItem>(ReminderCacheItem.CacheName).Get(userIdentifier.ToUserIdentifierString(), f => UserGetReminderCacheItemInternal(userIdentifier));
        }

        public virtual UserWithReminderCacheItem GetUserCacheItemOrNull(UserIdentifier userIdentifier)
        {
            return _cacheManager
                .GetCache<string, UserWithReminderCacheItem>(ReminderCacheItem.CacheName)
                .GetOrDefault(userIdentifier.ToUserIdentifierString());
        }

        public virtual void UserAddReminder(UserIdentifier userIdentifier, ReminderCacheItem item)
        {
            var user = GetUserCacheItemOrNull(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                if (!user.Reminders.Any(f => f.Id == item.Id))
                {
                    user.Reminders.Add(item);
                    UpdateReminderOnCache(userIdentifier, user);
                }
            }
        }

        public virtual UserWithReminderCacheItem UserGetReminderCacheItemInternal(UserIdentifier userIdentifier)
        {

            using (_unitOfWorkManager.Current.SetTenantId(userIdentifier.TenantId))
            {
                var reminderCacheItems =
                    (from reminder in _reminderRepos.GetAll()
                     where reminder.CreatorUserId == userIdentifier.UserId
                     select new ReminderCacheItem
                     {
                         Id = reminder.Id,
                         CreatorUserId = reminder.CreatorUserId,
                         CreationTime = reminder.CreationTime,
                         IsLoop = reminder.IsLoop,
                         IsNotify = reminder.IsNotify,
                         Name = reminder.Name,
                         Note = reminder.Note,
                         State = reminder.State,
                         TenantId = reminder.TenantId,
                         TimeFinish = reminder.TimeFinish,
                         Level = reminder.Level

                     }).ToList();

                var user = _userManager.FindByIdAsync(userIdentifier.UserId.ToString());

                return new UserWithReminderCacheItem
                {
                    TenantId = userIdentifier.TenantId,
                    UserId = userIdentifier.UserId,
                    Reminders = reminderCacheItems
                };
            }
        }

        public virtual void UserRemoveReminder(UserIdentifier userIdentifier, ReminderCacheItem item)
        {
            var user = GetUserCacheItemOrNull(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                if (user.Reminders.Any(f => f.Id == item.Id))
                {
                    user.Reminders.Remove(item);
                    UpdateReminderOnCache(userIdentifier, user);
                }
            }
        }


        public virtual void UserUpdateReminder(UserIdentifier userIdentifier, ReminderCacheItem item)
        {
            var user = GetUserCacheItemOrNull(userIdentifier);
            if (user == null)
            {
                return;
            }

            lock (_syncObj)
            {
                var existingIndex = user.Reminders.FindIndex(
                    f => f.Id == item.Id);

                if (existingIndex >= 0)
                {
                    user.Reminders[existingIndex] = item;
                    UpdateReminderOnCache(userIdentifier, user);
                }

            }
        }

        private void UpdateReminderOnCache(UserIdentifier userIdentifier, UserWithReminderCacheItem user)
        {
            _cacheManager.GetCache(ReminderCacheItem.CacheName).Set(userIdentifier.ToUserIdentifierString(), user);
        }
    }
}
