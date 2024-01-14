using Abp.Domain.Entities.Auditing;
using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.Yootek.EntityDb.Yootek.DichVu.Business
{
    [Table("Providers", Schema = "social_business")]
    public class Provider : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public List<string>? ImageUrls { get; set; }
        public string OwnerInfo { get; set; }
        public  string BusinessInfo { get; set; }
        public int? TenantId { get; set; }
        public int? SocialTenantId { get; set; }
        public int? Type { get; set; }
        public int? GroupType { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PropertyHistories { get; set; }
        public string Properties { get; set; }
        public bool? IsDataStatic { get; set; }
        public bool? IsAdminCreate { get; set; }
        public string DistrictId { get; set; }
        public string ProvinceId { get; set; }
        public string WardId { get; set; }
        public string Address { get; set; }
        public int? ServiceType { get; set; }
    }
}
