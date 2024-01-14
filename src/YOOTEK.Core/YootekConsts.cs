namespace Yootek
{
    public class YootekConsts
    {
        public const string LocalizationSourceName = "Yootek";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;

        public const int DefaultPageSize = 10;
        public const bool AllowTenantsToChangeEmailSettings = false;
        /// <summary>
        /// Maximum allowed page size for paged requests.
        /// </summary>
        public const int MaxPageSize = 1000;
    }
}
