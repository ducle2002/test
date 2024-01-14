using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Forum;
using Yootek.Services.Dto;

namespace Yootek.Service
{
    public enum GetMemberFormId
    {
        GetAll = 1,
        GetNew = 2,
        GetVerified = 3,
        GetUnVerified = 4,
        GetDisable = 5,
    }
    
    public enum OrderByMember
    {
        [YootekServiceBase.FieldNameAttribute("FullName")]
        FullName = 1,
        [YootekServiceBase.FieldNameAttribute("State")]
        State = 2,
        [YootekServiceBase.FieldNameAttribute("Type")]
        Type = 3,
        [YootekServiceBase.FieldNameAttribute("CreationTime")]
        CreationTime =4,
    }
    
    [AutoMap(typeof(Member))]
    public class ClbMemberDto : Member
    {
        
    }

    public class MemberShortenedDto
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string? ImageUrl { get; set; }
        public string? Email { get; set; }
        public int Type { get; set; }
    }
    
    [AutoMap(typeof(Member))]
    public class CreateMemberByAdminDto
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? AccountId { get; set; }
        public string? Nationality { get; set; }
        public string? IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? Gender { get; set; }
        public int? Type { get; set; }
        public string? OtherPhones { get; set; }
        [Column("YearBirthday")]
        public int? BirthYear { get; set; }
        public string? Career { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }

    [AutoMap(typeof(Member))]
    public class UpdateMemberByAdminDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? AccountId { get; set; }
        public string? Nationality { get; set; }
        public string? IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? Gender { get; set; }
        public ClbMemberState? State { get; set; }
        public int? Type { get; set; }
        public string? OtherPhones { get; set; }
        [Column("YearBirthday")]
        public int? BirthYear { get; set; }
        public string? Career { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }
    
    public class UpdateStateMemberByAdminDto
    {
        public int Id { get; set; }
        public ClbMemberState? State { get; set; }
    }

    public class CreateMemberByUserDto
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? AccountId { get; set; }
        public string? Nationality { get; set; }
        public string? IdentityNumber { get; set; }
        public int? TenantId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? Gender { get; set; }
        public string? OtherPhones { get; set; }
        [Column("YearBirthday")]
        public int? BirthYear { get; set; }
        public string? Career { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }

    public class UpdateMemberByUserDto
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public string? Gender { get; set; }
        public string? OtherPhones { get; set; }
        [Column("YearBirthday")]
        public int? BirthYear { get; set; }
        public string? Career { get; set; }
        public List<string>? IdentityImageUrls { get; set; }
    }

    public class GetAllMemberInput : CommonInputDto
    {
        public int FormId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public ClbMemberState? State { get; set; }
        public OrderByMember? OrderBy { get; set; }
        public int? FromAge { get; set; }
        public int? ToAge { get; set; }
        public string Sorting { get; set; }
    }
    
}