using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace Yootek.Yootek.EntityDb.Clb.Enterprise
{
    [Table("ClbEnterprises")]
    public class Enterprises : FullAuditedEntity<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int? DistrictId { get; set; }
        public int? ProvinceId { get; set; }
        public int? WardId { get; set; }
        public EnterpriseType Type { get; set; }
        public EnterpriseStatus Status { get; set; } = EnterpriseStatus.Active;
        public int BusinessField { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? OwnerName { get; set; }
        public string ImageUrl { get; set; }
        public DateTime FoundedDate { get; set; }
        public string? TaxCode { get; set; }
    }

    public enum EnterpriseType
    {
        SoleProprietorship = 1,
        Partnership = 2,
        GeneralPartnership = 3,
        JoinedVenture = 4,
        Company = 5,
        LimitedLiabilityCompany = 6,
        NonProfit = 7,
        BenefitCorporation = 8,
        BusinessFunding = 9,
        SocialEnterprise = 10,
    }
    
    public enum EnterpriseStatus
    {
        Active = 1,
        Inactive = 2
    }
}