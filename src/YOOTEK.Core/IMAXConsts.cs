namespace IMAX
{
    public class IMAXConsts
    {
        public const string LocalizationSourceName = "IMAX";

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
