using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace Yootek.EntityDb
{
    [Table("MeterMonthlys")]
    public class MeterMonthly : FullAuditedEntity<long>, IMayHaveTenant
    {
        public int? TenantId { get; set; }
        public long? MeterId { get; set; }
        public DateTime? Period { get; set; }
        public int? Value { get; set; }
        public int? FirstValue { get; set; }
        public bool? IsClosed { get; set; }
        [CanBeNull] public string ImageUrl { get; set; }
    }
}