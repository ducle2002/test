using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.EntityDb
{
    public class ApartmentType: Entity<long>, ICreationAudited, IHasCreationTime, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        [StringLength(512)]
        public string Name { get; set; }
        [StringLength(256)]
        public string Code { get; set; }
        public DateTime CreationTime { get; set; }
        public long? CreatorUserId { get; set; }
    }
}
