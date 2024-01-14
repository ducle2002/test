using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yootek.Yootek.EntityDb.SmartCommunity.Apartment
{
    [Table("ApartmentRentalHistories")]
    public class ApartmentRentalHistory : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long ApartmentId { get; set; }
        public long OwnerId { get; set; }
        public long RentalId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
    }
}
