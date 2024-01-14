using Abp.AutoMapper;
using Yootek.EntityDb;
using System.Collections.Generic;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(QuestionAnswer))]
    public class QuestionAnswerGetAllDto : QuestionAnswer
    {
        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }
        public int CommentCount { get; set; }
        public int CountLike { get; set; }

    }
    [AutoMap(typeof(QuestionAnswer))]
    public class QuestionAnswerDto: QuestionAnswerGetAllDto
    {
        public bool IsAdminAnswered { get; set; }
    }


    [AutoMap(typeof(QAComment))]
    public class CommentQuestionAnswerDto : QAComment
    {
        public string CreatorName { get; set; }
        public string CreatorAvatar { get; set; }
    }
    [AutoMap(typeof(QuestionAnswer))]
    public class ExportQuestionAnswerDto: QuestionAnswerDto
    {
        public List<CommentQuestionAnswerDto> Comments { get; set; }
    }
}
