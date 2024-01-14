using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace Yootek.Yootek.EntityDb.Clb.Votes
{
    [Table("ClbUserVotes")]
    public class ClbUserVote : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int State { get; set; }
        [StringLength(256)]
        public string OptionVoteId { get; set; }
        public string Comment { get; set; }
        public long CityVoteId { get; set; }
        public int? TenantId { get; set; }
    }
}