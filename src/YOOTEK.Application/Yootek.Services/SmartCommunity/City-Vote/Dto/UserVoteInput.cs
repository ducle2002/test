using Abp.AutoMapper;
using Yootek.EntityDb;
using System.ComponentModel.DataAnnotations;

namespace Yootek.Services.Dto
{
    [AutoMap(typeof(UserVote))]
    public class CreateVoteInput
    {
        public long CityVoteId { get; set; }
        public int State { get; set; }
        [StringLength(256)]
        public string OptionVoteId { get; set; }
        public string Comment { get; set; }
        public string? OptionOther { get; set; }
    }

    [AutoMap(typeof(UserVote))]
    public class UpdateVoteInput
    {
        public long Id { get; set; }
        public int State { get; set; }
        [StringLength(256)]
        public string OptionVoteId { get; set; }
        public string Comment { get; set; }
        public string? OptionOther { get; set; }

    }
}
