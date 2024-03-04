using Abp.Application.Services;
using Abp.Configuration;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.Runtime.Security;
using Abp.Timing;
using Abp.UI;
using Yootek.Application.Configuration.Tenant.Dto;
using Yootek.Configuration;
using Yootek.Editions;
using Yootek.EntityDb;
using Yootek.Timing;
using Microsoft.VisualBasic;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Yootek.Application.Configuration.Tenant
{
    public interface ITenantSettingsAppService : IApplicationService
    {
        Task<TenantSettingsEditDto> GetAllSettings();
        Task<EmailSettingsEditDto> GetEmailSettingsAsync();
    }

    public class TenantSettingsAppService : YootekAppServiceBase, ITenantSettingsAppService
    {
        private readonly IEmailSender _emailSender;
        private readonly EditionManager _editionManager;
        private readonly ITimeZoneService _timeZoneService;
        private readonly ISettingDefinitionManager _settingDefinitionManager;

        public TenantSettingsAppService(
            IEmailSender emailSender,
            EditionManager editionManager,
            ITimeZoneService timeZoneService,
            ISettingDefinitionManager settingDefinitionManager)
        {
            _emailSender = emailSender;
            _editionManager = editionManager;
            _timeZoneService = timeZoneService;
            _settingDefinitionManager = settingDefinitionManager;
        }

        #region Get Settings

        public async Task<TenantSettingsEditDto> GetAllSettings()
        {
            return new TenantSettingsEditDto
            {
                //General = await GetGeneralSettingsAsync(),
                //Email = await GetEmailSettingsAsync(),
                TimeScheduleCheckBill = await GetTimeScheduleCheckBill(),
                UserBillSetting = await GetUserBillSettings(),
                BankTransferSetting = await GetBankTransfers(),
            };
        }


        private async Task<GeneralSettingsEditDto> GetGeneralSettingsAsync()
        {
            var timezone = await SettingManager.GetSettingValueForApplicationAsync(TimingSettingNames.TimeZone);
            var settings = new GeneralSettingsEditDto
            {
                Timezone = timezone,
                TimezoneForComparison = timezone
            };

            var defaultTimeZoneId =
                await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Application, AbpSession.TenantId);
            if (settings.Timezone == defaultTimeZoneId)
            {
                settings.Timezone = string.Empty;
            }

            return settings;
        }

        public async Task<EmailSettingsEditDto> GetEmailSettingsAsync()
        {
            try
            {
                var smtpPassword = await SettingManager.GetSettingValueAsync(EmailSettingNames.Smtp.Password);

                var result = new EmailSettingsEditDto
                {
                    DefaultFromAddress =
                        await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.DefaultFromAddress,
                        AbpSession.TenantId.Value),
                    DefaultFromDisplayName =
                        await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.DefaultFromDisplayName,
                        AbpSession.TenantId.Value),
                    SmtpHost = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.Host,
                        AbpSession.TenantId.Value),
                    SmtpPort = await SettingManager.GetSettingValueForTenantAsync<int>(EmailSettingNames.Smtp.Port,
                        AbpSession.TenantId.Value),
                    SmtpUserName = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.UserName,
                        AbpSession.TenantId.Value),
                    SmtpPassword = SimpleStringCipher.Instance.Decrypt(smtpPassword),
                    SmtpDomain = await SettingManager.GetSettingValueForTenantAsync(EmailSettingNames.Smtp.Domain,
                        AbpSession.TenantId.Value)
                };
                return result;
            }
            catch (Exception e)
            {
                return new EmailSettingsEditDto();
            }
        }

        private async Task<TimeScheduleCheckBillSettingsEditDto> GetTimeScheduleCheckBill()
        {
            try
            {
                return new TimeScheduleCheckBillSettingsEditDto
                {
                    //EndPeriodDay = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.EndPeriodDay, AbpSession.TenantId.Value),
                    //HeadPeriodDay = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.HeadPeriodDay, AbpSession.TenantId.Value),

                    IsEnableCreateE = await SettingManager.GetSettingValueForTenantAsync<bool>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateE,
                        AbpSession.TenantId.Value),
                    IsEnableCreateW = await SettingManager.GetSettingValueForTenantAsync<bool>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateW,
                        AbpSession.TenantId.Value),
                    IsEnableCreateP = await SettingManager.GetSettingValueForTenantAsync<bool>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateP,
                        AbpSession.TenantId.Value),
                    IsEnableCreateM = await SettingManager.GetSettingValueForTenantAsync<bool>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateM,
                        AbpSession.TenantId.Value),

                    ElectricEndPeriodDay = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricEndPeriodDay,
                        AbpSession.TenantId.Value),
                    ElectricHeadPeriodDay = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricHeadPeriodDay,
                        AbpSession.TenantId.Value),

                    ManagerMonthNumber = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberM,
                        AbpSession.TenantId.Value),
                    ParkingMonthNumber = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberP,
                        AbpSession.TenantId.Value),


                    WaterEndPeriodDay = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.WaterEndPeriodDay,
                        AbpSession.TenantId.Value),
                    WaterHeadPeriodDay = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.WaterHeadPeriodDay,
                        AbpSession.TenantId.Value),

                    ParkingCreateDay = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.ParkingCreateDay, AbpSession.TenantId.Value),
                    ManagerCreateDay = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.TimeScheduleCheckBill.ManagerCreateDay, AbpSession.TenantId.Value),

                    BillNotificationTime1 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime1, AbpSession.TenantId.Value),
                    BillNotificationTime2 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime2, AbpSession.TenantId.Value),
                    BillNotificationTime3 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime3, AbpSession.TenantId.Value),

                    BillDebtNotificationTime1 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime1, AbpSession.TenantId.Value),
                    BillDebtNotificationTime2 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime2, AbpSession.TenantId.Value),
                    BillDebtNotificationTime3 = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime3, AbpSession.TenantId.Value),

                };
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<BankInfoDto> GetBankInfo()
        {
            try
            {
                var bankInfo = await SettingManager.GetSettingValueForTenantAsync(
                          AppSettings.TenantManagement.BankTransfer.BankInfo, AbpSession.TenantId.Value);
                return new BankInfoDto() { BankInfo = bankInfo };
                  
               
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public async Task<int> GetParkingPriceType()
        {
            try
            {
                var data = await SettingManager.GetSettingValueForTenantAsync<int>(
                          AppSettings.TenantManagement.UserBillConfig.ParkingBillType, AbpSession.TenantId.Value);
                return data;

            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task<UserBillSettingsEditDto> GetUserBillSettings()
        {
            try
            {
                return new UserBillSettingsEditDto
                {
                    DueDate = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueDate, AbpSession.TenantId.Value),
                    DueMonth = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueMonth, AbpSession.TenantId.Value),
                    DueDateElectric = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueDateElectric, AbpSession.TenantId.Value),
                    DueMonthElectric = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueMonthElectric, AbpSession.TenantId.Value),
                    DueDateWater = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueDateWater, AbpSession.TenantId.Value),
                    DueMonthWater = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueMonthWater, AbpSession.TenantId.Value),
                    DueDateParking = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueDateParking, AbpSession.TenantId.Value),
                    DueMonthParking = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueMonthParking, AbpSession.TenantId.Value),
                    DueDateLighting = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueDateLighting, AbpSession.TenantId.Value),
                    DueMonthLighting = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueMonthLighting, AbpSession.TenantId.Value),
                    DueDateManager = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueDateManager, AbpSession.TenantId.Value),
                    DueMonthManager = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueMonthManager, AbpSession.TenantId.Value),
                    DueDateResidence = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueDateResidence, AbpSession.TenantId.Value),
                    DueMonthResidence = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.DueMonthResidence, AbpSession.TenantId.Value),
                    ParkingBillType = await SettingManager.GetSettingValueForTenantAsync<int>(
                        AppSettings.TenantManagement.UserBillConfig.ParkingBillType, AbpSession.TenantId.Value),
                    // SendUserBillNotificationDay = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.TenantManagement.UserBillConfig.SendUserBillNotificationDay, AbpSession.TenantId.Value),
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        private async Task<BankTransferSettingDto> GetBankTransfers()
        {
            try
            {
                return new BankTransferSettingDto
                {
                    BankCode = await SettingManager.GetSettingValueForTenantAsync(AppSettings.TenantManagement.BankTransfer.BankCode, AbpSession.TenantId.Value),
                    BankNumber = await SettingManager.GetSettingValueForTenantAsync(AppSettings.TenantManagement.BankTransfer.BankNumber, AbpSession.TenantId.Value),
                    QRCode = await SettingManager.GetSettingValueForTenantAsync(AppSettings.TenantManagement.BankTransfer.QRCode, AbpSession.TenantId.Value),
                    BankInfo = await SettingManager.GetSettingValueForTenantAsync(AppSettings.TenantManagement.BankTransfer.BankInfo, AbpSession.TenantId.Value),
                };
            }
            catch (Exception e)
            {
                return null;
            }
        }

        #endregion

        #region Update Settings

        public async Task UpdateAllTenantSettings(TenantSettingsEditDto input)
        {
            //await UpdateEmailSettingsAsync(input.Email);
            //await UpdateGeneralSettingsAsync(input.General);
            await UpdateTimeScheduleCheckBill(input.TimeScheduleCheckBill);
            await UpdateUserBillSettingsAsync(input.UserBillSetting);
        }

        public async Task UpdateBankInfo(BankInfoDto input)
        {
            await UpdateBankTransfer(input);
        }

        public async Task<BankTransferSettingDto> GetAllTransferInfo()
        {
            var transferInfo = await GetBankTransfers();
            return transferInfo;
        }

        private async Task UpdateGeneralSettingsAsync(GeneralSettingsEditDto settings)
        {
            if (Clock.SupportsMultipleTimezone)
            {
                if (settings.Timezone.IsNullOrEmpty())
                {
                    var defaultValue =
                        await _timeZoneService.GetDefaultTimezoneAsync(SettingScopes.Application, AbpSession.TenantId);
                    await SettingManager.ChangeSettingForApplicationAsync(TimingSettingNames.TimeZone, defaultValue);
                }
                else
                {
                    await SettingManager.ChangeSettingForApplicationAsync(TimingSettingNames.TimeZone,
                        settings.Timezone);
                }
            }
        }

        public async Task UpdateEmailSettingsAsync(EmailSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.DefaultFromAddress,
                input.DefaultFromAddress);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.DefaultFromDisplayName,
                input.DefaultFromDisplayName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.Smtp.Host, input.SmtpHost);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.Smtp.Port,
                input.SmtpPort +"");
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.Smtp.UserName, input.SmtpUserName);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.Smtp.Password,
                SimpleStringCipher.Instance.Encrypt(input.SmtpPassword));
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.Smtp.Domain, input.SmtpDomain);
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.Smtp.EnableSsl,
               "true");
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, EmailSettingNames.Smtp.UseDefaultCredentials,
                "false");
        }

        private async Task UpdateTimeScheduleCheckBill(TimeScheduleCheckBillSettingsEditDto input)
        {
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.TimeScheduleCheckBill.HeadPeriodDay, input.HeadPeriodDay.ToString());
            //await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.TimeScheduleCheckBill.EndPeriodDay, input.EndPeriodDay.ToString());

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
               AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateE,
               input.IsEnableCreateE.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
               AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateW,
               input.IsEnableCreateW.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
              AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateP,
              input.IsEnableCreateP.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
               AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateM,
               input.IsEnableCreateM.ToString());

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricHeadPeriodDay,
                input.ElectricHeadPeriodDay.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricEndPeriodDay,
                input.ElectricEndPeriodDay.ToString());

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.TimeScheduleCheckBill.WaterHeadPeriodDay,
                input.WaterHeadPeriodDay.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.TimeScheduleCheckBill.WaterEndPeriodDay,
                input.WaterEndPeriodDay.ToString());

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.TimeScheduleCheckBill.ParkingCreateDay, input.ParkingCreateDay.ToString());

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
               AppSettings.TenantManagement.TimeScheduleCheckBill.ManagerCreateDay, input.ManagerCreateDay.ToString());

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
              AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberP, input.ParkingMonthNumber.ToString());

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
               AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberM, input.ManagerMonthNumber.ToString());

            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime1, input.BillDebtNotificationTime1.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime2, input.BillDebtNotificationTime2.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime3, input.BillDebtNotificationTime3.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime1,
                input.BillNotificationTime1.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime2,
                input.BillNotificationTime2.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime3,
                input.BillNotificationTime3.ToString());
        }

        private async Task UpdateUserBillSettingsAsync(UserBillSettingsEditDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.ParkingBillType, input.ParkingBillType.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueDate, input.DueDate.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueMonth, input.DueMonth.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueDateElectric, input.DueDateElectric.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueMonthElectric, input.DueMonthElectric.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueDateWater, input.DueDateWater.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueMonthWater, input.DueMonthWater.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueDateParking, input.DueDateParking.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueMonthParking, input.DueMonthParking.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueDateLighting, input.DueDateLighting.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueMonthLighting, input.DueMonthLighting.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueDateManager, input.DueDateManager.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueMonthManager, input.DueMonthManager.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueDateResidence, input.DueDateResidence.ToString());
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.DueMonthResidence, input.DueMonthResidence.ToString());
            //  await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value, AppSettings.TenantManagement.UserBillConfig.SendUserBillNotificationDay, input.SendUserBillNotificationDay.ToString());
        }

        private async Task UpdateBankTransfer(BankInfoDto input)
        {
            await SettingManager.ChangeSettingForTenantAsync(AbpSession.TenantId.Value,
                AppSettings.TenantManagement.BankTransfer.BankInfo, input.BankInfo.ToString());
        }

        #endregion
    }
}