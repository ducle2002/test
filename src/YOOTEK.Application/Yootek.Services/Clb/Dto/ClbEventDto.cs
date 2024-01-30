using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Yootek.Common;
using Yootek.Yootek.EntityDb.Clb.Event;
using Yootek.Service;
using Yootek.Services;

namespace Yootek.Yootek.Services.Yootek.Clb.Dto
{
    public class EventDto
    {
    }

    public enum EOrderByEvent
    {
        [YootekServiceBase.FieldNameAttribute("Name")]
        NAME = 1,
        [YootekServiceBase.FieldNameAttribute("CreationTime")]
        CREATION_TIME = 2,
    }
    
    #region MyRegion

        public class ClbEventDto : ClbEvent
        {
            public long CountComment { get; set; }
            public long CountFollow { get; set; }
            public bool? IsFollow { get; set; }
            public MemberShortenedDto? Creator { get; set; }
        }

        public class ClbEventCommentDto : ClbEventComment
        {
            public MemberShortenedDto? Creator { get; set; }
        }
        
        public class CreateClbEventCommentDto
        {
            public string Comment { get; set; }
            public bool? IsLike { get; set; }
            public int? TenantId { get; set; }
            public long EventId { get; set; }
            public int? Type { get; set; }
        }
        
        public class UpdateClbEventCommentDto
        {
            public long Id { get; set; }
            public string Comment { get; set; }
            public bool? IsLike { get; set; }
            public int? TenantId { get; set; }
            public long EventId { get; set; }
            public int? Type { get; set; }
        }
        
        public class CreateClbEventFollowDto
        {
            public bool? IsLike { get; set; }
            public int? TenantId { get; set; }
            public long EventId { get; set; }
            public int? Type { get; set; }
        }
        
        public class UpdateClbEventFollowDto
        {
            public long Id { get; set; }
            public bool? IsLike { get; set; }
            public int? TenantId { get; set; }
            public long EventId { get; set; }
            public int? Type { get; set; }
        }

        public class ClbEventInput : CommonInputDto
        {
            public bool? IsAllowComment { get; set; }
            public DateTime? FromDay { get; set; }
            public DateTime? ToDay { get; set; }
            public EOrderByEvent OrderBy { get; set; }
            public bool? IsFollow { get; set; }
        }
        
        public class GetClbEventCommentInput : CommonInputDto
        {
            public long NotificationId { get; set; }
            public int Type { get; set; }
            public DateTime? FromDay { get; set; }
            public DateTime? ToDay { get; set; }
            public OrderByComment? OrderBy { get; set; }
        }

        public class CreateClbEvent
        {
            public int? TenantId { get; set; }
            [StringLength(2000)]
            public string Name { get; set; }
            public string Description { get; set; }
            [StringLength(2000)]
            public string? FileUrl { get; set; }
            public List<string> AttachUrls { get; set; }
            public bool? IsAllowComment { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string? Location { get; set; }
            public string? Organizer { get; set; }
        }
        
        public class UpdateClbEvent
        {
            public long Id { get; set; }
            [StringLength(2000)]
            public string Name { get; set; }
            public string Description { get; set; }
            [StringLength(2000)]
            public string? FileUrl { get; set; }
            public List<string> AttachUrls { get; set; }
            public bool? IsAllowComment { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string? Location { get; set; }
            public string? Organizer { get; set; }
        }

        #endregion
}