using Yootek.Common;
using System;
using System.Collections.Generic;

namespace Yootek.App.ServiceHttpClient.Dto.Yootek.SmartCommunity.WorkDtos
{
    public class WorkCommentDto
    {
        public long Id { get; set; }
        public long WorkId { get; set; }
        public int? TenantId { get; set; }
        public string Content { get; set; }
        public List<string> ImageUrls { get; set; }
        public string? FullName { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
    }

    public class WorkCommentDetailDto
    {
        public long Id { get; set; }
        public int? TenantId { get; set; }
        public long WorkId { get; set; }
        public string Content { get; set; }
        public string? FullName { get; set; }
        public List<string> ImageUrls { get; set; }
        public DateTime CreationTime { get; set; }
        public long CreatorUserId { get; set; }
    }
    public class CreateWorkCommentDto
    {
        public long WorkId { get; set; }
        public string Content { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
    public class UpdateWorkCommentDto
    {
        public long Id { get; set; }
        public string? Content { get; set; }
        public List<string>? ImageUrls { get; set; }
    }
    public class DeleteWorkCommentDto
    {
        public long Id { get; set; }
    }
    public class GetAllWorkCommentDto : CommonInputDto
    {
        public long? WorkId { get; set; }
    }

    public class GetAllWorkCommentNotPagingDto : FilteredInputDto
    {
        public long? WorkId { get; set; }
    }
    public class GetWorkCommentDto
    {
        public long Id { get; set; }
    }
}
