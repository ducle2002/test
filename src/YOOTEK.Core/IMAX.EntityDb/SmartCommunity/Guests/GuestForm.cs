using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace Yootek.EntityDb
{
    [Table("GuestForms")]
    public class GuestForm : FullAuditedEntity<long>, IMustHaveTenant
    {
        public int TenantId { get; set; }
        public string Name { get; set; }
        public long UrbanId { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
        [CanBeNull] public string PhoneNumber { get; set; }
        [CanBeNull] public string Address { get; set; }
        [CanBeNull] public string Description { get; set; }
    }
}