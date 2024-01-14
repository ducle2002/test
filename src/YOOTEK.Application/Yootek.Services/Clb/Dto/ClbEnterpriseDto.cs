using System;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.AutoMapper;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;
using Yootek.Yootek.EntityDb.Clb.Enterprise;
using JetBrains.Annotations;

namespace Yootek.Yootek.Services.Yootek.Clb.Dto
{
    public enum OrderByEnterprise
    {
        [YootekServiceBase.FieldNameAttribute("Name")]
        Name = 1,
        [YootekServiceBase.FieldNameAttribute("Status")]
        Status = 2,
        [YootekServiceBase.FieldNameAttribute("Type")]
        Type = 3,
        [YootekServiceBase.FieldNameAttribute("FoundedDate")]
        FoundedDate = 4,
        [YootekServiceBase.FieldNameAttribute("CreationTime")]
        CreationTime =5,
    }
    
    public enum OrderByEmployee
    {
        [YootekServiceBase.FieldNameAttribute("FullName")]
        FullName = 1,
        [YootekServiceBase.FieldNameAttribute("Role")]
        Role = 2,
        [YootekServiceBase.FieldNameAttribute("Status")]
        Status = 3,
        [YootekServiceBase.FieldNameAttribute("CreationTime")]
        CreationTime =4,
    }

    #region Enterprise
        public class EnterpriseDto : FullAuditedEntity<long>
    {
        public int? TenantId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int? DistrictId { get; set; }
        public int? ProvinceId { get; set; }
        public int? WardId { get; set; }
        public EnterpriseType? Type { get; set; }
        public EnterpriseStatus? Status { get; set; }
        public int? BusinessField { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? OwnerName { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? FoundedDate { get; set; }
        public string? TaxCode { get; set; }
        
        public long? MemberCount { get; set; }
        public long? ProjectCount { get; set; }
        public long? PostCount { get; set; }
    }

    [AutoMap(typeof(Enterprises))]
    public class CreateEnterpriseDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int? DistrictId { get; set; }
        public int? ProvinceId { get; set; }
        public int? WardId { get; set; }
        public EnterpriseType Type { get; set; }
        public int BusinessField { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? OwnerName { get; set; }
        public string ImageUrl { get; set; }
        public DateTime FoundedDate { get; set; }
        public string? TaxCode { get; set; }
    }

    [AutoMap(typeof(Enterprises))]
    public class UpdateEnterpriseDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public int? DistrictId { get; set; }
        public int? ProvinceId { get; set; }
        public int? WardId { get; set; }
        public EnterpriseType? Type { get; set; }
        public EnterpriseStatus? Status { get; set; }
        public int? BusinessField { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? OwnerName { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? FoundedDate { get; set; }
        public string? TaxCode { get; set; }
    }
    
    public class GetEnterpriseDto : CommonInputDto
    {
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public EnterpriseStatus? Status { get; set; }
        public EnterpriseType? Type { get; set; }
        public OrderByEnterprise? OrderBy { get; set; }
        public string Sorting { get; set; }
    }
    #endregion

    #region BusinessField
    public class BusinessFieldDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public long? EnterpriseCount { get; set; }
    }
    public class GetBusinessFieldDto : CommonInputDto
    {
    }
    [AutoMap(typeof(BusinessField))]
    public class CreateBusinessFieldDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
    [AutoMap(typeof(BusinessField))]
    public class UpdateBusinessFieldDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }

    }
    public class DeleteBusinessFieldDto
    {
        public int Id { get; set; }
    }
    #endregion

    #region UserEnterprise
    
    public class EmployeeDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long EnterpriseId { get; set; }
        public long MemberId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public UserEnterpriseRole? Role { get; set; }
        public UserEnterpriseStatus? Status { get; set; }
        public string? Description { get; set; }
    }
    
    public class GetAllEmployeeOfEnterpriseDto : CommonInputDto
    {
        public long EnterpriseId { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public EnterpriseStatus? Status { get; set; }
        public OrderByEmployee? OrderBy { get; set; }
    }
    
    public class AddMemberToEnterpriseDto
    {
        public long EnterpriseId { get; set; }
        public long MemberId { get; set; }
        public string? Description { get; set; }
        public UserEnterpriseRole? Role { get; set; } = UserEnterpriseRole.Employee;
    }
    
    [AutoMap(typeof(UserEnterprises))]
    public class UpdateEmployeeDto
    {
        public long Id { get; set; }
        public long EnterpriseId { get; set; }
        public long MemberId { get; set; }
        public string? Description { get; set; }
        public UserEnterpriseRole? Role { get; set; }
        public UserEnterpriseStatus? Status { get; set; }
    }
    
    public class LeaveEnterpriseDto
    {
        public long EnterpriseId { get; set; }
    }
    
    public class DeleteMemberOfEnterpriseDto
    {
        public long Id { get; set; }
    }


    #endregion
}