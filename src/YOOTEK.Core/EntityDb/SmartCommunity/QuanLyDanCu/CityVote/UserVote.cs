using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.EntityDb
{
    public class UserVote : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int State { get; set; }
        [StringLength(256)]
        public string OptionVoteId { get; set; }
        public string Comment { get; set; }
        public long CityVoteId { get; set; }
        public int? TenantId { get; set; }
        public string? OptionOther { get; set; }
    }
}
