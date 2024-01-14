namespace Yootek.Authorization.Accounts.Dto
{
    public class IsTenantAvailableOutput
    {
        public TenantAvailabilityState State { get; set; }

        public int? TenantId { get; set; }
        public string MobileConfig { get; set; }
        public string AdminPageConfig { get; set; }
        public string Permissions { get; set; }

        public IsTenantAvailableOutput()
        {
        }

        public IsTenantAvailableOutput(TenantAvailabilityState state, int? tenantId = null, string mobileConfig = null,
            string adminPageConfig = null)
        {
            State = state;
            TenantId = tenantId;
            MobileConfig = mobileConfig;
            AdminPageConfig = adminPageConfig;
        }
    }
}