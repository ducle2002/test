using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("CitizenTemp")]
    public class CitizenTemp : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveBuilding, IMayHaveUrban
    {
        [StringLength(256)]
        public string FullName { get; set; }
        [StringLength(1000)]
        public string Address { get; set; }
        [StringLength(256)]
        public string Nationality { get; set; }
        [StringLength(256)]
        public string IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public string ImageUrl { get; set; }
        [StringLength(256)]
        public string PhoneNumber { get; set; }
        [StringLength(256)]
        public string Email { get; set; }
        [StringLength(50)]
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool IsVoter { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        public int? State { get; set; }
        public string BuildingCode { get; set; }
        public int Type { get; set; }
        public bool IsStayed { get; set; }
        public string OtherPhones { get; set; }
        [Column("YearBirthday")]
        public int BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public string UrbanCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        [StringLength(100)]
        public string TaxCode { get; set; }

        [StringLength(1000)]
        public string CitizenCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        [StringLength(256)]
        public string Career { get; set; }
        [Range(0, 256)]
        public int? OwnerGeneration { get; set; }
        public long? OwnerId { get; set; }
        public long? AccountId { get; set; }
        public int? CareerCategoryId { get; set; }

        [StringLength(1000)]
        public string? Hometown { get; set; }
    }
}
