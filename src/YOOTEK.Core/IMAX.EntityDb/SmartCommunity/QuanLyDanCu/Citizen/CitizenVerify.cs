using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    [Table("CitizenVerifications")]
    public class CitizenVerification : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit
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
        [StringLength(256)]
        public string PhoneNumber { get; set; }
        [StringLength(256)]
        public string Email { get; set; }
        [StringLength(50)]
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [StringLength(256)]
        public string ApartmentCode { get; set; }
        public int? State { get; set; }
        public string BuildingCode { get; set; }
        [Column("YearBirthday")]
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? CitizenTempId { get; set; }
        public int? MatchCount { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
    }
}
