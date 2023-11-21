using IMAX.Common;
using IMAX.Services.Dto;
using System;
using IMAX.Common.Enum;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services
{

    public class GetAllForumInput : CommonInputDto
    {

        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; }
        public int? Type { get; set; }
        public int? TypeTitle { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public bool? IsAdminAnswered { get; set; }
        public OrderByForum? OrderBy { get; set; }
        public CommonENumForum.FORUM_STATE? State { get; set; }
    }

    public enum OrderByForum
    {
        [FieldName("CreationTime")]
        CREATION_TIME = 1,
        [FieldName("ThreadTitle")]
        THREAD_TITLE = 2,
    }
}
