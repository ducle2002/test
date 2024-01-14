using Abp.AutoMapper;
using Yootek.Common;
using Yootek.Common.Enum;
using Yootek.EntityDb;
using Yootek.Yootek.EntityDb.Forum;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Yootek.Yootek.EntityDb.Clb.QnA;
using static Yootek.YootekServiceBase;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(ForumPost))]
    public class ClbForumInputDto : ClbForum
    {

    }

    [AutoMap(typeof(ClbForumComment))]
    public class ClbCommentForumInputDto : ClbForumComment
    {

    }

    [AutoMap(typeof(ClbForumTopic))]
    public class ClbForumTopicInputDto : ClbForumTopic
    {
    }

    public class GetAllClbForumSocialInput : CommonInputDto
    {

        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; }
        public int? Type { get; set; }
        public int? TypeTitle { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
    }

    public class GetAllClbQuestionAnswerInput : GetAllClbForumSocialInput
    {
        public bool? IsAdminAnswered { get; set; }
        public OrderByClbQnA? OrderBy { get; set; }
    }

    public enum OrderByClbQnA
    {
        [FieldName("CreationTime")]
        CREATION_TIME = 1,
        [FieldName("ThreadTitle")]
        THREAD_TITLE = 2,
    }

    public class GetAllClbQnASocialInput : GetAllClbForumSocialInput
    {
        public OrderByQuestionAnswer? OrderBy { get; set; }
        public long? CreatorUserId { get; set; }
        public bool? QuestionAnswered { get; set; }
        public bool? IsAdminAnswered { get; set; }

        public CommonENumForum.FORUM_STATE? State { get; set; }
    }

    public class GetAllClbCommentForumSocialInput : CommonInputDto
    {
        public long? ForumId { get; set; }
    }

    public class GetAllClbForumTopicInput : CommonInputDto
    {
    }

    public class ClbForumExcelInputDto : GetAllClbForumSocialInput
    {
        [CanBeNull] public List<long> Ids;
    }

    public class ClbQuestionAnswerExcelOutputDto : GetAllClbQuestionAnswerInput
    {
        [CanBeNull] public List<long> Ids;
    }

    #region Output
    [AutoMap(typeof(ClbForum))]
    public class ClbForumGetAllDto : ClbForum
    {
        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }
        public int CommentCount { get; set; }
        public int CountLike { get; set; }

    }
    [AutoMap(typeof(ClbForum))]
    public class ClbQuestionAnswerDto: ClbForumGetAllDto
    {
        public bool IsAdminAnswered { get; set; }
    }


    [AutoMap(typeof(ClbForumComment))]
    public class ClbCommentForumDto : ForumComment
    {
        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }
    }
    [AutoMap(typeof(ForumPost))]
    public class ClbExportQuestionAnswerDto: ClbQuestionAnswerDto
    {
        public List<ClbCommentForumDto> Comments { get; set; }
    }
    #endregion
}
