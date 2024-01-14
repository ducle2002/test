using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;

namespace Yootek.App.ServiceHttpClient.Dto.Social.Forum
{
    public class ReportDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long? TargetId { get; set; }
        public EReportType Type { get; set; }
        public EReportReason Reason { get; set; }
        public string? Description { get; set; }
        public DateTime? CreationTime { get; set; }
        public ShortenedUserDto? Creator { get; set; }
    }
    
    public class CreateReportDto
    {
        public long TargetId { get; set; }
        public EReportType Type { get; set; }
        public EReportReason Reason { get; set; }
        public string? Description { get; set; }
    }
    
    public class GetListReportDto : CommonInputDto
    {
        public EReportType? Type { get; set; }
        public EReportReason? Reason { get; set; }
    }
    
    public enum EReportType
    {
        Post = 1,
        Comment = 2,
        FanPage = 3,
        Group = 4,
        User = 5
    }

    public enum EReportReason
    {
        Spam = 1,
        Violence = 2,
        Harassment = 3,
        Fake = 4,
        Other = 5,
    }
}