using Yootek.EntityDb;
using System;

namespace Yootek.Services.Dto
{
    public class FamilyDto
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public long? AccountId { get; set; }
        public string ImageUrl { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ApartmentCode { get; set; }
        public string BuildingCode { get; set; }
        public int? Type { get; set; }
        public string OtherPhones { get; set; }
        public int? BirthYear { get; set; }
        public RELATIONSHIP? RelationShip { get; set; }
        public int? MemberNum { get; set; }
        public string Career { get; set; }

    }
}
