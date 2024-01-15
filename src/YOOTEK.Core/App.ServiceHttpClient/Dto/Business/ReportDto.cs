using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.Common;
using System.Collections.Generic;

namespace Yootek.App.ServiceHttpClient.Dto.Business
{
    public enum TypeReport
    {
        DEFAULT = 0,
        FAKE_ACCOUNT = 1,
        IMPERSONATE_SOMEONE = 2,
        INCONSONANT_POST = 3,
        NEED_SOME_HELP = 4,
        HAUNT_OR_BULLY = 5,
        OTHERS = 6
    }
    public enum ReportState
    {
        PENDING = 1,
        APPROVED = 2,
    }
    public class Report : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long ProviderId { get; set; }
        public TypeReport TypeReport { get; set; }
        public string ReportMessage { get; set; }
        public List<string> ImageUrls { get; set; }
        public bool IsStatic { get; set; }
        public ReportState State { get; set; }
    }
    public class JoinReportProvider : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long? ProviderId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int? ProviderState { get; set; }
        public string? StateProperties { get; set; }
        public bool? IsDataStatic { get; set; }
        public bool? IsAdminCreate { get; set; }
        public long? OwnerId { get; set; }
        public TypeReport? TypeReport { get; set; }
        public string? ReportMessage { get; set; }
        public List<string>? ImageUrls { get; set; }
        public bool? IsStatic { get; set; }
        public ReportState? ReportState { get; set; }
    }
    public class CreateReportDto
    {
        public long ProviderId { get; set; }
        public int TypeReport { get; set; }
        public string ReportMessage { get; set; }
        public List<string>? ImageUrls { get; set; }
        public bool IsStatic { get; set; }
    }
    public class GetAllReportsByAdminDto : CommonInputDto
    {
        public long? ProviderId { get; set; }
        public bool? IsStatic { get; set; }
        public int? TypeReport { get; set; }
        public int FormId { get; set; }
    }
    public class UpdateStateReportDto
    {
        public long Id { get; set; }
        public int UpdateState { get; set; }
        public int CurrentState { get; set; }
    }
    public class DeleteReportDto
    {
        public List<long> Ids { get; set; }
    }
}
