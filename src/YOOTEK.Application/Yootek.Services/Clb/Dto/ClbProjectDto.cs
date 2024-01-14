using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.App.ServiceHttpClient.Dto.Social.Forum;
using Yootek.Common;
using Yootek.Yootek.EntityDb.Clb.Projects;

namespace Yootek.Yootek.Services.Yootek.Clb.Dto
{
    public class ProjectDto : FullAuditedEntity<long>
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public EProjectType? Type { get; set; }
        public EProjectStatus? Status { get; set; }
        public int? ParentId { get; set; }
        public long? ManagerId { get; set; }
        public long? Capital { get; set; }
        public EProjectStage? Stage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public long? CountPost { get; set; }
        public ShortenedUserDto? Creator { get; set; }
    }

    public class CreateProjectDto
    {
        public int? TenantId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public EProjectType? Type { get; set; }
        public EProjectStatus? Status { get; set; }
        public int? ParentId { get; set; }
        public long? ManagerId { get; set; }
        public long? Capital { get; set; }
        public EProjectStage? Stage { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    
    public class UpdateProjectDto : Entity<long>
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Code { get; set; }
        public EProjectType? Type { get; set; }
        public EProjectStatus? Status { get; set; }
        public long? ManagerId { get; set; }
        public long? Capital { get; set; }
        public EProjectStage? Stage { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }   
    }
    
    public class GetAllProjectDto : CommonInputDto
    {
        public EProjectType? Type { get; set; }
        public EProjectStatus? Status { get; set; }
        public EProjectStage? Stage { get; set; }
        public DateTime? StartDate { get; set; }
        public OrderByProject? OrderBy { get; set; }
    }
    
    public enum OrderByProject
    {
        [YootekServiceBase.FieldNameAttribute("Name")]
        NAME = 1,
        [YootekServiceBase.FieldNameAttribute("CreationTime")]
        CREATION_TIME = 2,
    }
}