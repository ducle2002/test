using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public enum TheXeStatus
    {
        [Display(Name = "Hoạt động")]
        Active = 1,

        [Display(Name = "Khóa")]
        Locked = 2,

        [Display(Name = "Hủy")]
        Canceled = 3,
    }

    [Table("TheXe")]
    public class TheXe : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
    {
        public int? TenantId { get; set; }

        [StringLength(1000)]
        public string BuildingCode { get; set; }

        public long? OrganizationUnitId { get; set; }

        public string Code { get; set; }

        [StringLength(1000)]
        public string? ApartmentOwnerName { get; set; }

        public string? RelationShipWithApartmentOwner { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(1000)]
        public string? Email { get; set; }

        [StringLength(1000)]
        public string ApartmentCode { get; set; }

        [StringLength(20)]
        public string LicensePlate { get; set; }

        public string? Description { get; set; }

        public TheXeStatus Status { get; set; }
    }
}
