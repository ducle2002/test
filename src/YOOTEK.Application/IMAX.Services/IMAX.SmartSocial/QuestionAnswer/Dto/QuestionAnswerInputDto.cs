﻿using Abp.AutoMapper;
using IMAX.Common;
using IMAX.Common.Enum;
using IMAX.EntityDb;
using IMAX.IMAX.EntityDb.Forum;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using static IMAX.IMAXServiceBase;

namespace IMAX.Services.Dto
{

    public class GetAllQuestionAnswerInput : CommonInputDto
    {
        public long? Id { get; set; }
        public int? FormId { get; set; }
        public int? FormCase { get; set; }
        public int? Type { get; set; }
        public int? TypeTitle { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public bool? IsAdminAnswered { get; set; }
        public OrderByQuestionAnswer? OrderBy { get; set; }
    }

    public enum OrderByQuestionAnswer
    {
        [FieldName("CreationTime")]
        CREATION_TIME = 1,
        [FieldName("ThreadTitle")]
        THREAD_TITLE = 2,
    }

    public class GetAllQASocialInput : GetAllQuestionAnswerInput
    {
        public long? CreatorUserId { get; set; }
        public bool? QuestionAnswered { get; set; }

        public CommonENumForum.FORUM_STATE? State { get; set; }
    }

    public class GetAllCommentForumSocialInput : CommonInputDto
    {
        public long? ForumId { get; set; }
    }

    public class GetAllForumTopicInput : CommonInputDto
    {
    }


    public class QuestionAnswerExcelOutputDto : GetAllQuestionAnswerInput
    {
        [CanBeNull] public List<long> Ids;
    }

}
