using Abp.MultiTenancy;
using Yootek.Authorization.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Yootek.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public const string NTenancyNameRegex = "^[a-zA-Z0-9_-]{1,}$";

        public string MobileConfig { get; set; }
        public string AdminPageConfig { get; set; }

        [StringLength(128)] public string SubName { get; set; }

        public TenantType? TenantType { get; set; }

        public Tenant()
        {
            var mbConfig = new MobileConfig();
            mbConfig.MobileVersion = "";
            MobileConfig = JsonSerializer.Serialize(mbConfig);
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }

    public enum TenantType
    {
        IOC = 2,
        GOVERNMENT = 3,
        FORUM = 4,
        ERP = 5
    }
}