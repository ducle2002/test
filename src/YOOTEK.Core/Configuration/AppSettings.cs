using System.Collections.Generic;

namespace Yootek.Configuration
{
    /// <summary>
    /// Defines string constants for setting names in the application.
    /// See <see cref="AppSettingProvider"/> for setting definitions.
    /// </summary>
    public static class AppSettings
    {
        public static class UserManagement
        {
            public static class TwoFactorLogin
            {
                public const string IsGoogleAuthenticatorEnabled =
                    "App.UserManagement.TwoFactorLogin.IsGoogleAuthenticatorEnabled";
            }

            public static class SessionTimeOut
            {
                public const string IsEnabled = "App.UserManagement.SessionTimeOut.IsEnabled";
                public const string TimeOutSecond = "App.UserManagement.SessionTimeOut.TimeOutSecond";

                public const string ShowTimeOutNotificationSecond =
                    "App.UserManagement.SessionTimeOut.ShowTimeOutNotificationSecond";

                public const string ShowLockScreenWhenTimedOut =
                    "App.UserManagement.SessionTimeOut.ShowLockScreenWhenTimedOut";
            }

            public const string AllowSelfRegistration = "App.UserManagement.AllowSelfRegistration";

            public const string IsNewRegisteredUserActiveByDefault =
                "App.UserManagement.IsNewRegisteredUserActiveByDefault";

            public const string UseCaptchaOnRegistration = "App.UserManagement.UseCaptchaOnRegistration";
            public const string UseCaptchaOnLogin = "App.UserManagement.UseCaptchaOnLogin";
            public const string SmsVerificationEnabled = "App.UserManagement.SmsVerificationEnabled";
            public const string IsCookieConsentEnabled = "App.UserManagement.IsCookieConsentEnabled";
            public const string IsQuickThemeSelectEnabled = "App.UserManagement.IsQuickThemeSelectEnabled";
            public const string AllowOneConcurrentLoginPerUser = "App.UserManagement.AllowOneConcurrentLoginPerUser";

            public const string AllowUsingGravatarProfilePicture =
                "App.UserManagement.AllowUsingGravatarProfilePicture";

            public const string UseGravatarProfilePicture = "App.UserManagement.UseGravatarProfilePicture";
        }

        public static class TenantManagement
        {
            public static class TimeScheduleCheckBill
            {
                public static string HeadPeriodDay = "Tenant.TimeScheduleCheckBill.HeadPeriod";
                public static string EndPeriodDay = "Tenant.TimeScheduleCheckBill.EndPeriod";

                public static string IsEnableCreateE = "Tenant.TimeScheduleCheckBill.IsEnableCreateE";
                public static string ElectricHeadPeriodDay = "Tenant.TimeScheduleCheckBill.ElectricHeadPeriod";
                public static string ElectricEndPeriodDay = "Tenant.TimeScheduleCheckBill.ElectricEndPeriod";

                public static string IsEnableCreateW = "Tenant.TimeScheduleCheckBill.IsEnableCreateW";
                public static string WaterHeadPeriodDay = "Tenant.TimeScheduleCheckBill.WaterHeadPeriod";
                public static string WaterEndPeriodDay = "Tenant.TimeScheduleCheckBill.WaterEndPeriod";

                public static string IsEnableCreateP = "Tenant.TimeScheduleCheckBill.IsEnableCreateP";
                public static string ParkingCreateDay = "Tenant.TimeScheduleCheckBill.ParkingCreateDay";
                public static string MonthNumberP = "Tenant.TimeScheduleCheckBill.MonthNumberP";

                public static string IsEnableCreateM = "Tenant.TimeScheduleCheckBill.IsEnableCreateM";
                public static string ManagerCreateDay = "Tenant.TimeScheduleCheckBill.ManagerCreateDay";
                public static string MonthNumberM = "Tenant.TimeScheduleCheckBill.MonthNumberM";

                public static string BillNotificationTime1 = "Tenant.TimeScheduleCheckBill.BillNotificationTime1";
                public static string BillNotificationTime2 = "Tenant.TimeScheduleCheckBill.BillNotificationTime2";
                public static string BillNotificationTime3 = "Tenant.TimeScheduleCheckBill.BillNotificationTime3";

