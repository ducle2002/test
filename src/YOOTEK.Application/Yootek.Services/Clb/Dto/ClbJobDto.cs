using System;
using Abp.Domain.Entities;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.Common;
using Yootek.Yootek.EntityDb.Clb.Jobs;

namespace Yootek.Yootek.Services.Yootek.Clb.Dto
{
    public class JobDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public long? Salary { get; set; }
        public string Position { get; set; }
        public string? Requirement { get; set; }
        public string Benefit { get; set; }
        public string EnterpriseName { get; set; }
        public string EnterpriseAddress { get; set; }
        public string? EnterprisePhone { get; set; }
        public string? EnterpriseEmail { get; set; }
        public EJobsType? Type { get; set; }
        public EJobsStatus? Status { get; set; }  
        public long? CountPost { get; set; }
        public ShortenedUserDto? Creator { get; set; }
        public DateTime? CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }

    public class CreateJobDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public long? Salary { get; set; }
        public string Position { get; set; }
        public string? Requirement { get; set; }
        public string Benefit { get; set; }
        public string EnterpriseName { get; set; }
        public string EnterpriseAddress { get; set; }
        public string? EnterprisePhone { get; set; }
        public string? EnterpriseEmail { get; set; }
        public EJobsType? Type { get; set; }
        public EJobsStatus? Status { get; set; }
    }
    
    public class UpdateJobDto : Entity<long>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public long? Salary { get; set; }
        public string? Position { get; set; }
        public string? Requirement { get; set; }
        public string? Benefit { get; set; }
        public string? EnterpriseName { get; set; }
        public string? EnterpriseAddress { get; set; }
        public string? EnterprisePhone { get; set; }
        public string? EnterpriseEmail { get; set; }
        public EJobsType? Type { get; set; }
        public EJobsStatus? Status { get; set; }
    }

    public class GetAllJobDto : CommonInputDto
    {
        public long? Salary { get; set; }
        public EJobsType? Type { get; set; }
        public EJobsStatus? Status { get; set; }
        public OrderByJob? OrderBy { get; set; }
    }
    
    public enum OrderByJob
    {
        [YootekServiceBase.FieldNameAttribute("Name")]
        NAME = 1,
        [YootekServiceBase.FieldNameAttribute("CreationTime")]
        CREATION_TIME = 2,
        [YootekServiceBase.FieldNameAttribute("LastModificationTime")]
        LAST_MODIFICATION_TIME = 3,
        [YootekServiceBase.FieldNameAttribute("Salary")]
        SALARY = 4,
    }
}