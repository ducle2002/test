using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Organizations;
using Yootek.Organizations.Interface;
using JetBrains.Annotations;
using System;
using System.ComponentModel.DataAnnotations;

namespace Yootek.EntityDb
{
    public enum STATUS_VOTE
    {
        COMING = 0,
        IN_PROGRESS = 1,
        FINISH = 2
    }

    public class CityVote : FullAuditedEntity<long>, IMayHaveTenant, IMayHaveOrganizationUnit, IMayHaveBuilding, IMayHaveUrban
    {
        [StringLength(1000)]
        public string Name { get; set; }
        [CanBeNull] public string Description { get; set; }
        public string Options { get; set; }
        public int? TenantId { get; set; }
        public DateTime FinishTime { get; set; }
        public DateTime StartTime { get; set; }
        public long? OrganizationUnitId { get; set; }
        public bool? IsShowNumbersVote { get; set; }
        public STATUS_VOTE? Status { get; set; }
        public long? UrbanId { get; set; }
        public long? BuildingId { get; set; }
        public bool? IsOptionOther { get; set; }
    }

}
