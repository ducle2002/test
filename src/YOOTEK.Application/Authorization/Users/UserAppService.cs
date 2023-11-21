using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.AutoMapper;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.IO;
using Abp.Linq.Extensions;
using Abp.Localization;
using Abp.MultiTenancy;
using Abp.Notifications;
using Abp.Runtime.Session;
using Abp.UI;
using IMAX.Authorization;
using IMAX.Authorization.Accounts;
using IMAX.Authorization.Roles;
using IMAX.Authorization.Users;
using IMAX.Common.DataResult;
using IMAX.EntityDb;
using IMAX.Friendships;
using IMAX.Friendships.Cache;
using IMAX.Friendships.Dto;
using IMAX.Notifications;
using IMAX.Roles.Dto;
using IMAX.Storage;
using IMAX.Users.Dto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IMAX.Users
{
   // [AbpAuthorize(PermissionNames.Pages_Users, PermissionNames.Pages_Users_Activation)]
    public class UserAppService : AsyncCrudAppService<User, UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        private readonly UserManager _userManager;
        private readonly RoleManager _roleManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        // private readonly IRepository<Friendship, long> _friendshipRepos;
        private readonly IUserFriendsCache _userFriendsCache;
        private readonly IRepository<Role> _roleRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IAbpSession _abpSession;
        private readonly LogInManager _logInManager;
        private readonly IAppFolders _appFolders;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly INotificationSubscriptionManager _notificationSubscriptionManager;
        private readonly IAppNotifier _appNotifier;
        private readonly ITenantCache _tenantCache;
        private readonly IRepository<Staff, long> _staffRepository;
        public UserAppService(
            IRepository<User, long> repository,

            UserRegistrationManager userRegistrationManager,
            IRepository<Staff, long> staffRepository,
            // IRepository<Friendship, long> friendshipRepos,
            IUserFriendsCache userFriendsCache,
            UserManager userManager,
            RoleManager roleManager,
            INotificationSubscriptionManager notificationSubscriptionManager,
            IAppNotifier appNotifier,
            IRepository<Role> roleRepository,
            IPasswordHasher<User> passwordHasher,
            IAbpSession abpSession,
            LogInManager logInManager,
            IAppFolders appFolders,
            IBinaryObjectManager binaryObjectManager,
            ITenantCache tenantCache)
            : base(repository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userRegistrationManager = userRegistrationManager;
            _userFriendsCache = userFriendsCache;
            _notificationSubscriptionManager = notificationSubscriptionManager;
            _appNotifier = appNotifier;
            _roleRepository = roleRepository;
            _passwordHasher = passwordHasher;
            _abpSession = abpSession;
            _logInManager = logInManager;
            _appFolders = appFolders;
            _binaryObjectManager = binaryObjectManager;
            _tenantCache = tenantCache;
            _staffRepository = staffRepository;
        }

      
        public override async Task<PagedResultDto<UserDto>> GetAllAsync(PagedUserResultRequestDto input)
        {
            try
            {
                CheckGetAllPermission();

                var query = CreateFilteredQuery(input);

                var totalCount = await AsyncQueryableExecuter.CountAsync(query);

                query = ApplySorting(query, input);
                query = ApplyPaging(query, input);

                var entities = await AsyncQueryableExecuter.ToListAsync(query);
                var userId = AbpSession.GetUserId();
                //var cacheItem = _userFriendsCache.GetCacheItem(AbpSession.ToUserIdentifier());
                //var cacheItem = _userFriendsCache.GetUserFriendsCacheItemInternal(AbpSession.ToUserIdentifier(), FriendshipState.Accepted);

                //var friends = cacheItem.Friends.MapTo<List<FriendDto>>();
                var users = entities.Select(MapToEntityDto).ToList();
               

                return new PagedResultDto<UserDto>(
                    totalCount,
                    users
                );
            }
            catch(Exception e)
            {
                throw new Exception("");
            }
        }

        public override async Task<UserDto> CreateAsync(CreateUserDto input)
        {
            CheckCreatePermission();

            var user = ObjectMapper.Map<User>(input);

            user.TenantId = AbpSession.TenantId;
            user.IsEmailConfirmed = true;

            await _userManager.InitializeOptionsAsync(AbpSession.TenantId);

            CheckErrors(await _userManager.CreateAsync(user, input.Password));

            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            }

            CurrentUnitOfWork.SaveChanges();
            //Notifications
            await _notificationSubscriptionManager.SubscribeToAllAvailableNotificationsAsync(user.ToUserIdentifier());
           // await _appNotifier.WelcomeToTheApplicationAsync(user);

            return MapToEntityDto(user);
        }

        public async Task<UserDto> GetDetail()
        {
            //CheckPermission(PermissionNames.Pages_Users);
            var user = await _userManager.FindByIdAsync(AbpSession.GetUserId().ToString());

            //chuyển về dạng Dto để chuyển dữ liệu sang json và lưu vào qrcode
            var userDto = MapToEntityDto(user);

            return base.MapToEntityDto(user);
        }

        public async Task UpdateProfilePicture(UpdateProfilePictureInput input)
        {
            var tempProfilePicturePath = Path.Combine(_appFolders.TempFileDownloadFolder, input.FileName);

            byte[] byteArray;

            using (var fsTempProfilePicture = new FileStream(tempProfilePicturePath, FileMode.Open))
            {
                using (var bmpImage = new Bitmap(fsTempProfilePicture))
                {
                    var width = input.Width == 0 ? bmpImage.Width : input.Width;
                    var height = input.Height == 0 ? bmpImage.Height : input.Height;
                    var bmCrop = bmpImage.Clone(new Rectangle(input.X, input.Y, width, height), bmpImage.PixelFormat);

                    using (var stream = new MemoryStream())
                    {
                        bmCrop.Save(stream, bmpImage.RawFormat);
                        stream.Close();
                        byteArray = stream.ToArray();
                    }
                }
            }

            if (byteArray.LongLength > 102400) //100 KB
            {
                throw new UserFriendlyException(L("ResizedProfilePicture_Warn_SizeLimit"));
            }

            var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());

            if (user.ProfilePictureId.HasValue)
            {
                await _binaryObjectManager.DeleteAsync(user.ProfilePictureId.Value);
            }

            var storedFile = new BinaryObject(AbpSession.TenantId, byteArray);
            await _binaryObjectManager.SaveAsync(storedFile);

            user.ProfilePictureId = storedFile.Id;

            FileHelper.DeleteIfExists(tempProfilePicturePath);
        }

        public async Task<List<User>> GetAllUserByRoles(int roleId)
        {
            CheckGetPermission();

            var users = await Repository.GetAllIncluding(x => x.Roles).Where(y => y.Roles.Any(m => m.Id == roleId)).ToListAsync();
            return users;
        }

        public override async Task<UserDto> UpdateAsync(UserDto input)
        {
            CheckUpdatePermission();

            var user = await _userManager.GetUserByIdAsync(input.Id);
           // var staff = await _staffRepository.GetAllListAsync(in)

            MapToEntity(input, user);

            CheckErrors(await _userManager.UpdateAsync(user));

            if (input.RoleNames != null)
            {
                CheckErrors(await _userManager.SetRolesAsync(user, input.RoleNames));
            }

            return await GetAsync(input);
        }

        public override async Task DeleteAsync(EntityDto<long> input)
        {
            var user = await _userManager.GetUserByIdAsync(input.Id);
            var staff = await _staffRepository.FirstOrDefaultAsync(u => u.UserId == user.Id);
            if (staff != null)
            {
                await _staffRepository.DeleteAsync(staff);
            }  
            await _userManager.DeleteAsync(user);
        }

        [AbpAuthorize]
        public async Task Activate(EntityDto<long> user)
        {
            await Repository.UpdateAsync(user.Id, async (entity) =>
            {
                entity.IsActive = true;
            });
        }

        [AbpAuthorize]
        public async Task DeActivate(EntityDto<long> user)
        {
            await Repository.UpdateAsync(user.Id, async (entity) =>
            {
                entity.IsActive = false;
            });
        }

        public async Task<ListResultDto<RoleDto>> GetRoles()
        {
            var roles = await _roleRepository.GetAllListAsync();
            return new ListResultDto<RoleDto>(ObjectMapper.Map<List<RoleDto>>(roles));
        }

        public async Task ChangeLanguage(ChangeUserLanguageDto input)
        {
            await SettingManager.ChangeSettingForUserAsync(
                AbpSession.ToUserIdentifier(),
                LocalizationSettingNames.DefaultLanguage,
                input.LanguageName
            );
        }

        protected override User MapToEntity(CreateUserDto createInput)
        {
            var user = ObjectMapper.Map<User>(createInput);
            user.SetNormalizedNames();
            return user;
        }

        protected override void MapToEntity(UserDto input, User user)
        {
            ObjectMapper.Map(input, user);
            user.SetNormalizedNames();
        }

        protected override UserDto MapToEntityDto(User user)
        {
            if (user.Roles != null)
            {
                var roleIds = user.Roles.Select(x => x.RoleId).ToArray();
                var roles = _roleManager.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.NormalizedName);
                var userDto = base.MapToEntityDto(user);
                userDto.RoleNames = roles.ToArray();
                return userDto;
            }
            else
            {
                var userDto = base.MapToEntityDto(user);
                return userDto;
            }
        }
        //protected IQueryable<User> QueryGetallUser(PagedUserResultRequestDto input)
        //{
        //    var query = (from us in Repository.GetAllIncluding(x => x.Roles)
        //                 join fd in _friendshipRepos.GetAll() on us.Id equals fd.UserId into tb_fd
        //                 from fd in tb_fd.DefaultIfEmpty() 
        //                 select new UserDto()
        //                 {
        //                     UserName = us.UserName,
        //                     Surname = us.Surname,


        //                 })
        //                  .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.UserName.Contains(input.Keyword) || x.Name.Contains(input.Keyword) || x.EmailAddress.Contains(input.Keyword))
        //                 .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);

        //}

        protected override IQueryable<User> CreateFilteredQuery(PagedUserResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Roles)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), 
                    x => x.UserName.Contains(input.Keyword) 
                    || x.Name.Contains(input.Keyword) 
                    || x.EmailAddress.Contains(input.Keyword)
                    || (x.Surname+" "+x.Name).Trim().ToLower().Contains(input.Keyword.ToLower().Trim())
                    || (x.Name + " " + x.Surname).Trim().ToLower().Contains(input.Keyword.ToLower().Trim()))
                .WhereIf(input.IsActive.HasValue, x => x.IsActive == input.IsActive);
        }

        protected override async Task<User> GetEntityByIdAsync(long id)
        {
            var user = await Repository.GetAllIncluding(x => x.Roles).FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new EntityNotFoundException(typeof(User), id);
            }

            return user;
        }

        protected override IQueryable<User> ApplySorting(IQueryable<User> query, PagedUserResultRequestDto input)
        {
            return query.OrderBy(r => r.UserName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
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

        //thay đổi password tenant
        private async Task<bool> ChangePasswordAdmin(int tenantId)
        {
            using(CurrentUnitOfWork.SetTenantId(tenantId))
            {
                await _userManager.InitializeOptionsAsync(tenantId);

                var user = await _userManager.FindByNameOrEmailAsync("admin");
                if (user == null)
                {
                    throw new Exception("There is no current user!");
                }

                CheckErrors(await _userManager.ChangePasswordAsync(user, "yootek@123"));

                return true;
            }
        }

        public async Task<bool> ResetPassword(ResetPasswordDto input)
        {
            if (_abpSession.UserId == null)
            {
                throw new UserFriendlyException("Please log in before attempting to reset password.");
            }

            var currentUser = await _userManager.GetUserByIdAsync(_abpSession.GetUserId());
            var loginAsync = await _logInManager.LoginAsync(currentUser.UserName, input.AdminPassword, GetTenancyNameOrNull(), shouldLockout: false);
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
        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        public async Task<object> GetUserStatistics(GetStatisticsUserInput input)
        {
            try
            {
                DateTime now = DateTime.Now;
                int currentMonth = now.Month;
                int currentYear = now.Year;

                Dictionary<string, int> dataResult = new Dictionary<string, int>();

                switch (input.QueryCase)
                {
                    case QueryCaseUserStatistics.ByMonth:
                        if(currentMonth >= input.NumberRange)
                        {
                            for (int index = currentMonth - input.NumberRange + 1; index <= currentMonth; index++)
                            {
                                var query = Repository.GetAll().AsQueryable();
                                var count = await query.Where(x => x.IsActive && x.CreationTime.Month == index && x.CreationTime.Year == currentYear).CountAsync();
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), count);
                            }
                        } else
                        {
                            for (var index = 13 - (input.NumberRange - currentMonth); index <= 12; index++)
                            {
                                var query = Repository.GetAll().AsQueryable();
                                var count = await query.Where(x => x.IsActive && x.CreationTime.Month == index && x.CreationTime.Year == currentYear - 1).CountAsync();
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), count);
                            }
                            for (var index = 1; index <= currentMonth; index++)
                            {
                                var query = Repository.GetAll().AsQueryable();
                                var count = await query.Where(x => x.IsActive && x.CreationTime.Month == index && x.CreationTime.Year == currentYear).CountAsync();
                                dataResult.Add(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(index), count);
                            }
                        }
                        break;
                }
                var data = DataResult.ResultSuccess(dataResult, "Get success!");
                return data;
            } catch (Exception e)
            {
                var data = DataResult.ResultError(e.ToString(), "Có lỗi");
                Logger.Fatal(e.Message, e);
                throw new UserFriendlyException(e.Message);
            }
        }

    }


    //Extension method to convert Bitmap to Byte Array
    public static class BitmapExtension
    {
        public static byte[] BitmapToByteArray(this Bitmap bitmap)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
    }
}

