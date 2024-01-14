using System;

namespace Yootek
{
    public class AppConsts
    {
        /// <summary>
        /// Default page size for paged requests.
        /// </summary>
        public const int DefaultPageSize = 10;

        /// <summary>
        /// Maximum allowed page size for paged requests.
        /// </summary>
        public const int MaxPageSize = 1000;

        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public const string DefaultPassPhrase = "gsKxGZ012HLL3MI5";

        /// <summary>
        /// Default type device string
        /// </summary>
        public const string LightingDevice = "Smart Lighting";
        public const string CurtainDevice = "Smart Curtain";
        public const string AirDevice = "Smart Air";
        public const string ConditionerDevice = "Smart Conditioner";
        public const string ConnectionDevice = "Smart Connection";
        public const string FireAlarmDevice = "Smart Fire Alarm";
        public const string DoorEntryDevice = "Smart Door Entry";
        public const string SecurityDevice = "Smart Security";
        public const string WaterDevice = "Smart Water";

        public const int ResizedMaxProfilPictureBytesUserFriendlyValue = 1024;

        public const int MaxProfilPictureBytesUserFriendlyValue = 5;

        public const string TokenValidityKey = "token_validity_key";
        public const string RefreshTokenValidityKey = "refresh_token_validity_key";
        public const string SecurityStampKey = "AspNet.Identity.SecurityStamp";

        public const string TokenType = "token_type";

        public static string UserIdentifier = "user_identifier";


        public const string ThemeDefault = "default";
        public const string Theme2 = "theme2";
        public const string Theme3 = "theme3";
        public const string Theme4 = "theme4";
        public const string Theme5 = "theme5";
        public const string Theme6 = "theme6";
        public const string Theme7 = "theme7";
        public const string Theme8 = "theme8";
        public const string Theme9 = "theme9";
        public const string Theme10 = "theme10";
        public const string Theme11 = "theme11";

        public static TimeSpan AccessTokenExpiration = TimeSpan.FromDays(31);
        public static TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(365);

        public const string DateTimeOffsetFormat = "yyyy-MM-ddTHH:mm:sszzz";

        public const string ReminderNotify = "app.reminderNotifier";

        public const int DeleteAccountDays = 7;
    }
    public class State_Object
    {
        public const int New = 1;
        public const int Active = 2;
        public const int Refuse = 3;
        public const int Disable = 4;
    }

    public class TenantConsts
    {
        public const string TenancyNameRegex = "^[a-zA-Z][a-zA-Z0-9_-]{1,}$";

        public const string DefaultTenantName = "Default";

        public const int MaxNameLength = 128;

        public const int DefaultTenantId = 1;
    }
}
