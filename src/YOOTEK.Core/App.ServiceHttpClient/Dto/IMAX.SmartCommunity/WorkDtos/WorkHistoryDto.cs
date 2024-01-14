using Abp.Domain.Entities;
using Yootek.Common;
using System;
using System.Collections.Generic;

namespace Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos
{
    public class CreateWorkHistoryDto
    {
        public long CreatorId { get; set; }
        public List<long> RecipientIds { get; set; }
        public List<long> SupervisorIds { get; set; }
        public string Note { get; set; }
        public long WorkId { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
    public class DeleteWorkHistoryDto
    {
        public long Id { get; set; }
    }
    public class GetAllWorkHistoryDto : CommonInputDto
    {
        public long? WorkId { get; set; }
    }
    public class GetWorkHistoryByIdDto
    {
        public long Id { get; set; }
    }
    public class UpdateWorkHistoryDto : Entity<long>
    {
        public long? RecipientId { get; set; }
        public long? AssignerId { get; set; }
        public string? Note { get; set; }
        public List<string>? ImageUrls { get; set; }
        public DateTime? ReadTime { get; set; }
    }
    public class WorkHistoryDto : Entity<long>
    {
        public int? TenantId { get; set; }
        public long RecipientId { get; set; }
        public string? RecipientName { get; set; }
        public long AssignerId { get; set; }
        public string? AssignerName { get; set; }
        public long WorkId { get; set; }
        public string Note { get; set; }
        public List<string> ImageUrls { get; set; }
        public DateTime ReadTime { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
        public string? CreatorName { get; set; }
    }
}
