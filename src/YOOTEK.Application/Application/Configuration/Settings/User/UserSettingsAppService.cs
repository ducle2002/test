
using Abp.Application.Services;
using Abp.Configuration;
using Abp.Net.Mail;
using Abp.Runtime.Session;
using Abp.Timing;
using Yootek.App.ServiceHttpClient;
using Yootek.Application.Configuration.Settings.User.Dto;
using Yootek.Application.Configuration.Tenant.Dto;
using Yootek.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.Application.Configuration.Settings.User
{
    public interface IUserSettingsAppService : IApplicationService
    {

    }

    public class UserSettingsAppService : YootekAppServiceBase, IUserSettingsAppService
    {
        private readonly IVietNamAdministrativeUnitService _vietNamAdministrativeUnitService;
        public UserSettingsAppService(
            IVietNamAdministrativeUnitService vietNamAdministrativeUnitService
            ) 
        {
            _vietNamAdministrativeUnitService = vietNamAdministrativeUnitService;
        }

        #region GET settings
        public async Task<UserSettingEditDto> GetAllUserSettings()
        {
            var settings = new UserSettingEditDto()
            {
                UserAddress = await GetUserAddressSettingsAsync()
            };

            return settings;
        }

        private async Task<UserAddressSettingEditDto> GetUserAddressSettingsAsync()
        {

            try
            {
                var settings = new UserAddressSettingEditDto
                {
                    Address = await SettingManager.GetSettingValueForUserAsync(AppSettings.UserConfig.UserAddress.Address, AbpSession.TenantId, AbpSession.UserId ?? 0),
                    Latitude = await SettingManager.GetSettingValueForUserAsync<float>(AppSettings.UserConfig.UserAddress.Latitude, AbpSession.TenantId, AbpSession.UserId ?? 0),
                    Longitude = await SettingManager.GetSettingValueForUserAsync<float>(AppSettings.UserConfig.UserAddress.Longitude, AbpSession.TenantId, AbpSession.UserId ?? 0),

                    DistrictCode = await SettingManager.GetSettingValueForUserAsync(AppSettings.UserConfig.UserAddress.DistrictCode, AbpSession.TenantId, AbpSession.UserId ?? 0),
                    ProvinceCode = await SettingManager.GetSettingValueForUserAsync(AppSettings.UserConfig.UserAddress.ProvinceCode, AbpSession.TenantId, AbpSession.UserId ?? 0),
                    WardCode = await SettingManager.GetSettingValueForUserAsync(AppSettings.UserConfig.UserAddress.WardCode, AbpSession.TenantId, AbpSession.UserId ?? 0),
                    PhoneNumber = await SettingManager.GetSettingValueForUserAsync(AppSettings.UserConfig.UserAddress.PhoneNumber, AbpSession.TenantId, AbpSession.UserId ?? 0),
                    Email = await SettingManager.GetSettingValueForUserAsync(AppSettings.UserConfig.UserAddress.Email, AbpSession.TenantId, AbpSession.UserId ?? 0),
                    FullName = await SettingManager.GetSettingValueForUserAsync(AppSettings.UserConfig.UserAddress.FullName, AbpSession.TenantId, AbpSession.UserId ?? 0),

                };

                if(!string.IsNullOrWhiteSpace(settings.ProvinceCode))
                {
                    settings.Province = await _vietNamAdministrativeUnitService.GetProvinceByCode(settings.ProvinceCode);
                }

                if (!string.IsNullOrWhiteSpace(settings.DistrictCode))
                {
                    settings.District = await _vietNamAdministrativeUnitService.GetDistrictByCode(settings.DistrictCode);
                }

                if (!string.IsNullOrWhiteSpace(settings.WardCode))
                {
                    settings.Ward = await _vietNamAdministrativeUnitService.GetWardByCode(settings.WardCode);
                }

                return settings;
            }
            catch(Exception ex)
            {
                return new UserAddressSettingEditDto();
            }
        }
        #endregion

        #region Update settings
        public async Task UpdateAllUserSettings(UserSettingEditDto input)
        {
            await UpdateUserAddressSettingsAsync(input.UserAddress);
        }


        private async Task UpdateUserAddressSettingsAsync(UserAddressSettingEditDto input)
        {

            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.Address, input.Address);
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.Latitude, input.Latitude.ToString());
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.Longitude, input.Longitude.ToString());

            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.DistrictCode, input.DistrictCode);
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.ProvinceCode, input.ProvinceCode);
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.WardCode, input.WardCode);
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.PhoneNumber, input.PhoneNumber);
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.Email, input.Email);
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettings.UserConfig.UserAddress.FullName, input.FullName);

        }
        #endregion
    }
}
