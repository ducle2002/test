using Yootek.Common;
using Yootek.Services.Dto;
using System;
using Yootek.Common.Enum;
using static Yootek.YootekServiceBase;

namespace Yootek.Services
{

    public class GetAllForumInput : CommonInputDto
    {

        public long? Id { get; set; }
        public int? FormId { get; set; }
        public long? TopicId { get; set; }
        public int? FormCase { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public OrderByForum? OrderBy { get; set; }
        public CommonENumForum.FORUM_STATE? State { get; set; }
        public int? Type { get; set; }
        public int? GroupId { get; set; }
        public long? UserId { get; set; }
    }

    public enum OrderByForum
    {
        [FieldName("CreationTime")]
        CREATION_TIME = 1,
        [FieldName("ThreadTitle")]
        THREAD_TITLE = 2,
    }
}
