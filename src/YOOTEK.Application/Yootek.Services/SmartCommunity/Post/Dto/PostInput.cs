using Yootek.Common;
using System;
using static Yootek.YootekServiceBase;

namespace Yootek.Services.Dto
{
    public class PostInput : CommonInputDto
    {
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public long? CreatorUserId { get; set; }
        public int? PostState { get; set; }
        public OrderByPost? OrderBy { get; set; }
    }

    public enum OrderByPost
    {
        [FieldName("FullName")]
        FULL_NAME = 1,
        [FieldName("CreationTime")]
        CREATION_TIME = 2
    }

    public class ReportCommentInput : CommonInputDto
    {
    }
}
