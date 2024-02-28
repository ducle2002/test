﻿using Abp.MultiTenancy;
using Yootek.Authorization.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Yootek.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        public string MobileConfig { get; set; }
        public string AdminPageConfig { get; set; }

        [StringLength(128)] public string SubName { get; set; }

        public int? TenantType { get; set; }

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
}