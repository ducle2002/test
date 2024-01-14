using Abp.AutoMapper;
using Yootek.EntityDb;

namespace Yootek.Services
{
    [AutoMap(typeof(CitizenReflectComment))]
    public class CitizenReflectCommentDto : CitizenReflectComment
    {
        public string FullName { get; set; }
        public string ImageUrl { get; set; }
        public long CreatorFeedbackId { get; set; }
    }

}
