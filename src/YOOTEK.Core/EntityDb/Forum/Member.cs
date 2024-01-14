using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.EntityDb;

namespace Yootek.Yootek.EntityDb.Forum
{
    public enum ClbMemberState
    {
        New = 0,
        Accepted = 1,
        Refuse = 2,
        Disable = 3,
    }
    public enum ClbMemberType
    {
        Member = 0,
        Admin = 1,
        Moderator = 2,
    }
    
    [Table("ClbMembers")]
    public class Member : FullAuditedEntity<long>, IMayHaveTenant
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? AccountId { get; set; }
        public string Nationality { get; set; }
        public string IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string ImageUrl { get; set; }
        public string Gender { get; set; }
        public ClbMemberState? State { get; set; }
        public int? Type { get; set; }
        public string OtherPhones { get; set; }
        [Column("YearBirthday")]
        public int? BirthYear { get; set; }
        public string? Career { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }
}