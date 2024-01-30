using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Repositories;
using Abp.IdentityFramework;
using Abp.IO;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Authorization;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Authorization.Users.Cache;
using Yootek.Common.DataResult;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Notifications;
using Yootek.Roles.Dto;
using Yootek.Storage;
using Yootek.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Yootek.Users
{
    public interface IUserDefaultAppService : IApplicationService
    {
        Task<object> GetAllReminder();
        Task<object> CreateOrUpdateReminder(ReminderDto input);
        Task<object> DeleteReminder(long id);
        Task NotifyReminder();

        //  Task<ListResultDto<RoleDto>> GetRoles();
        // Task ChangeLanguage(ChangeUserLanguageDto input);
        Task<UserDto> GetDetail();
        Task UpdateProfilePicture(UpdateUserProfilePictureInput input);
        Task<bool> ChangePassword(ChangePasswordDto input);
        Task SetTaskDeleteAccount();
        Task CancelTaskDeleteAccount();
        Task HandleDeleteAccountTasks();
    }

    [AbpAuthorize]
    public class UserDefaultAppService : ApplicationService, IUserDefaultAppService
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<Role> _roleRepository;
        private readonly IRepository<Reminder, long> _reminderRepos;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly IAppFolders _appFolders;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IAppNotifier _appNotifier;
        private readonly IUserReminderCache _userReminderCache;
        private readonly INotificationCommunicator _notificationCommunicator;

        public UserDefaultAppService(
            IUserReminderCache userReminderCache,
            IRepository<User, long> userRepos,
            IRepository<Reminder, long> reminderRepos,
            UserManager userManager,
            RoleManager roleManager,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            IAppFolders appFolders,
            IAppNotifier appNotifier,
            INotificationCommunicator notificationCommunicator,
            IBinaryObjectManager binaryObjectManager)

        {
            _userReminderCache = userReminderCache;
            _reminderRepos = reminderRepos;
            _userManager = userManager;
            _roleManager = roleManager;
            _userRepository = userRepos;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            _appFolders = appFolders;
            _appNotifier = appNotifier;
            _binaryObjectManager = binaryObjectManager;
            _notificationCommunicator = notificationCommunicator;
        }

        #region Reminder

      
        public async Task<object> CreateOrUpdateReminder(ReminderDto input)
        {
            try
            {
                input.TenantId = AbpSession.TenantId;
                if (input.Id > 0)
                {
                    //update
                    var updateData = await _reminderRepos.GetAsync(input.Id);
                    if (updateData != null)
                    {
                        input.MapTo(updateData);
                        await _reminderRepos.UpdateAsync(updateData);

                    }
                    var data = DataResult.ResultSuccess(updateData, "update success!");
                    return data;
                }
                else
                {
                    //Insert
                    var insertInput = input.MapTo<Reminder>();
                    long id = await _reminderRepos.InsertAndGetIdAsync(insertInput);
                    if (id > 0)
                    {
                        insertInput.Id = id;
                    }
                    var data = DataResult.ResultSuccess(insertInput, "Create success!");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                throw new ValidationException(e.Message);
            }
        }

        public async Task<object> GetAllReminder()
        {
            try
            {
                var result = await _reminderRepos.GetAllListAsync(x => x.CreatorUserId == AbpSession.UserId);

                var data = DataResult.ResultSuccess(result, "Get success!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                return data;
            }

        }

        public async Task NotifyReminder()
        {
            try
            {
                var hour = DateTime.Now.Hour;
                var datetime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
                var weekday = (int)DateTime.Now.DayOfWeek;
                //var reminders = await _reminderRepos.GetAllListAsync(x => x.TimeFinish.Hour == hour && x.TimeFinish.Minute == DateTime.Now.Minute);
                var reminders = _userReminderCache.GetAllReminderCache().Where(x => x.TimeFinish.Hour == hour && x.TimeFinish.Minute == DateTime.Now.Minute && x.State == (int)CommonENum.STATE_REMINDER.ON).ToList();
                if (reminders != null && reminders.Count > 0)
                {
                    foreach (var rem in reminders)
                    {
                        try
                        {
                            rem.LoopDays = JsonSerializer.Deserialize<List<int>>(rem.LoopDay);
                        }
                        catch (Exception e)
                        {

                        }

                        if ((rem.IsLoop.Value && rem.LoopDays.Contains(weekday)) || new DateTime(rem.TimeFinish.Year, rem.TimeFinish.Month, rem.TimeFinish.Day, rem.TimeFinish.Hour, rem.TimeFinish.Minute, 0) == datetime)
                        {

                            var user = new UserIdentifier(rem.TenantId, rem.CreatorUserId.Value);
                            UserIdentifier[] users = new UserIdentifier[] { user };
                            var tsk1 = _appNotifier.MultiSendMessageAsync("App.ReminderMessage", users, rem.Name);
                            var tsk2 = _notificationCommunicator.SendMessageEventAsync(AppConsts.ReminderNotify, true, users);
                            Task.WaitAll(tsk1, tsk2);
                        }

                    }
                }
            }
            catch (Exception e)
            {
            }
        }

        //public async Task<object> ShareReminder()
        //{
        //    try
        //    {
        //        var result = await _reminderRepos.GetAllListAsync(x => x.CreatorUserId == AbpSession.UserId);

        //        var data = DataResult.ResultSuccess(result, "Get success!");
        //        return data;
        //    }
        //    catch (Exception e)
        //    {
        //        var data = DataResult.ResultError(e.ToString(), "Có lỗi");
        //        return data;
        //    }

        //}

        //public async Task<object> GetAllDeviceByHomeId(long smarthomeid)
        //{
        //    try
        //    {
        //        var result = await _deviceRepos.GetAllListAsync();

        //        var data = DataResult.ResultSuccess(result, "Get success!");
        //        return data;
        //    }
        //    catch (Exception e)
        //    {
        //        var data = DataResult.ResultError(e.ToString(), "Có lỗi");
        //        return data;
        //    }

        //}

        public async Task<object> DeleteReminder(long id)
        {
            try
            {
                var device = await _reminderRepos.GetAsync(id);
                if (device != null)
                {
                    await _reminderRepos.DeleteAsync(device);
                    var data = DataResult.ResultSuccess("Delete success !");
                    return data;
                }
                else
                {
                    var data = DataResult.ResultFail("Reminder not found !");
                    return data;
                }
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                throw;
            }
        }

        public async Task<object> DeleteMultipleReminder([FromBody] List<long> ids)
        {
            try
            {
                if (ids.Count == 0) return DataResult.ResultError("Error", "Empty input!");
                await _reminderRepos.DeleteAsync(x => ids.Contains(x.Id));
                var data = DataResult.ResultSuccess("Deleted successfully!");
                return data;
            }
            catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Exception !");
                Logger.Fatal(e.Message);
                throw;
            }
        }
        #endregion


        #region  User Info

        public async Task<UserDto> GetDetail()
        {
            // CheckGetPermission();
            var user = await _userManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            return MapToEntityDto(user);
        }

        public async Task UpdateProfilePicture(UpdateUserProfilePictureInput input)
        {

            var user = await _userManager.GetUserByIdAsync(AbpSession.UserId ?? 0);

            user.ImageUrl = input.ImageUrl;

            CheckErrors(await _userManager.UpdateAsync(user));

            return;
        }


        public async Task<bool> ChangePassword(ChangePasswordDto input)
        {
            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            var user = await _userManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            if (await _userManager.CheckPasswordAsync(user, input.CurrentPassword))
            {
                CheckErrors(await _userManager.ChangePasswordAsync(user, input.NewPassword));
            }
            else
            {
                CheckErrors(IdentityResult.Failed(new IdentityError
                {
                    Description = "Incorrect password."
                }));
            }

            return true;
        }


        public async Task<UserDto> UpdateAsync(UserDto input)
        {
            if(!string.IsNullOrEmpty(input.FullName))
            {
                var names = input.FullName.Trim().Split(" ");
                if(names.Length == 1)
                {
                    input.Name = names[0];
                    input.Surname = "";
                }
                if(names.Length > 1)
                {
                    input.Surname = names[0];
                    input.Name = input.FullName.Trim().Split(input.Surname)[1].Trim();
                }
            }

            if(input.DateOfBirth.HasValue) input.DateOfBirth = TimeZoneInfo.ConvertTime(input.DateOfBirth.Value, TimeZoneInfo.Local);

            var user = await _userManager.GetUserByIdAsync(input.Id);

            MapToEntity(input, user);

            CheckErrors(await _userManager.UpdateAsync(user));

            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            }

            return input;
        }

        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attempting to reset password.");
            }

            var currentUser = await _userManager.GetUserByIdAsync(_abpSession.GetUserId());
            var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, shouldLockout: false);
            if (loginAsync.Result != AbpLoginResultType.Success)
            {
                throw new UserFriendlyException("Your 'Admin Password' did not match the one on record.  Please try again.");
            }

            if (currentUser.IsDeleted || !currentUser.IsActive)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(currentUser);
            if (!roles.Contains(StaticRoleNames.Tenants.Admin))
            {
                throw new UserFriendlyException("Only administrators may reset passwords.");
            }

            var user = await _userManager.GetUserByIdAsync(input.UserId);
            if (user != null)
            {
                user.Password = _passwordHasher.HashPassword(user, input.NewPassword);
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return true;
        }

        public async Task SetTaskDeleteAccount()
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attempting to delete account.");
            }

            var user = await _userManager.GetUserByIdAsync(_abpSession.GetUserId());
            user.WillBeDeletedDate = DateTime.Now.AddDays(AppConsts.DeleteAccountDays);
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task CancelTaskDeleteAccount()
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attempting to cancel delete account.");
            }
            // Cancel task delete account
            var user = await _userManager.GetUserByIdAsync(_abpSession.GetUserId());
            user.WillBeDeletedDate = null;
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task HandleDeleteAccountTasks()
        {
            var needDeletedUsers = _userRepository.GetAll().Where(x => x.WillBeDeletedDate <= DateTime.Now).ToList();
            foreach (var user in needDeletedUsers)
            {
                user.WillBeDeletedDate = null;
                user.IsDeleted = true;
                _userRepository.Update(user);
            }

        }

        #endregion

        #region Extension

        protected User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }

        protected void MapToEntity(UserDto input, User user)
        {
            ObjectMapper.Map(input, user);
            user.SetNormalizedNames();
        }

        protected UserDto MapToEntityDto(User user)
        {
            if (user.Roles != null)
            {
                var roleIds = user.Roles.Select(x => x.RoleId).ToArray();
                var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);
                var userDto = ObjectMapper.Map<UserDto>(user);
                userDto.RoleNames = roles.ToArray();
                return userDto;
            }
            else
            {
                var userDto = ObjectMapper.Map<UserDto>(user);
                userDto.FullName = user.Surname + " " + user.Name;
                return userDto;
            }
        }


        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        #endregion

    }

}
