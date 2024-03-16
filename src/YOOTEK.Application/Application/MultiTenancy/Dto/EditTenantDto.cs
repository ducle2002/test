using Abp.Application.Services.Dto;
using Abp.Auditing;
using Abp.MultiTenancy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yootek.MultiTenancy.Dto
{
    public class TenantEditDto : EntityDto
    {
        [Required]
        [StringLength(AbpTenantBase.MaxTenancyNameLength)]
        public string TenancyName { get; set; }

        [Required]
        [StringLength(TenantConsts.MaxNameLength)]
        public string Name { get; set; }

        [DisableAuditing] public string ConnectionString { get; set; }

        public int? EditionId { get; set; }

        public bool IsActive { get; set; }
        public TenantType? TenantType { get; set; }

        public DateTime? SubscriptionEndDateUtc { get; set; }

        public bool IsInTrialPeriod { get; set; }
    }

    public class UpdateConfigTenantDto
    {
        public int Id { get; set; }

        //    public MobileConfigDto MobileConfig { get; set; }
        public string MobileConfig { get; set; }
        public string AdminPageConfig { get; set; }
    }
}