using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Yootek.EntityDb;
using JetBrains.Annotations;

namespace Yootek.Yootek.EntityDb.Clb.Votes
{
    [Table("ClbCityVotes")]
    public class ClbCityVote : FullAuditedEntity<long>, IMayHaveTenant
    {
        [StringLength(1000)]
        public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        public string Options { get; set; }
        public int? TenantId { get; set; }
        public DateTime FinishTime { get; set; }
        public DateTime StartTime { get; set; }
        public bool? IsShowNumbersVote { get; set; }
        public STATUS_VOTE? Status { get; set; }
    }
}