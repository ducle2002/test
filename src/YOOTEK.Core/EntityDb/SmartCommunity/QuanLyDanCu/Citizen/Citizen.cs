using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;

namespace Yootek.EntityDb
{
    public enum RELATIONSHIP
    {
        Contractor = 1,//Host
        Wife = 2,
        Husband = 3,
        Daughter = 4,
        Son = 5,
        Family = 6,
        Father = 7,
        Mother = 8,
        Grandfather = 9,
        Grandmother = 10,
        Guest = 11,
        Wife_Guest = 12,
        Husband_Guest = 13,
        Daughter_Guest = 14,
        Son_Guest = 15,
        Family_Guest = 16,
        Father_Guest = 17,
        Mother_Guest = 18,
        Grandfather_Guest = 19,
        Grandmother_Guest = 20,
        Staff_Guest = 21,
        Renter_Guest = 22,
    }

    public enum STATE_CITIZEN
    {
        NEW = 0, // user mới tạo
        ACCEPTED = 1,   // đã xác minh
        MATCHCHECK = 3,
        MISMATCH = 4,
        REFUSE = 5,  // yêu cầu sửa
        DISABLE = 6,  // yêu cầu bị admin hủy
        EDITED = 7,

    }

    [Table("Citizen")]
    public class Citizen : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveBuilding, IMayHaveUrban
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
        public long? AccountId { get; set; }

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
        public STATE_CITIZEN? State { get; set; }
        [StringLength(256)]
        public string BuildingCode { get; set; }
        public int? Type { get; set; }
        public bool? IsStayed { get; set; }
        public string OtherPhones { get; set; }
        [Column("YearBirthday")]
        public int? BirthYear { get; set; }
        public long? OrganizationUnitId { get; set; }
        public long? CitizenTempId { get; set; }
        [StringLength(1000)]
        public string? CitizenCode { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        [StringLength(256)]
        public string? Career { get; set; }
        [StringLength(256)]
        public string? UrbanCode { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
        [StringLength(1000)]
        public string? HomeAddress { get; set; }
    }
}
