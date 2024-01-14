using Abp.AutoMapper;
using Yootek.Common;
using Yootek.EntityDb;
using System;

namespace Yootek.Services
{
    [AutoMap(typeof(CitizenReflect))]
    public class UserFeedbackDto : CitizenReflect
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string ImageUrl { get; set; }
        public long UserId { get; set; }
        public int? CountUnreadComment { get; set; }
        public int? CountAllComment { get; set; }
        public int? Rating { get; set; }
        public string Note { get; set; }
        public string FileOfNote { get; set; }
    }

    public class GetFeedbackInput : CommonInputDto
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int FormId { get; set; }
        public int? State { get; set; }
        public long UserId { get; set; }
        public DateTime? FromDay { get; set; }
        public DateTime? ToDay { get; set; }
        public long? OrganizationUnitId { get; set; }
    }

    public class GetCommentFeedbackInput : CommonInputDto
    {
        public int FeedbackId { get; set; }
    }

    public class GetAllFeedbackByMonth
    {
        public int NumberMonth { get; set; }
    }

    public class StatisticsUserFeedbackInput
    {
        public int? Type { get; set; }
        public int NumberMonth { get; set; }
    }

    public class FeedbackUserInput : CommonInputDto
    {
        //public int? State { get; set; }
        public int FormId { get; set; }

    }

    public class RateFeedbackDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }


}