                public static string BillDebtNotificationTime1 = "Tenant.TimeScheduleCheckBill.BillDebtNotificationTime1";
                public static string BillDebtNotificationTime2 = "Tenant.TimeScheduleCheckBill.BillDebtNotificationTime2";
                public static string BillDebtNotificationTime3 = "Tenant.TimeScheduleCheckBill.BillDebtNotificationTime3";
            }

            public static class UserBillConfig
            {
                public static string DueDate = "UserBillConfig.DueDate";
                public static string DueMonth = "UserBillConfig.DueMonth";

                public static string DueDateElectric = "UserBillConfig.DueDateElectric";
                public static string DueMonthElectric = "UserBillConfig.DueMonthElectric";

                public static string DueDateWater = "UserBillConfig.DueDateWater";
                public static string DueMonthWater = "UserBillConfig.DueMonthWater";

                public static string DueDateParking = "UserBillConfig.DueDateParking";
                public static string DueMonthParking = "UserBillConfig.DueMonthParking";

                public static string DueDateLighting = "UserBillConfig.DueDateLighting";
                public static string DueMonthLighting = "UserBillConfig.DueMonthLighting";

                public static string DueDateManager = "UserBillConfig.DueDateManager";
                public static string DueMonthManager = "UserBillConfig.DueMonthManager";

                public static string DueDateResidence = "UserBillConfig.DueDateResidence";
                public static string DueMonthResidence = "UserBillConfig.DueMonthResidence";

                public static string SendUserBillNotificationDay = "UserBillConfig.SendUserBillNotificationDay";

                public static string ParkingBillType = "UserBillConfig.ParkingBillType";
            }

            public static class BankTransfer
            {
                public static string BankCode = "BankTransfer.BankCode";
                public static string BankNumber = "BankTransfer.BankNumber";
                public static string QRCode = "BankTransfer.QRCode";
                public static string BankInfo = "BankTransfer.BankInfo";
            }
        }

        public static class UserConfig
        {
            public static class UserAddress
            {
                public static string FullName = "UserConfig.FullName";
                public static string PhoneNumber = "UserConfig.PhoneNumber";
                public static string Email = "UserConfig.Email";
                public static string Address = "UserConfig.Address";
                public static string ProvinceCode = "UserConfig.ProvinceCode";
                public static string DistrictCode = "UserConfig.DistrictCode";
                public static string WardCode = "UserConfig.WardCode";
                public static string Latitude = "UserConfig.Latitude";
                public static string Longitude = "UserConfig.Longitude";
            }
        }

        public static class ExternalLoginProvider
        {
            public const string OpenIdConnectMappedClaims = "ExternalLoginProvider.OpenIdConnect.MappedClaims";
            public const string WsFederationMappedClaims = "ExternalLoginProvider.WsFederation.MappedClaims";

            public static class Host
            {
                public const string Facebook = "ExternalLoginProvider.Facebook";
                public const string Google = "ExternalLoginProvider.Google";
                public const string Twitter = "ExternalLoginProvider.Twitter";
                public const string Microsoft = "ExternalLoginProvider.Microsoft";
                public const string OpenIdConnect = "ExternalLoginProvider.OpenIdConnect";
                public const string WsFederation = "ExternalLoginProvider.WsFederation";
            }

            public static class Tenant
            {
                public const string Facebook = "ExternalLoginProvider.Facebook.Tenant";
                public const string Facebook_IsDeactivated = "ExternalLoginProvider.Facebook.IsDeactivated";
                public const string Google = "ExternalLoginProvider.Google.Tenant";
                public const string Google_IsDeactivated = "ExternalLoginProvider.Google.IsDeactivated";
                public const string Twitter = "ExternalLoginProvider.Twitter.Tenant";
                public const string Twitter_IsDeactivated = "ExternalLoginProvider.Twitter.IsDeactivated";
                public const string Microsoft = "ExternalLoginProvider.Microsoft.Tenant";
                public const string Microsoft_IsDeactivated = "ExternalLoginProvider.Microsoft.IsDeactivated";
                public const string OpenIdConnect = "ExternalLoginProvider.OpenIdConnect.Tenant";
                public const string OpenIdConnect_IsDeactivated = "ExternalLoginProvider.OpenIdConnect.IsDeactivated";
                public const string WsFederation = "ExternalLoginProvider.WsFederation.Tenant";
                public const string WsFederation_IsDeactivated = "ExternalLoginProvider.WsFederation.IsDeactivated";
            }
        }
    }
}