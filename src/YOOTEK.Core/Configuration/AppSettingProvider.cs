using Abp.Configuration;
using Abp.Localization;
using Abp.Net.Mail;
using Abp.Zero.Configuration;
using System.Collections.Generic;
using System.Configuration;

namespace Yootek.Configuration
{
    public class AppSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            context.Manager.GetSettingDefinition(AbpZeroSettingNames.UserManagement.TwoFactorLogin.IsEnabled).DefaultValue = false.ToString().ToLowerInvariant();
            ChangeEmailSettingScopes(context);
            return new[]
            {
                new SettingDefinition(AppSettingNames.UiTheme, "red", scopes: SettingScopes.Application | SettingScopes.Tenant | SettingScopes.User, isVisibleToClients: true),
                new SettingDefinition(
                           AbpZeroSettingNames.UserManagement.UserLockOut.IsEnabled,
                           "true",
                           new FixedLocalizableString("Is user lockout enabled."),
                           scopes: SettingScopes.Application | SettingScopes.Tenant,
                           clientVisibilityProvider: new VisibleSettingClientVisibilityProvider()
                           ),

                new SettingDefinition(
                           AbpZeroSettingNames.UserManagement.UserLockOut.MaxFailedAccessAttemptsBeforeLockout,
                           "10",
                           new FixedLocalizableString("Maxumum Failed access attempt count before user lockout."),
                           scopes: SettingScopes.Application | SettingScopes.Tenant,
                           clientVisibilityProvider: new VisibleSettingClientVisibilityProvider()
                           ),

                new SettingDefinition(
                           AbpZeroSettingNames.UserManagement.UserLockOut.DefaultAccountLockoutSeconds,
                           "300", //5 minutes
                           new FixedLocalizableString("User lockout in seconds."),
                           scopes: SettingScopes.Application | SettingScopes.Tenant,
                           clientVisibilityProvider: new VisibleSettingClientVisibilityProvider()
                           ),
                // Tenant config
                //new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.EndPeriodDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.EndPeriodDay] ?? "1", scopes: SettingScopes.Tenant),
                //new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.HeadPeriodDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.HeadPeriodDay] ?? "1", scopes: SettingScopes.Tenant),
               new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateP, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateP] ?? "false", scopes: SettingScopes.Tenant),
               new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateM, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateM] ?? "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateE, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateE] ?? "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricEndPeriodDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricEndPeriodDay] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricHeadPeriodDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.ElectricHeadPeriodDay] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateW, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.IsEnableCreateW] ?? "false", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.WaterEndPeriodDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.WaterEndPeriodDay] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.ParkingCreateDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.ParkingCreateDay] ?? "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.ManagerCreateDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.ManagerCreateDay] ?? "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberM, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberM] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberP, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.MonthNumberP] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.WaterHeadPeriodDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.WaterHeadPeriodDay] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime1, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime1] ?? "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime2, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime2] ?? "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime3, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.BillNotificationTime3] ?? "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime1, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime1] ?? "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime2, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime2] ?? "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime3, ConfigurationManager.AppSettings[AppSettings.TenantManagement.TimeScheduleCheckBill.BillDebtNotificationTime3] ?? "0", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueDate, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueDate] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueMonth, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueMonth] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueDateElectric, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueDateElectric] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueMonthElectric, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueMonthElectric] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueDateWater, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueDateWater] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueMonthWater, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueMonthWater] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueDateManager, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueDateManager] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueMonthManager, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueMonthManager] ?? "1", scopes: SettingScopes.Tenant),

                 new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.ParkingBillType, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.ParkingBillType] ?? "5", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueDateParking, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueDateParking] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueMonthParking, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueMonthParking] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueDateLighting, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueDateLighting] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueMonthLighting, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueMonthLighting] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueDateResidence, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueDateResidence] ?? "1", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueMonthResidence, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueMonthResidence] ?? "1", scopes: SettingScopes.Tenant),

                new SettingDefinition(AppSettings.TenantManagement.BankTransfer.BankCode, ConfigurationManager.AppSettings[AppSettings.TenantManagement.BankTransfer.BankCode] ?? "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.BankTransfer.BankNumber, ConfigurationManager.AppSettings[AppSettings.TenantManagement.BankTransfer.BankNumber] ?? "0", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.BankTransfer.QRCode, ConfigurationManager.AppSettings[AppSettings.TenantManagement.BankTransfer.QRCode] ?? "", scopes: SettingScopes.Tenant),
                new SettingDefinition(AppSettings.TenantManagement.BankTransfer.BankInfo, ConfigurationManager.AppSettings[AppSettings.TenantManagement.BankTransfer.BankInfo] ?? "", scopes: SettingScopes.Tenant),

               // new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.SendUserBillNotificationDay, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.SendUserBillNotificationDay] ?? "1", scopes: SettingScopes.Tenant),
                // new SettingDefinition(AppSettings.TenantManagement.UserBillConfig.DueDate, ConfigurationManager.AppSettings[AppSettings.TenantManagement.UserBillConfig.DueDate] ?? "", scopes: SettingScopes.Tenant),

                //User config
                new SettingDefinition(AppSettings.UserConfig.UserAddress.Latitude, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.Latitude] ?? "21", scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.UserConfig.UserAddress.Longitude, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.Longitude] ?? "105", scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.UserConfig.UserAddress.Address, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.Address] ?? "", scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.UserConfig.UserAddress.ProvinceCode, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.ProvinceCode] ?? "", scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.UserConfig.UserAddress.DistrictCode, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.DistrictCode] ?? "", scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.UserConfig.UserAddress.WardCode, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.WardCode] ?? "", scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.UserConfig.UserAddress.FullName, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.FullName] ?? "", scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.UserConfig.UserAddress.PhoneNumber, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.PhoneNumber] ?? "", scopes: SettingScopes.User),
                new SettingDefinition(AppSettings.UserConfig.UserAddress.Email, ConfigurationManager.AppSettings[AppSettings.UserConfig.UserAddress.Email] ?? "", scopes: SettingScopes.User),

            };
        }

        private void ChangeEmailSettingScopes(SettingDefinitionProviderContext context)
        {
            if (!YootekConsts.AllowTenantsToChangeEmailSettings)
            {
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.Host).Scopes = SettingScopes.Tenant;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.Port).Scopes = SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.UserName).Scopes =
                    SettingScopes.Tenant;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.Password).Scopes =
                    SettingScopes.Tenant;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.Domain).Scopes = SettingScopes.Tenant;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.EnableSsl).Scopes =
                    SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.Smtp.UseDefaultCredentials).Scopes =
                    SettingScopes.Application;
                context.Manager.GetSettingDefinition(EmailSettingNames.DefaultFromAddress).Scopes =
                    SettingScopes.Tenant;
                context.Manager.GetSettingDefinition(EmailSettingNames.DefaultFromDisplayName).Scopes =
                    SettingScopes.Tenant;

            }
        }

    }
}
